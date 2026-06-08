using System.Collections.ObjectModel;
using AL71.Core.Input;
using AL71.Core.Models;
using AL71.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AL71.UI.ViewModels;

/// <summary>
/// Strumento diagnostico (Fase 1.1/1.2): cattura tasti (KeyCode/ScanCode/VK) ed
/// elenca le tastiere HID, per scoprire VID/PID dell'AL71 e costruire la mappa tasti.
/// </summary>
public partial class DiagnosticsViewModel : ObservableObject
{
    private readonly AppController _controller;

    [ObservableProperty] private string _lastKey = "Premi un tasto...";
    [ObservableProperty] private DeviceInfo? _selectedDevice;

    public ObservableCollection<DeviceInfo> Devices { get; } = new();

    public DiagnosticsViewModel(AppController controller)
    {
        _controller = controller;
        RefreshDevices();
    }

    /// <summary>Chiamato dalla view su PreviewKeyDown.</summary>
    public void RecordKey(int virtualKey)
    {
        var name = KeyNames.FromVirtualKey(virtualKey);
        var scan = Win32.VirtualKeyToScanCode(virtualKey);
        LastKey = $"PhysicalKey: {name}\nScanCode: {scan}\nVirtualKey: {virtualKey} (0x{virtualKey:X2})";
    }

    [RelayCommand]
    private void RefreshDevices()
    {
        Devices.Clear();
        foreach (var d in _controller.DeviceMonitor.EnumerateKeyboards())
            Devices.Add(d);
    }

    [RelayCommand]
    private void SetAsTarget()
    {
        if (SelectedDevice is null)
            return;
        _controller.SetTargetDevice(SelectedDevice.VendorId, SelectedDevice.ProductId);
    }
}
