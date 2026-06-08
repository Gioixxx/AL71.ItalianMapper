using System.Collections.ObjectModel;
using AL71.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AL71.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AppController _controller;
    private bool _suppressProfileChange;

    public DashboardViewModel Dashboard { get; }
    public EditorViewModel Editor { get; }
    public DiagnosticsViewModel Diagnostics { get; }
    public SettingsViewModel Settings { get; }

    /// <summary>Esposto per consentire alla view di avviare la procedura guidata.</summary>
    public AppController Controller => _controller;

    /// <summary>Da chiamare dopo la chiusura della procedura guidata per aggiornare lo stato.</summary>
    public void OnWizardClosed() => Dashboard.Refresh();

    public ObservableCollection<string> Profiles { get; } = new();

    [ObservableProperty] private string? _selectedProfile;

    public MainViewModel(AppController controller)
    {
        _controller = controller;
        Dashboard = new DashboardViewModel(controller);
        Editor = new EditorViewModel(controller);
        Diagnostics = new DiagnosticsViewModel(controller);
        Settings = new SettingsViewModel(controller);

        ReloadProfiles();
    }

    public void ReloadProfiles()
    {
        _suppressProfileChange = true;
        Profiles.Clear();
        foreach (var p in _controller.ListProfiles())
            Profiles.Add(p);
        SelectedProfile = _controller.ActiveProfile?.Name;
        _suppressProfileChange = false;
    }

    partial void OnSelectedProfileChanged(string? value)
    {
        if (_suppressProfileChange || string.IsNullOrEmpty(value))
            return;

        _controller.SwitchProfile(value);
        Dashboard.Refresh();
    }
}
