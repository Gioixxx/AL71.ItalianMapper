namespace AL71.UI.ViewModels.Wizard;

/// <summary>Una riga della tastiera nella procedura guidata.</summary>
public sealed class CaptureRow
{
    public required IReadOnlyList<CaptureKeyViewModel> Keys { get; init; }
}
