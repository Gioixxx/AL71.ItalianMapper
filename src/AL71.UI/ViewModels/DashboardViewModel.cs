using System.Linq;
using AL71.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AL71.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly AppController _controller;

    [ObservableProperty] private string _activeProfile = "-";
    [ObservableProperty] private string _deviceStatus = "-";
    [ObservableProperty] private string _remapStatus = "-";
    [ObservableProperty] private string _activeProfileDescription = "";

    /// <summary>Numero di tasti effettivamente rimappati nel profilo attivo.</summary>
    public int MappedKeyCount =>
        _controller.ActiveProfile?.Mappings.Count(m => m.HasAnyMapping) ?? 0;

    public DashboardViewModel(AppController controller)
    {
        _controller = controller;
        _controller.StatusChanged += (_, _) => Refresh();
        Refresh();
    }

    public void Refresh() => UiThread.Run(() =>
    {
        ActiveProfile = _controller.ActiveProfile?.Name ?? "-";
        ActiveProfileDescription = _controller.ActiveProfile?.Description ?? "";
        DeviceStatus = _controller.IsDeviceConnected ? "Connessa" : "Disconnessa";
        RemapStatus = _controller.IsRemapActive ? "Attivo" : "Disattivo";
        OnPropertyChanged(nameof(MappedKeyCount));
    });

    /// <summary>Persiste la descrizione modificata dall'utente.</summary>
    [RelayCommand]
    private void SaveDescription() =>
        _controller.UpdateActiveProfileDescription(ActiveProfileDescription);
}
