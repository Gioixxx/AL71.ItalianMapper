namespace AL71.Core.Input;

/// <summary>
/// Conversione tra Virtual-Key code di Windows e l'identificatore "fisico" del tasto
/// usato nei layout (es. "OEM_1", "A", "SPACE"). Condiviso tra motore e diagnostica.
/// </summary>
public static class KeyNames
{
    // Mappa VK -> nome fisico per i tasti non alfanumerici e speciali rilevanti.
    private static readonly IReadOnlyDictionary<int, string> VkToName = new Dictionary<int, string>
    {
        [0x08] = "BACKSPACE",
        [0x09] = "TAB",
        [0x0D] = "ENTER",
        [0x1B] = "ESC",
        [0x20] = "SPACE",
        [0x14] = "CAPS",
        [0x2D] = "INSERT",
        [0x2E] = "DELETE",
        [0x24] = "HOME",
        [0x23] = "END",
        [0x21] = "PGUP",
        [0x22] = "PGDN",
        [0x25] = "LEFT",
        [0x26] = "UP",
        [0x27] = "RIGHT",
        [0x28] = "DOWN",
        // Modificatori
        [0xA0] = "LSHIFT",
        [0xA1] = "RSHIFT",
        [0xA2] = "LCTRL",
        [0xA3] = "RCTRL",
        [0xA4] = "LALT",
        [0xA5] = "RALT",
        [0x5B] = "LWIN",
        [0x5C] = "RWIN",
        [0x5D] = "APPS",
        // OEM / punteggiatura (posizioni fisiche su layout ANSI US)
        [0xBA] = "OEM_1",      // ;:
        [0xBB] = "OEM_PLUS",   // =+
        [0xBC] = "OEM_COMMA",  // ,<
        [0xBD] = "OEM_MINUS",  // -_
        [0xBE] = "OEM_PERIOD", // .>
        [0xBF] = "OEM_2",      // /?
        [0xC0] = "OEM_3",      // `~
        [0xDB] = "OEM_4",      // [{
        [0xDC] = "OEM_5",      // \|
        [0xDD] = "OEM_6",      // ]}
        [0xDE] = "OEM_7",      // '"
        [0xDF] = "OEM_8",
        [0xE2] = "OEM_102",    // <> su tastiere ISO
    };

    /// <summary>
    /// Restituisce il nome fisico canonico per un Virtual-Key code.
    /// Lettere A-Z e cifre 0-9 sono mappate direttamente; funzionali "F1".."F24".
    /// </summary>
    public static string FromVirtualKey(int vkCode)
    {
        // Lettere A-Z
        if (vkCode >= 0x41 && vkCode <= 0x5A)
            return ((char)vkCode).ToString();

        // Cifre 0-9 (riga numeri)
        if (vkCode >= 0x30 && vkCode <= 0x39)
            return ((char)vkCode).ToString();

        // Tastierino numerico NUMPAD0..NUMPAD9
        if (vkCode >= 0x60 && vkCode <= 0x69)
            return "NUMPAD" + (vkCode - 0x60);

        // Funzionali F1..F24
        if (vkCode >= 0x70 && vkCode <= 0x87)
            return "F" + (vkCode - 0x70 + 1);

        return VkToName.TryGetValue(vkCode, out var name)
            ? name
            : $"VK_{vkCode:X2}";
    }

    /// <summary>True se il VK code è un modificatore (Shift/Ctrl/Alt/Win).</summary>
    public static bool IsModifier(int vkCode) => vkCode switch
    {
        0x10 or 0xA0 or 0xA1 => true, // Shift
        0x11 or 0xA2 or 0xA3 => true, // Ctrl
        0x12 or 0xA4 or 0xA5 => true, // Alt
        0x5B or 0x5C => true,         // Win
        _ => false
    };
}
