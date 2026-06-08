using AL71.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AL71.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly AppController _controller;

    [ObservableProperty] private string _activeProfile = "-";
    [ObservableProperty] private string _deviceStatus = "-";
    [ObservableProperty] private string _remapStatus = "-";

    public DashboardViewModel(AppController controller)
    {
        _controller = controller;
        _controller.StatusChanged += (_, _) => Refresh();
        Refresh();
    }

    public void Refresh() => UiThread.Run(() =>
    {
        ActiveProfile = _controller.ActiveProfile?.Name ?? "-";
        DeviceStatus = _controller.IsDeviceConnected ? "Connessa" : "Disconnessa";
        RemapStatus = _controller.IsRemapActive ? "Attivo" : "Disattivo";
    });
}
