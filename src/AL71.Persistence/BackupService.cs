using System.IO.Compression;

namespace AL71.Persistence;

/// <summary>
/// Backup della cartella Profiles (+ settings) in archivi zip dentro Backups.
/// Supporta backup manuali e una semplice politica giornaliera/settimanale.
/// </summary>
public sealed class BackupService
{
    private readonly Action<string>? _log;

    public BackupService(Action<string>? log = null)
    {
        _log = log;
        AppPaths.EnsureCreated();
    }

    /// <summary>Crea un backup immediato; restituisce il percorso dell'archivio.</summary>
    public string CreateBackup(string suffix = "manual")
    {
        AppPaths.EnsureCreated();
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var zipPath = Path.Combine(AppPaths.Backups, $"backup-{stamp}-{suffix}.zip");

        using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        AddFolder(zip, AppPaths.Profiles, "Profiles");
        if (File.Exists(AppPaths.SettingsFile))
            zip.CreateEntryFromFile(AppPaths.SettingsFile, "Settings/settings.json");

        _log?.Invoke($"Backup creato: {zipPath}");
        return zipPath;
    }

    /// <summary>
    /// Esegue un backup giornaliero se non ne esiste già uno per oggi.
    /// Da chiamare all'avvio dell'app.
    /// </summary>
    public void RunDailyIfNeeded()
    {
        var today = DateTime.Now.ToString("yyyyMMdd");
        var exists = Directory.EnumerateFiles(AppPaths.Backups, $"backup-{today}-*.zip").Any();
        if (!exists)
            CreateBackup("daily");
    }

    /// <summary>Mantiene solo gli ultimi <paramref name="keep"/> backup.</summary>
    public void Prune(int keep = 14)
    {
        var files = Directory.GetFiles(AppPaths.Backups, "backup-*.zip")
            .OrderByDescending(File.GetCreationTimeUtc)
            .Skip(keep);

        foreach (var f in files)
        {
            try { File.Delete(f); } catch { /* ignore */ }
        }
    }

    private static void AddFolder(ZipArchive zip, string folder, string entryRoot)
    {
        if (!Directory.Exists(folder))
            return;

        foreach (var file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(folder, file);
            zip.CreateEntryFromFile(file, $"{entryRoot}/{relative.Replace('\\', '/')}");
        }
    }
}
