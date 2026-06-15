using System.Windows;

namespace AL71.UI;

/// <summary>Semplice dialog di input testuale (es. nome del profilo).</summary>
public partial class PromptDialog : Window
{
    private PromptDialog(string title, string message, string initial)
    {
        InitializeComponent();
        Title = title;
        PromptText.Text = message;
        InputBox.Text = initial;
        Loaded += (_, _) =>
        {
            InputBox.SelectAll();
            InputBox.Focus();
        };
    }

    /// <summary>Mostra il dialog e restituisce il testo inserito, o null se annullato/vuoto.</summary>
    public static string? Ask(Window owner, string title, string message, string initial = "")
    {
        var dialog = new PromptDialog(title, message, initial) { Owner = owner };
        return dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputBox.Text)
            ? dialog.InputBox.Text.Trim()
            : null;
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
