using AL71.Core.Abstractions;
using AL71.Core.Models;
using AL71.Device;
using AL71.Hook;
using AL71.Layouts;
using AL71.Persistence;
using Serilog;

namespace AL71.UI.Services;

/// <summary>
/// Punto di composizione e orchestrazione: tiene insieme motore di remap, hook,
/// monitoraggio dispositivi e persistenza, restando indipendente dalla UI.
/// La UI osserva lo stato tramite <see cref="StatusChanged"/>.
/// </summary>
public sealed class AppController : IDisposable
{
    private readonly ISettingsStore _settingsStore;
    private readonly IProfileStore _profileStore;
    private readonly IRemapEngine _engine;
    private readonly IKeyboardHookService _hook;
    private readonly IDeviceMonitor _deviceMonitor;
    private readonly BackupService _backup;

    public AppController()
    {
        _settingsStore = new SettingsStore();
        _profileStore = new ProfileStore();
        _backup = new BackupService(s => Log.Information("{Msg}", s));

        var injector = new KeyInjector();
        _engine = new RemapEngine(injector);
        _engine.KeyRemapped += (_, e) =>
            Log.Information("Tasto rimappato {Key} [{Layer}] -> {Out}", e.PhysicalKey, e.Layer, e.Output);

        _hook = new KeyboardHookService(_engine, s => Log.Information("{Msg}", s));
        _deviceMonitor = new DeviceMonitor(s => Log.Information("{Msg}", s));
    }

    public AppSettings Settings { get; private set; } = new();

    public KeyboardProfile? ActiveProfile { get; private set; }

    public bool IsDeviceConnected => _deviceMonitor.IsTargetConnected;

    public IDeviceMonitor DeviceMonitor => _deviceMonitor;

    /// <summary>Stato effettivo del remap (considerando device e impostazioni).</summary>
    public bool IsRemapActive => _engine.Enabled;

    public event EventHandler? StatusChanged;

    public void Initialize()
    {
        AppPaths.EnsureCreated();
        Settings = _settingsStore.Load();

        EnsureDefaultProfile();
        ActiveProfile = LoadProfileSafe(Settings.ActiveProfile);
        _engine.ActiveProfile = ActiveProfile;

        _deviceMonitor.Connected += OnDeviceChanged;
        _deviceMonitor.Disconnected += OnDeviceChanged;
        _deviceMonitor.Start(Settings.DeviceVendorId, Settings.DeviceProductId);

        _hook.Start();
        ApplyEnabledState();

        _backup.RunDailyIfNeeded();
        _backup.Prune();

        Log.Information("Avvio completato. Profilo attivo: {Profile}", ActiveProfile?.Name);
    }

    public IReadOnlyList<string> ListProfiles() => _profileStore.ListProfiles();

    public void SwitchProfile(string name)
    {
        ActiveProfile = LoadProfileSafe(name);
        _engine.ActiveProfile = ActiveProfile;
        Settings.ActiveProfile = name;
        _settingsStore.Save(Settings);
        Log.Information("Profilo cambiato: {Profile}", name);
        RaiseStatusChanged();
    }

    public void SaveActiveProfile()
    {
        if (ActiveProfile is null)
            return;
        _profileStore.Save(ActiveProfile);
        _engine.ActiveProfile = ActiveProfile; // ricostruisce la cache di lookup
        Log.Information("Profilo salvato: {Profile}", ActiveProfile.Name);
    }

    public bool RemapEnabled
    {
        get => Settings.RemapEnabled;
        set
        {
            Settings.RemapEnabled = value;
            _settingsStore.Save(Settings);
            ApplyEnabledState();
        }
    }

    public void SaveSettings() => _settingsStore.Save(Settings);

    /// <summary>Imposta il VID/PID rilevato della tastiera e riavvia il monitoraggio.</summary>
    public void SetTargetDevice(int vendorId, int productId)
    {
        Settings.DeviceVendorId = vendorId;
        Settings.DeviceProductId = productId;
        _settingsStore.Save(Settings);
        _deviceMonitor.Stop();
        _deviceMonitor.Start(vendorId, productId);
        ApplyEnabledState();
        RaiseStatusChanged();
    }

    private void OnDeviceChanged(object? sender, DeviceInfo e)
    {
        ApplyEnabledState();
        RaiseStatusChanged();
    }

    private void ApplyEnabledState()
    {
        var deviceOk = !Settings.OnlyWhenDeviceConnected || _deviceMonitor.IsTargetConnected;
        _engine.Enabled = Settings.RemapEnabled && deviceOk;
        RaiseStatusChanged();
    }

    private void EnsureDefaultProfile()
    {
        if (_profileStore.ListProfiles().Count == 0)
        {
            var italian = ItalianLayout.CreateDefault();
            _profileStore.Save(italian);
            Settings.ActiveProfile = italian.Name;
            _settingsStore.Save(Settings);
            Log.Information("Creato profilo predefinito: {Profile}", italian.Name);
        }
    }

    private KeyboardProfile LoadProfileSafe(string name)
    {
        try
        {
            return _profileStore.Load(name);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Profilo '{Name}' non caricabile, uso il layout italiano.", name);
            var fallback = ItalianLayout.CreateDefault();
            return fallback;
        }
    }

    private void RaiseStatusChanged() => StatusChanged?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _hook.Dispose();
        _deviceMonitor.Dispose();
        Log.CloseAndFlush();
    }
}
