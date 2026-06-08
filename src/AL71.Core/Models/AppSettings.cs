namespace AL71.Core.Models;

/// <summary>
/// Impostazioni globali dell'applicazione (persistite in Settings/settings.json).
/// </summary>
public sealed class AppSettings
{
    /// <summary>Nome del profilo attivo.</summary>
    public string ActiveProfile { get; set; } = "Italiano";

    /// <summary>Avvia l'app all'accesso a Windows.</summary>
    public bool RunAtStartup { get; set; }

    /// <summary>Il motore di remap è abilitato.</summary>
    public bool RemapEnabled { get; set; } = true;

    /// <summary>
    /// Abilita il remap solo quando l'AL71 è rilevata come connessa.
    /// Se false, il remap globale è sempre attivo (entro i limiti del WH_KEYBOARD_LL).
    /// </summary>
    public bool OnlyWhenDeviceConnected { get; set; } = true;

    /// <summary>VID rilevato dell'AL71 (0 se non ancora scoperto).</summary>
    public int DeviceVendorId { get; set; }

    /// <summary>PID rilevato dell'AL71 (0 se non ancora scoperto).</summary>
    public int DeviceProductId { get; set; }

    public bool MinimizeToTray { get; set; } = true;
}
