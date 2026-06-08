using System.Windows;
using AL71.Persistence;
using AL71.UI.Services;
using AL71.UI.ViewModels;
using Serilog;

namespace AL71.UI;

public partial class App : Application
{
    private AppController? _controller;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        LoggingSetup.Configure();
        Log.Information("=== AL71 Italian Layout Manager in avvio ===");

        DispatcherUnhandledException += (_, args) =>
        {
            Log.Error(args.Exception, "Eccezione UI non gestita");
            args.Handled = true;
        };

        _controller = new AppController();
        try
        {
            _controller.Initialize();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Errore durante l'inizializzazione");
        }

        var vm = new MainViewModel(_controller);
        var window = new MainWindow { DataContext = vm };
        MainWindow = window;

        var startMinimized = e.Args.Contains("--minimized");
        window.Show();
        if (startMinimized && _controller.Settings.MinimizeToTray)
            window.Hide();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _controller?.Dispose();
        Log.Information("=== Chiusura ===");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
