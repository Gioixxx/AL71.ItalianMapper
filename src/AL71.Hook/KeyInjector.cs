using System.Runtime.InteropServices;
using AL71.Core.Abstractions;
using static AL71.Hook.NativeMethods;

namespace AL71.Hook;

/// <summary>
/// Inietta input nel sistema. Per i caratteri usa KEYEVENTF_UNICODE: invia il
/// carattere direttamente, indipendentemente dal layout Windows attivo, evitando
/// il problema AltGr (=Ctrl+Alt) e i dead-key. Marca gli eventi con una firma in
/// dwExtraInfo per riconoscerli ed evitare loop.
/// </summary>
public sealed class KeyInjector : IKeyInjector
{
    /// <summary>Digita una stringa (uno o più caratteri Unicode) tramite SendInput.</summary>
    public void SendText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        // Per ogni char UTF-16: un evento key-down e uno key-up.
        var inputs = new INPUT[text.Length * 2];
        var i = 0;
        foreach (var ch in text)
        {
            inputs[i++] = MakeUnicode(ch, keyUp: false);
            inputs[i++] = MakeUnicode(ch, keyUp: true);
        }

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    /// <summary>Invia un Virtual-Key (down+up), per tasti speciali e funzionali.</summary>
    public void SendVirtualKey(ushort vk, bool extended = false)
    {
        var flags = extended ? KEYEVENTF_EXTENDEDKEY : 0u;
        var inputs = new[]
        {
            MakeVk(vk, flags),
            MakeVk(vk, flags | KEYEVENTF_KEYUP)
        };
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    private static INPUT MakeUnicode(char ch, bool keyUp) => new()
    {
        type = INPUT_KEYBOARD,
        u = new InputUnion
        {
            ki = new KEYBDINPUT
            {
                wVk = 0,
                wScan = ch,
                dwFlags = KEYEVENTF_UNICODE | (keyUp ? KEYEVENTF_KEYUP : 0u),
                time = 0,
                dwExtraInfo = InjectionSignature
            }
        }
    };

    private static INPUT MakeVk(ushort vk, uint flags) => new()
    {
        type = INPUT_KEYBOARD,
        u = new InputUnion
        {
            ki = new KEYBDINPUT
            {
                wVk = vk,
                wScan = 0,
                dwFlags = flags,
                time = 0,
                dwExtraInfo = InjectionSignature
            }
        }
    };
}
