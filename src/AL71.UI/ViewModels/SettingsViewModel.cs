using AL71.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AL71.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppController _controller;

    [ObservableProperty] private bool _runAtStartup;
    [ObservableProperty] private bool _remapEnabled;
    [ObservableProperty] private bool _onlyWhenDeviceConnected;
    [ObservableProperty] private bool _minimizeToTray;

    public SettingsViewModel(AppController controller)
    {
        _controller = controller;
        _runAtStartup = AutoStartService.IsEnabled();
        _remapEnabled = controller.Settings.RemapEnabled;
        _onlyWhenDeviceConnected = controller.Settings.OnlyWhenDeviceConnected;
        _minimizeToTray = controller.Settings.MinimizeToTray;
    }

    public string DeviceInfoText =>
        _controller.Settings.DeviceVendorId == 0 && _controller.Settings.DeviceProductId == 0
            ? "Nessun dispositivo selezionato — usa la scheda Diagnostica."
            : $"VID=0x{_controller.Settings.DeviceVendorId:X4}  PID=0x{_controller.Settings.DeviceProductId:X4}";

    partial void OnRunAtStartupChanged(bool value)
    {
        AutoStartService.SetEnabled(value);
        _controller.Settings.RunAtStartup = value;
        _controller.SaveSettings();
    }

    partial void OnRemapEnabledChanged(bool value) => _controller.RemapEnabled = value;

    partial void OnOnlyWhenDeviceConnectedChanged(bool value)
    {
        _controller.Settings.OnlyWhenDeviceConnected = value;
        _controller.SaveSettings();
    }

    partial void OnMinimizeToTrayChanged(bool value)
    {
        _controller.Settings.MinimizeToTray = value;
        _controller.SaveSettings();
    }
}
