using System.Collections.ObjectModel;
using AL71.Core.Input;
using AL71.Core.Models;
using AL71.Layouts;
using AL71.Persistence;
using AL71.UI.Models;
using AL71.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AL71.UI.ViewModels.Wizard;

/// <summary>
/// Procedura guidata: (1) benvenuto, (2) scoperta VID/PID con il metodo
/// scollega/ricollega, (3) cattura della mappa tasti, (4) riepilogo e salvataggio.
/// </summary>
public partial class SetupWizardViewModel : ObservableObject
{
    private readonly AppController _controller;
    private readonly DiagnosticsStore _diagStore = new();

    private IReadOnlyList<DeviceInfo> _snapshot = Array.Empty<DeviceInfo>();
    private readonly List<CaptureKeyViewModel> _sequence = new();
    private int _index;

    public SetupWizardViewModel(AppController controller)
    {
        _controller = controller;
        BuildRows();
    }

    // ---- Navigazione tra i passi -------------------------------------------------
    // 0 = Benvenuto, 1 = Dispositivo, 2 = Tasti, 3 = Riepilogo
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWelcome), nameof(IsDevice), nameof(IsKeys), nameof(IsSummary))]
    [NotifyPropertyChangedFor(nameof(CanGoBack), nameof(CanGoNext), nameof(NextLabel))]
    private int _step;

    public bool IsWelcome => Step == 0;
    public bool IsDevice => Step == 1;
    public bool IsKeys => Step == 2;
    public bool IsSummary => Step == 3;

    public bool CanGoBack => Step > 0;
    public bool CanGoNext => Step < 3;
    public string NextLabel => Step == 3 ? "Fine" : "Avanti";

    [RelayCommand]
    private void Next()
    {
        if (Step == 1 && VendorId == 0 && ProductId == 0)
        {
            DeviceStatus = "Seleziona o rileva la tastiera prima di proseguire.";
            return;
        }

        if (Step < 3)
        {
            Step++;
            if (Step == 2) StartCapture();
            if (Step == 3) BuildSummary();
        }
    }

    [RelayCommand]
    private void Back()
    {
        if (Step > 0) Step--;
    }

    // ---- Passo 2: dispositivo (VID/PID) -----------------------------------------
    public ObservableCollection<DeviceInfo> Devices { get; } = new();

    [ObservableProperty] private DeviceInfo? _selectedDevice;
    [ObservableProperty] private int _vendorId;
    [ObservableProperty] private int _productId;
    [ObservableProperty] private string _detectedDeviceText = "Nessuna tastiera selezionata.";
    [ObservableProperty] private string _deviceStatus =
        "Metodo consigliato: clicca \"1) Fotografa\", poi scollega l'AL71 e clicca \"2) Rileva\".";

    [RelayCommand]
    private void SnapshotDevices()
    {
        _snapshot = _controller.DeviceMonitor.EnumerateKeyboards();
        DeviceStatus = $"Fotografati {_snapshot.Count} dispositivi HID. " +
                       "Ora SCOLLEGA l'AL71 e clicca \"2) Rileva\".";
    }

    [RelayCommand]
    private void DetectByRemoval()
    {
        if (_snapshot.Count == 0)
        {
            DeviceStatus = "Prima clicca \"1) Fotografa\".";
            return;
        }

        var current = _controller.DeviceMonitor.EnumerateKeyboards();
        var removed = _snapshot
            .Where(s => !current.Any(c => c.VendorId == s.VendorId && c.ProductId == s.ProductId))
            .GroupBy(s => (s.VendorId, s.ProductId))
            .Select(g => g.First())
            .ToList();

        if (removed.Count == 0)
        {
            DeviceStatus = "Nessun dispositivo rimosso rilevato. Scollega l'AL71 e riprova.";
            return;
        }

        if (removed.Count == 1)
        {
            SetDetected(removed[0]);
            DeviceStatus = "Tastiera identificata! Ricollega pure l'AL71 e clicca Avanti.";
            return;
        }

        // Più dispositivi spariti: mostra la lista per la scelta manuale.
        Devices.Clear();
        foreach (var d in removed) Devices.Add(d);
        DeviceStatus = "Più dispositivi rimossi: selezionane uno qui sotto e clicca \"Usa selezionata\".";
    }

    [RelayCommand]
    private void RefreshDevices()
    {
        Devices.Clear();
        foreach (var d in _controller.DeviceMonitor.EnumerateKeyboards())
            Devices.Add(d);
        DeviceStatus = "Elenco aggiornato. Seleziona l'AL71 e clicca \"Usa selezionata\".";
    }

    [RelayCommand]
    private void UseSelectedDevice()
    {
        if (SelectedDevice is null)
        {
            DeviceStatus = "Nessuna voce selezionata nell'elenco.";
            return;
        }
        SetDetected(SelectedDevice);
        DeviceStatus = "Tastiera selezionata. Clicca Avanti.";
    }

    private void SetDetected(DeviceInfo d)
    {
        VendorId = d.VendorId;
        ProductId = d.ProductId;
        DetectedDeviceText = $"{d.DeviceName}  —  VID=0x{d.VendorId:X4}  PID=0x{d.ProductId:X4}";
    }

    // ---- Passo 3: cattura mappa tasti -------------------------------------------
    public ObservableCollection<CaptureRow> Rows { get; } = new();

    [ObservableProperty] private string _keyPrompt = "";
    [ObservableProperty] private string _progressText = "";
    [ObservableProperty] private string _lastCapture = "";

    private void BuildRows()
    {
        Rows.Clear();
        _sequence.Clear();
        foreach (var row in KeyboardLayoutDefinition.Rows)
        {
            var keys = row.Keys.Select(k => new CaptureKeyViewModel
            {
                PhysicalKey = k.PhysicalKey,
                Label = k.Label,
                Width = k.Width
            }).ToList();

            _sequence.AddRange(keys);
            Rows.Add(new CaptureRow { Keys = keys });
        }
    }

    private void StartCapture()
    {
        foreach (var k in _sequence)
        {
            k.IsCurrent = false;
            k.IsCaptured = false;
            k.CapturedName = null;
        }
        _index = 0;
        UpdateCurrent();
    }

    /// <summary>Chiamato dalla finestra su PreviewKeyDown durante il passo Tasti.</summary>
    public void RecordKey(int virtualKey)
    {
        if (Step != 2 || _index >= _sequence.Count)
            return;

        var key = _sequence[_index];
        key.VkCode = virtualKey;
        key.ScanCode = Win32.VirtualKeyToScanCode(virtualKey);
        key.CapturedName = KeyNames.FromVirtualKey(virtualKey);
        key.IsCaptured = true;

        LastCapture = $"{key.Label} → PhysicalKey={key.CapturedName}, ScanCode={key.ScanCode}, " +
                      $"VK={virtualKey} (0x{virtualKey:X2})";

        Advance();
    }

    [RelayCommand]
    private void SkipKey() => Advance();

    [RelayCommand]
    private void PreviousKey()
    {
        if (_index > 0)
        {
            _sequence[_index].IsCurrent = false;
            _index--;
            UpdateCurrent();
        }
    }

    [RelayCommand]
    private void RestartCapture() => StartCapture();

    private void Advance()
    {
        if (_index < _sequence.Count)
            _sequence[_index].IsCurrent = false;
        _index++;
        UpdateCurrent();
    }

    private void UpdateCurrent()
    {
        var captured = _sequence.Count(k => k.IsCaptured);
        ProgressText = $"{captured}/{_sequence.Count} tasti catturati";

        if (_index < _sequence.Count)
        {
            var key = _sequence[_index];
            key.IsCurrent = true;
            KeyPrompt = $"Premi sull'AL71 il tasto evidenziato: \"{key.Label}\"";
        }
        else
        {
            KeyPrompt = "Tutti i tasti elaborati. Clicca Avanti per il riepilogo.";
        }
    }

    // ---- Passo 4: riepilogo e salvataggio ---------------------------------------
    [ObservableProperty] private string _summaryText = "";
    [ObservableProperty] private bool _saved;

    private void BuildSummary()
    {
        var captured = _sequence.Count(k => k.IsCaptured);
        SummaryText =
            $"Dispositivo: {DetectedDeviceText}\n" +
            $"Tasti catturati: {captured}/{_sequence.Count}\n\n" +
            "Clicca \"Salva\" per scrivere device.json e keymap-al71.json e impostare l'AL71 come tastiera target.";
        Saved = false;
    }

    [RelayCommand]
    private void Save()
    {
        var device = new DeviceInfo
        {
            VendorId = VendorId,
            ProductId = ProductId,
            DeviceName = DetectedDeviceText
        };
        var devicePath = _diagStore.SaveDevice(device);
        var mapPath = _diagStore.SaveKeyMap(BuildCapturedMap());

        _controller.SetTargetDevice(VendorId, ProductId);

        Saved = true;
        SummaryText =
            $"Salvato!\n\nVID/PID impostati: 0x{VendorId:X4} / 0x{ProductId:X4}\n" +
            $"• {devicePath}\n• {mapPath}\n\n" +
            "Ora puoi generare il profilo italiano dalla mappa, oppure chiudere.";
    }

    /// <summary>Genera e attiva il profilo "Italiano (AL71)" dalla mappa catturata.</summary>
    [RelayCommand]
    private void GenerateProfile()
    {
        var profile = ProfileFactory.BuildItalianFromKeymap(BuildCapturedMap(), out var missing);
        _controller.ImportProfile(profile, activate: true);

        var note = missing.Count == 0
            ? "Tutti i tasti del layout di base sono coperti."
            : $"Tasti del layout non ancora catturati ({missing.Count}): {string.Join(", ", missing)}";

        SummaryText =
            $"Profilo \"{profile.Name}\" generato e attivato " +
            $"({profile.Mappings.Count} tasti mappati).\n\n{note}\n\n" +
            "Puoi chiudere la procedura guidata.";
    }

    private Dictionary<string, KeyScanInfo> BuildCapturedMap()
    {
        var map = new Dictionary<string, KeyScanInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var k in _sequence.Where(k => k.IsCaptured))
            map[k.PhysicalKey] = new KeyScanInfo { ScanCode = k.ScanCode, VkCode = k.VkCode };
        return map;
    }
}
