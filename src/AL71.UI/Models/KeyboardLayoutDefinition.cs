namespace AL71.UI.Models;

/// <summary>
/// Disposizione fisica (approssimata) della YUNZII AL71 (~71 tasti, ANSI 65%).
/// Da affinare con la mappa reale catturata dalla diagnostica.
/// </summary>
public static class KeyboardLayoutDefinition
{
    private static KeyCap K(string physical, string label, double width = 1.0)
        => new() { PhysicalKey = physical, Label = label, Width = width };

    public static IReadOnlyList<KeyRow> Rows { get; } = new[]
    {
        new KeyRow
        {
            Keys = new[]
            {
                K("ESC", "Esc"), K("1", "1"), K("2", "2"), K("3", "3"), K("4", "4"),
                K("5", "5"), K("6", "6"), K("7", "7"), K("8", "8"), K("9", "9"),
                K("0", "0"), K("OEM_MINUS", "- _"), K("OEM_PLUS", "= +"),
                K("BACKSPACE", "Backspace", 2.0), K("DELETE", "Del")
            }
        },
        new KeyRow
        {
            Keys = new[]
            {
                K("TAB", "Tab", 1.5), K("Q", "Q"), K("W", "W"), K("E", "E"), K("R", "R"),
                K("T", "T"), K("Y", "Y"), K("U", "U"), K("I", "I"), K("O", "O"),
                K("P", "P"), K("OEM_4", "["), K("OEM_6", "]"), K("OEM_5", "\\", 1.5),
                K("HOME", "Home")
            }
        },
        new KeyRow
        {
            Keys = new[]
            {
                K("CAPS", "Caps", 1.75), K("A", "A"), K("S", "S"), K("D", "D"), K("F", "F"),
                K("G", "G"), K("H", "H"), K("J", "J"), K("K", "K"), K("L", "L"),
                K("OEM_1", "; :"), K("OEM_7", "' \""), K("ENTER", "Enter", 2.25),
                K("PGUP", "PgUp")
            }
        },
        new KeyRow
        {
            Keys = new[]
            {
                K("LSHIFT", "Shift", 2.25), K("Z", "Z"), K("X", "X"), K("C", "C"), K("V", "V"),
                K("B", "B"), K("N", "N"), K("M", "M"), K("OEM_COMMA", ", <"),
                K("OEM_PERIOD", ". >"), K("OEM_2", "/ ?"), K("RSHIFT", "Shift", 1.75),
                K("UP", "↑"), K("PGDN", "PgDn")
            }
        },
        new KeyRow
        {
            Keys = new[]
            {
                K("LCTRL", "Ctrl", 1.25), K("LWIN", "Win", 1.25), K("LALT", "Alt", 1.25),
                K("SPACE", "Space", 6.25), K("RALT", "AltGr", 1.25), K("FN", "Fn"),
                K("LEFT", "←"), K("DOWN", "↓"), K("RIGHT", "→")
            }
        }
    };
}
