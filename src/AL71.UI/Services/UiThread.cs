using System.Windows;

namespace AL71.UI.Services;

/// <summary>Esegue azioni sul thread della UI (gli eventi del controller arrivano da altri thread).</summary>
public static class UiThread
{
    public static void Run(Action action)
    {
        var app = Application.Current;
        if (app is null)
        {
            action();
            return;
        }

        if (app.Dispatcher.CheckAccess())
            action();
        else
            app.Dispatcher.Invoke(action);
    }
}
