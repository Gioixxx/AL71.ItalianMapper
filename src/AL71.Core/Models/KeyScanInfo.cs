namespace AL71.Core.Models;

/// <summary>
/// Codici grezzi catturati per un tasto fisico durante la diagnostica (Fase 1.2):
/// scanCode hardware e Virtual-Key di Windows.
/// </summary>
public sealed class KeyScanInfo
{
    public int ScanCode { get; set; }
    public int VkCode { get; set; }
}
