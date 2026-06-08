using AL71.Core.Models;

namespace AL71.Core.Abstractions;

/// <summary>Notifica che un tasto è stato rimappato (per logging/UI).</summary>
public sealed class KeyRemappedEventArgs : EventArgs
{
    public required string PhysicalKey { get; init; }
    public required KeyboardLayer Layer { get; init; }
    public required string Output { get; init; }
}
