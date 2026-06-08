using CommunityToolkit.Mvvm.ComponentModel;

namespace AL71.UI.ViewModels.Wizard;

/// <summary>Un tasto nella procedura guidata di cattura: posizione attesa + stato.</summary>
public partial class CaptureKeyViewModel : ObservableObject
{
    public required string PhysicalKey { get; init; }
    public required string Label { get; init; }
    public double Width { get; init; } = 1.0;

    /// <summary>È il tasto che l'utente deve premere ora.</summary>
    [ObservableProperty] private bool _isCurrent;

    /// <summary>È già stato catturato.</summary>
    [ObservableProperty] private bool _isCaptured;

    [ObservableProperty] private int _scanCode;
    [ObservableProperty] private int _vkCode;
    [ObservableProperty] private string? _capturedName;
}
