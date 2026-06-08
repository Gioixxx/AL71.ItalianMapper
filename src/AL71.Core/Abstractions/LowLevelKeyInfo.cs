namespace AL71.Core.Abstractions;

/// <summary>
/// Dati grezzi di un evento tastiera catturato dall'hook a basso livello,
/// in forma indipendente dalla piattaforma. Passati al motore di remap.
/// </summary>
public readonly record struct LowLevelKeyInfo(
    int VkCode,
    int ScanCode,
    bool IsExtended,
    bool IsInjected,
    bool IsKeyDown);
