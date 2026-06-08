using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AL71.UI.ViewModels;

namespace AL71.UI;

public partial class MainWindow : Window
{
    private bool _reallyExit;

    public MainWindow() => InitializeComponent();

    private MainViewModel? Vm => DataContext as MainViewModel;

    private void OnTrayOpen(object sender, RoutedEventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void OnTrayToggle(object sender, RoutedEventArgs e)
    {
        if (Vm is not null)
            Vm.Settings.RemapEnabled = !Vm.Settings.RemapEnabled;
    }

    private void OnTrayExit(object sender, RoutedEventArgs e)
    {
        _reallyExit = true;
        Application.Current.Shutdown();
    }

    private void OnOpenWizard(object sender, RoutedEventArgs e)
    {
        if (Vm is null)
            return;

        var wizard = new SetupWizard(Vm.Controller) { Owner = this };
        wizard.ShowDialog();
        Vm.OnWizardClosed();
    }

    private void OnCaptureFocus(object sender, MouseButtonEventArgs e) => KeyCaptureArea.Focus();

    private void OnCaptureKey(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var vk = KeyInterop.VirtualKeyFromKey(key);
        Vm?.Diagnostics.RecordKey(vk);
        e.Handled = true;
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        // Riduci nella tray invece di uscire, se richiesto.
        if (!_reallyExit && Vm?.Settings.MinimizeToTray == true)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
