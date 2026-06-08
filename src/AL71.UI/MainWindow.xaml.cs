using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using AL71.UI.Services;
using AL71.UI.ViewModels;

namespace AL71.UI;

public partial class MainWindow : Window
{
    private const int HotkeyId = 0xA171;

    private bool _reallyExit;
    private bool _shutdownInitiated;
    private HwndSource? _source;
    private IntPtr _hwnd;

    public MainWindow() => InitializeComponent();

    private MainViewModel? Vm => DataContext as MainViewModel;

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Scorciatoia globale Ctrl+Alt+M per attivare/disattivare la mappatura.
        _hwnd = new WindowInteropHelper(this).Handle;
        _source = HwndSource.FromHwnd(_hwnd);
        _source?.AddHook(WndProc);
        Win32.RegisterHotKey(_hwnd, HotkeyId,
            Win32.MOD_CONTROL | Win32.MOD_ALT | Win32.MOD_NOREPEAT, 0x4D /* M */);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == Win32.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            Vm?.ToggleRemapCommand.Execute(null);
            handled = true;
        }
        return IntPtr.Zero;
    }

    private void OnTrayOpen(object sender, RoutedEventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void OnTrayToggle(object sender, RoutedEventArgs e) => Vm?.ToggleRemapCommand.Execute(null);

    private void OnTrayExit(object sender, RoutedEventArgs e)
    {
        _reallyExit = true;
        Close();
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
        // Con "Riduci nella tray" attivo, la X nasconde la finestra invece di uscire.
        if (!_reallyExit && Vm?.Settings.MinimizeToTray == true)
        {
            e.Cancel = true;
            Hide();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Chiusura reale: rilascia hotkey e tray, poi termina l'applicazione
        // (siamo in ShutdownMode=OnExplicitShutdown, niente chiusura automatica).
        if (_hwnd != IntPtr.Zero)
            Win32.UnregisterHotKey(_hwnd, HotkeyId);
        _source?.RemoveHook(WndProc);

        Tray?.Dispose();

        base.OnClosed(e);

        if (!_shutdownInitiated)
        {
            _shutdownInitiated = true;
            Application.Current.Shutdown();
        }
    }
}
