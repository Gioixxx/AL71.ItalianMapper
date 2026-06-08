using System.Text.Json;
using AL71.Core.Abstractions;
using AL71.Core.Models;
using AL71.Core.Serialization;

namespace AL71.Persistence;

/// <summary>Persistenza delle impostazioni globali in <see cref="AppPaths.SettingsFile"/>.</summary>
public sealed class SettingsStore : ISettingsStore
{
    public SettingsStore() => AppPaths.EnsureCreated();

    public AppSettings Load()
    {
        var path = AppPaths.SettingsFile;
        if (!File.Exists(path))
            return new AppSettings();

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonDefaults.Options) ?? new AppSettings();
        }
        catch
        {
            // Recupero automatico (Fase 17): se il file è corrotto, riparti dai default.
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        AppPaths.EnsureCreated();
        var json = JsonSerializer.Serialize(settings, JsonDefaults.Options);
        var tmp = AppPaths.SettingsFile + ".tmp";
        File.WriteAllText(tmp, json);
        if (File.Exists(AppPaths.SettingsFile))
            File.Replace(tmp, AppPaths.SettingsFile, null);
        else
            File.Move(tmp, AppPaths.SettingsFile);
    }
}
