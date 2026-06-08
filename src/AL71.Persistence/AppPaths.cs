namespace AL71.Persistence;

/// <summary>
/// Percorsi della cartella di configurazione: <c>%AppData%\AL71LayoutManager\</c>
/// con sottocartelle Profiles / Settings / Logs / Backups.
/// </summary>
public static class AppPaths
{
    public const string AppFolderName = "AL71LayoutManager";

    public static string Root { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        AppFolderName);

    public static string Profiles => Path.Combine(Root, "Profiles");
    public static string Settings => Path.Combine(Root, "Settings");
    public static string Logs => Path.Combine(Root, "Logs");
    public static string Backups => Path.Combine(Root, "Backups");

    public static string SettingsFile => Path.Combine(Settings, "settings.json");

    /// <summary>Crea l'intera struttura di cartelle se mancante.</summary>
    public static void EnsureCreated()
    {
        Directory.CreateDirectory(Profiles);
        Directory.CreateDirectory(Settings);
        Directory.CreateDirectory(Logs);
        Directory.CreateDirectory(Backups);
    }

    public static string ProfileFile(string name) =>
        Path.Combine(Profiles, SanitizeFileName(name) + ".json");

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}
