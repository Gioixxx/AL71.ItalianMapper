namespace AL71.Core.Models;

/// <summary>
/// I layer logici di un layout. Determinati a runtime dallo stato dei modificatori
/// (es. AltGr = Ctrl+Alt su Windows).
/// </summary>
public enum KeyboardLayer
{
    Normal = 0,
    Shift = 1,
    AltGr = 2,
    ShiftAltGr = 3,
    Fn = 4
}
