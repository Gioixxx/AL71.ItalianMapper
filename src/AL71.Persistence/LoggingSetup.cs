using Serilog;
using Serilog.Core;

namespace AL71.Persistence;

/// <summary>Configurazione di Serilog: file giornaliero rotante in Logs/.</summary>
public static class LoggingSetup
{
    /// <summary>Crea il logger e lo imposta come <see cref="Log.Logger"/> globale.</summary>
    public static Logger Configure()
    {
        AppPaths.EnsureCreated();

        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: Path.Combine(AppPaths.Logs, "log-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Logger = logger;
        return logger;
    }
}
