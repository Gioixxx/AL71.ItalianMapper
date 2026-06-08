using AL71.Core.Models;

namespace AL71.Layouts;

/// <summary>
/// Costruisce profili a partire dal layout italiano di base e dalla mappa tasti
/// catturata dalla diagnostica (keymap-al71.json).
/// </summary>
public static class ProfileFactory
{
    /// <summary>
    /// Genera un profilo italiano limitato/validato sui tasti effettivamente presenti
    /// nella mappa catturata. I tasti del layout di base non presenti nella mappa
    /// vengono elencati in <paramref name="missing"/> (per segnalarli all'utente).
    /// Se la mappa è vuota, restituisce il layout di base completo.
    /// </summary>
    public static KeyboardProfile BuildItalianFromKeymap(
        IReadOnlyDictionary<string, KeyScanInfo> keymap,
        out List<string> missing)
    {
        var baseProfile = ItalianLayout.CreateDefault();
        missing = new List<string>();

        var profile = new KeyboardProfile
        {
            Name = "Italiano (AL71)",
            Description = "Layout italiano generato dalla mappa tasti catturata dell'AL71.",
            Macros = baseProfile.Macros
        };

        if (keymap.Count == 0)
        {
            profile.Mappings = baseProfile.Mappings;
            return profile;
        }

        foreach (var mapping in baseProfile.Mappings)
        {
            if (keymap.ContainsKey(mapping.PhysicalKey))
                profile.Mappings.Add(mapping);
            else
                missing.Add(mapping.PhysicalKey);
        }

        return profile;
    }
}
