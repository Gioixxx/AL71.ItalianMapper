namespace AL71.UI.Models;

/// <summary>Un tasto disegnato sulla tastiera visuale.</summary>
public sealed class KeyCap
{
    public required string PhysicalKey { get; init; }
    public required string Label { get; init; }

    /// <summary>Larghezza relativa (1.0 = tasto standard).</summary>
    public double Width { get; init; } = 1.0;
}

/// <summary>Una riga di tasti.</summary>
public sealed class KeyRow
{
    public required IReadOnlyList<KeyCap> Keys { get; init; }
}
