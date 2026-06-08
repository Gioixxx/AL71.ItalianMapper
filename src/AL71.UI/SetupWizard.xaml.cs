using System.Windows;
using System.Windows.Input;
using AL71.UI.Services;
using AL71.UI.ViewModels.Wizard;

namespace AL71.UI;

public partial class SetupWizard : Window
{
    private readonly AppController _controller;

    public SetupWizard(AppController controller)
    {
        _controller = controller;
        InitializeComponent();
        DataContext = new SetupWizardViewModel(controller);

        // Sospendi il remap mentre catturiamo i tasti, ripristina alla chiusura.
        Loaded += (_, _) => _controller.SuspendRemap();
        Closed += (_, _) => _controller.ResumeRemap();
    }

    private SetupWizardViewModel? Vm => DataContext as SetupWizardViewModel;

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Vm is null || !Vm.IsKeys || e.IsRepeat)
            return;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var vk = KeyInterop.VirtualKeyFromKey(key);
        Vm.RecordKey(vk);
        e.Handled = true; // evita che il tasto sposti il focus o attivi i pulsanti
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
