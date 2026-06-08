using AL71.Core.Models;

namespace AL71.Core.Abstractions;

/// <summary>
/// Motore di remapping. Riceve i tasti grezzi dall'hook, decide il layer attivo,
/// cerca la mappatura e indica se sopprimere l'originale (iniettando il sostituto).
/// Pensato per essere veloce (lookup in cache) e thread-safe.
/// </summary>
public interface IRemapEngine
{
    /// <summary>Profilo attualmente caricato; impostarlo ricostruisce la cache di lookup.</summary>
    KeyboardProfile? ActiveProfile { get; set; }

    /// <summary>Se false, il motore lascia passare tutto inalterato.</summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Processa un tasto. Restituisce <c>true</c> se l'evento originale va soppresso
    /// (perché è stato gestito/sostituito dal motore), <c>false</c> per lasciarlo passare.
    /// </summary>
    bool ProcessKey(in LowLevelKeyInfo info);

    /// <summary>Alzato quando un tasto viene effettivamente rimappato.</summary>
    event EventHandler<KeyRemappedEventArgs>? KeyRemapped;
}
