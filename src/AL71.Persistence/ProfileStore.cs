using System.Text.Json;
using AL71.Core.Abstractions;
using AL71.Core.Models;
using AL71.Core.Serialization;

namespace AL71.Persistence;

/// <summary>Persistenza dei profili come file JSON in <see cref="AppPaths.Profiles"/>.</summary>
public sealed class ProfileStore : IProfileStore
{
    public ProfileStore() => AppPaths.EnsureCreated();

    public IReadOnlyList<string> ListProfiles()
    {
        if (!Directory.Exists(AppPaths.Profiles))
            return Array.Empty<string>();

        return Directory.GetFiles(AppPaths.Profiles, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public KeyboardProfile Load(string name)
    {
        var path = AppPaths.ProfileFile(name);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Profilo non trovato: {name}", path);

        return ReadFile(path);
    }

    public void Save(KeyboardProfile profile)
    {
        AppPaths.EnsureCreated();
        WriteFile(AppPaths.ProfileFile(profile.Name), profile);
    }

    public void Delete(string name)
    {
        var path = AppPaths.ProfileFile(name);
        if (File.Exists(path))
            File.Delete(path);
    }

    public void Export(KeyboardProfile profile, string filePath) => WriteFile(filePath, profile);

    public KeyboardProfile Import(string filePath) => ReadFile(filePath);

    private static KeyboardProfile ReadFile(string path)
    {
        var json = File.ReadAllText(path);
        var profile = JsonSerializer.Deserialize<KeyboardProfile>(json, JsonDefaults.Options)
            ?? throw new InvalidDataException($"JSON profilo non valido: {path}");

        // Verifica integrità di base (Fase 17).
        if (string.IsNullOrWhiteSpace(profile.Name))
            throw new InvalidDataException($"Profilo senza nome: {path}");

        return profile;
    }

    private static void WriteFile(string path, KeyboardProfile profile)
    {
        var json = JsonSerializer.Serialize(profile, JsonDefaults.Options);

        // Scrittura atomica: file temporaneo + replace, per non corrompere in caso di crash.
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, json);
        if (File.Exists(path))
            File.Replace(tmp, path, null);
        else
            File.Move(tmp, path);
    }
}
