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

    /// <summary>Aggiorna la descrizione del profilo attivo e la persiste.</summary>
    public void UpdateActiveProfileDescription(string? description)
    {
        if (ActiveProfile is null)
            return;
        ActiveProfile.Description = description ?? string.Empty;
        _profileStore.Save(ActiveProfile);
        Log.Information("Descrizione aggiornata per il profilo: {Profile}", ActiveProfile.Name);
    }

    /// <summary>Ripristina le mappature del profilo attivo al layout italiano di base (mantiene nome e descrizione).</summary>
    public void ResetActiveProfileToItalian()
    {
        if (ActiveProfile is null)
            return;

        var italian = ItalianLayout.CreateDefault();
        ActiveProfile.Mappings = italian.Mappings;
        ActiveProfile.Macros = italian.Macros;
        _profileStore.Save(ActiveProfile);
        _engine.ActiveProfile = ActiveProfile;
        Log.Information("Profilo '{Profile}' ripristinato al layout italiano.", ActiveProfile.Name);
        RaiseStatusChanged();
    }

    /// <summary>Salva un profilo (es. generato dalla mappa tasti) e opzionalmente lo attiva.</summary>
    public void ImportProfile(KeyboardProfile profile, bool activate)
    {
        _profileStore.Save(profile);
        Log.Information("Profilo importato/generato: {Profile}", profile.Name);
        if (activate)
            SwitchProfile(profile.Name);
    }

    /// <summary>Crea un nuovo profilo vuoto, lo salva e lo attiva.</summary>
    public string CreateProfile(string name)
    {
        name = EnsureUniqueName(name);
        var profile = new KeyboardProfile { Name = name };
        _profileStore.Save(profile);
        Log.Information("Profilo creato: {Profile}", name);
        SwitchProfile(name);
        return name;
    }

    /// <summary>Duplica un profilo esistente con un nuovo nome e attiva la copia.</summary>
    public string DuplicateProfile(string sourceName, string newName)
    {
        var copy = _profileStore.Load(sourceName);
        copy.Name = EnsureUniqueName(newName);
        _profileStore.Save(copy);
        Log.Information("Profilo duplicato: {Source} -> {Copy}", sourceName, copy.Name);
        SwitchProfile(copy.Name);
        return copy.Name;
    }

    /// <summary>Rinomina un profilo (crea il file col nuovo nome ed elimina il vecchio).</summary>
    public string RenameProfile(string oldName, string newName)
    {
        newName = newName.Trim();
        if (string.Equals(oldName, newName, StringComparison.Ordinal))
            return oldName;

        var profile = _profileStore.Load(oldName);
        profile.Name = EnsureUniqueName(newName);
        _profileStore.Save(profile);
        _profileStore.Delete(oldName);
        Log.Information("Profilo rinominato: {Old} -> {New}", oldName, profile.Name);
        SwitchProfile(profile.Name);
        return profile.Name;
    }

    /// <summary>Elimina un profilo. Rifiuta se è l'unico rimasto; se è quello attivo passa a un altro.</summary>
    public void DeleteProfile(string name)
    {
        if (_profileStore.ListProfiles().Count <= 1)
            throw new InvalidOperationException("Non puoi eliminare l'unico profilo rimasto.");

        _profileStore.Delete(name);
        Log.Information("Profilo eliminato: {Profile}", name);

        if (string.Equals(ActiveProfile?.Name, name, StringComparison.OrdinalIgnoreCase))
            SwitchProfile(_profileStore.ListProfiles().First());
    }

    /// <summary>Esporta il profilo attivo in un file JSON arbitrario.</summary>
    public void ExportActiveProfile(string filePath)
    {
        if (ActiveProfile is null)
            return;
        _profileStore.Export(ActiveProfile, filePath);
        Log.Information("Profilo esportato: {Profile} -> {Path}", ActiveProfile.Name, filePath);
    }

    /// <summary>Importa un profilo da un file JSON, evitando di sovrascrivere quelli esistenti, e lo attiva.</summary>
    public string ImportProfileFromFile(string filePath)
    {
        var profile = _profileStore.Import(filePath);
        profile.Name = EnsureUniqueName(profile.Name);
        _profileStore.Save(profile);
        Log.Information("Profilo importato da file: {Path} -> {Profile}", filePath, profile.Name);
        SwitchProfile(profile.Name);
        return profile.Name;
    }

    /// <summary>Rende il nome univoco aggiungendo un suffisso numerico se già usato.</summary>
    private string EnsureUniqueName(string name)
    {
        name = string.IsNullOrWhiteSpace(name) ? "Nuovo profilo" : name.Trim();
        var existing = _profileStore.ListProfiles();
        if (!existing.Any(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase)))
            return name;

        for (var i = 2; ; i++)
        {
            var candidate = $"{name} ({i})";
            if (!existing.Any(n => string.Equals(n, candidate, StringComparison.OrdinalIgnoreCase)))
                return candidate;
        }
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

    /// <summary>Sospende temporaneamente il remap (es. durante la cattura tasti) senza toccare le impostazioni.</summary>
    public void SuspendRemap()
    {
        _engine.Enabled = false;
        RaiseStatusChanged();
    }

    /// <summary>Ripristina lo stato del remap in base a impostazioni e dispositivo.</summary>
    public void ResumeRemap() => ApplyEnabledState();

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
