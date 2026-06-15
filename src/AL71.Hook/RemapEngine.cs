using System.Collections.Concurrent;
using AL71.Core.Abstractions;
using AL71.Core.Input;
using AL71.Core.Models;

namespace AL71.Hook;

/// <summary>
/// Motore di remapping. Determina il layer dallo stato dei modificatori, cerca la
/// mappatura del tasto fisico (cache <see cref="ConcurrentDictionary{TKey,TValue}"/>)
/// e, se presente, sopprime l'originale iniettando il sostituto Unicode.
/// </summary>
public sealed class RemapEngine : IRemapEngine
{
    private readonly IKeyInjector _injector;

    // Cache di lookup: tasto fisico -> mappatura. Sostituita atomicamente al cambio profilo.
    private volatile IReadOnlyDictionary<string, KeyMapping> _lookup =
        new Dictionary<string, KeyMapping>();

    private KeyboardProfile? _activeProfile;

    // Stato modificatori (aggiornato dal flusso dell'hook, thread singolo).
    private bool _lShift, _rShift, _lCtrl, _rCtrl, _lAlt, _rAlt, _lWin, _rWin;

    // Tasti fisici per cui abbiamo soppresso il key-down: ne sopprimiamo anche il key-up.
    private readonly HashSet<string> _suppressed = new();

    public RemapEngine(IKeyInjector injector) => _injector = injector;

    public bool Enabled { get; set; } = true;

    public event EventHandler<KeyRemappedEventArgs>? KeyRemapped;

    public KeyboardProfile? ActiveProfile
    {
        get => _activeProfile;
        set
        {
            _activeProfile = value;
            _lookup = BuildLookup(value);
        }
    }

    private static IReadOnlyDictionary<string, KeyMapping> BuildLookup(KeyboardProfile? profile)
    {
        var dict = new ConcurrentDictionary<string, KeyMapping>(StringComparer.OrdinalIgnoreCase);
        if (profile is null)
            return dict;

        foreach (var m in profile.Mappings)
        {
            if (!string.IsNullOrEmpty(m.PhysicalKey) && m.HasAnyMapping)
                dict[m.PhysicalKey] = m;
        }

        return dict;
    }

    public bool ProcessKey(in LowLevelKeyInfo info)
    {
        // Anti-loop: ignora ciò che abbiamo iniettato noi.
        if (info.IsInjected)
            return false;

        // I modificatori aggiornano lo stato ma non vengono mai rimappati.
        if (KeyNames.IsModifier(info.VkCode))
        {
            UpdateModifier(info.VkCode, info.IsKeyDown);
            return false;
        }

        if (!Enabled)
            return false;

        var physicalKey = KeyNames.FromVirtualKey(info.VkCode);

        // Key-up: se avevamo soppresso il down, sopprimi anche l'up per simmetria.
        if (!info.IsKeyDown)
            return _suppressed.Remove(physicalKey);

        // Non interferire con le scorciatoie reali (Ctrl/Alt/Win, esclusa la finta
        // combinazione AltGr = LCtrl+RAlt).
        if (IsShortcutModifierActive())
            return false;

        if (!_lookup.TryGetValue(physicalKey, out var mapping))
            return false;

        var layer = CurrentLayer();
        var output = mapping.ForLayer(layer);
        if (string.IsNullOrEmpty(output))
            return false;

        _injector.SendText(output);
        _suppressed.Add(physicalKey);
        KeyRemapped?.Invoke(this, new KeyRemappedEventArgs
        {
            PhysicalKey = physicalKey,
            Layer = layer,
            Output = output
        });
        return true;
    }

    private KeyboardLayer CurrentLayer()
    {
        var shift = _lShift || _rShift;
        var altGr = _rAlt; // AltGr = Right Alt su Windows
        return (shift, altGr) switch
        {
            (true, true) => KeyboardLayer.ShiftAltGr,
            (false, true) => KeyboardLayer.AltGr,
            (true, false) => KeyboardLayer.Shift,
            _ => KeyboardLayer.Normal
        };
    }

    private bool IsShortcutModifierActive()
    {
        // AltGr fa sì che Windows segnali un LCtrl "finto": va ignorato.
        var realCtrl = _rCtrl || (_lCtrl && !_rAlt);
        var plainAlt = _lAlt; // LAlt da solo = scorciatoia (Alt+...)
        var win = _lWin || _rWin;
        return realCtrl || plainAlt || win;
    }

    private void UpdateModifier(int vk, bool down)
    {
        switch (vk)
        {
            case 0xA0: _lShift = down; break;
            case 0xA1: _rShift = down; break;
            case 0x10: _lShift = down; break; // VK_SHIFT generico
            case 0xA2: _lCtrl = down; break;
            case 0xA3: _rCtrl = down; break;
            case 0x11: _lCtrl = down; break; // VK_CONTROL generico
            case 0xA4: _lAlt = down; break;
            case 0xA5: _rAlt = down; break;
            case 0x12: _lAlt = down; break;  // VK_MENU generico
            case 0x5B: _lWin = down; break;
            case 0x5C: _rWin = down; break;
        }
    }
}
