namespace AL71.Core.Models;

/// <summary>
/// Rimappatura di un singolo tasto fisico sui vari layer.
/// I valori sono le stringhe da emettere (uno o più caratteri Unicode);
/// <c>null</c> o stringa vuota significa "lascia passare il tasto originale".
/// </summary>
public sealed class KeyMapping
{
    /// <summary>Identificatore del tasto fisico, es. "OEM_1", "A", "SPACE".</summary>
    public string PhysicalKey { get; set; } = string.Empty;

    public string? Normal { get; set; }
    public string? Shift { get; set; }
    public string? AltGr { get; set; }
    public string? ShiftAltGr { get; set; }

    /// <summary>Macro opzionale legata al layer Fn (per trigger su questo tasto).</summary>
    public string? Fn { get; set; }

    /// <summary>Restituisce il valore per il layer richiesto, o null se non mappato.</summary>
    public string? ForLayer(KeyboardLayer layer) => layer switch
    {
        KeyboardLayer.Normal => Normal,
        KeyboardLayer.Shift => Shift,
        KeyboardLayer.AltGr => AltGr,
        KeyboardLayer.ShiftAltGr => ShiftAltGr,
        KeyboardLayer.Fn => Fn,
        _ => null
    };

    public bool HasAnyMapping =>
        !string.IsNullOrEmpty(Normal) ||
        !string.IsNullOrEmpty(Shift) ||
        !string.IsNullOrEmpty(AltGr) ||
        !string.IsNullOrEmpty(ShiftAltGr) ||
        !string.IsNullOrEmpty(Fn);
}
