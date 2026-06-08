using System.Reflection;
using System.Text.Json;
using AL71.Core.Models;
using AL71.Core.Serialization;

namespace AL71.Layouts;

/// <summary>
/// Carica il profilo del layout italiano incluso come risorsa embedded.
/// Usato per creare il profilo predefinito al primo avvio.
/// </summary>
public static class ItalianLayout
{
    private const string ResourceName = "AL71.Layouts.Resources.italian.json";

    /// <summary>Restituisce una nuova istanza del profilo "Italiano" di base.</summary>
    public static KeyboardProfile CreateDefault()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Risorsa layout non trovata: {ResourceName}");

        var profile = JsonSerializer.Deserialize<KeyboardProfile>(stream, JsonDefaults.Options);
        return profile ?? throw new InvalidOperationException("Impossibile deserializzare il layout italiano.");
    }
}
