using AL71.Core.Models;
using AL71.UI.Models;
using AL71.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AL71.UI.ViewModels;

/// <summary>
/// Tastiera visuale + editor del tasto selezionato. Modifica le mappature del
/// profilo attivo e le persiste tramite l'<see cref="AppController"/>.
/// </summary>
public partial class EditorViewModel : ObservableObject
{
    private readonly AppController _controller;

    public IReadOnlyList<KeyRow> Rows { get; } = KeyboardLayoutDefinition.Rows;

    [ObservableProperty] private string _selectedPhysicalKey = "";
    [ObservableProperty] private bool _hasSelection;
    [ObservableProperty] private string? _normal;
    [ObservableProperty] private string? _shift;
    [ObservableProperty] private string? _altGr;
    [ObservableProperty] private string? _shiftAltGr;

    public EditorViewModel(AppController controller) => _controller = controller;

    [RelayCommand]
    private void SelectKey(KeyCap? cap)
    {
        if (cap is null)
            return;

        SelectedPhysicalKey = cap.PhysicalKey;
        HasSelection = true;

        var mapping = FindMapping(cap.PhysicalKey);
        Normal = mapping?.Normal;
        Shift = mapping?.Shift;
        AltGr = mapping?.AltGr;
        ShiftAltGr = mapping?.ShiftAltGr;
    }

    [RelayCommand]
    private void Apply()
    {
        if (!HasSelection || _controller.ActiveProfile is null)
            return;

        var mapping = FindMapping(SelectedPhysicalKey);
        if (mapping is null)
        {
            mapping = new KeyMapping { PhysicalKey = SelectedPhysicalKey };
            _controller.ActiveProfile.Mappings.Add(mapping);
        }

        mapping.Normal = Empty(Normal);
        mapping.Shift = Empty(Shift);
        mapping.AltGr = Empty(AltGr);
        mapping.ShiftAltGr = Empty(ShiftAltGr);

        // Rimuovi le mappature vuote per non ingombrare il profilo.
        if (!mapping.HasAnyMapping)
            _controller.ActiveProfile.Mappings.Remove(mapping);

        _controller.SaveActiveProfile();
    }

    /// <summary>Rilegge i valori del tasto selezionato dal profilo attivo (dopo cambio profilo).</summary>
    public void ReloadFromActiveProfile()
    {
        if (!HasSelection)
            return;

        var mapping = FindMapping(SelectedPhysicalKey);
        Normal = mapping?.Normal;
        Shift = mapping?.Shift;
        AltGr = mapping?.AltGr;
        ShiftAltGr = mapping?.ShiftAltGr;
    }

    [RelayCommand]
    private void Clear()
    {
        Normal = Shift = AltGr = ShiftAltGr = null;
        Apply();
    }

    private KeyMapping? FindMapping(string physicalKey) =>
        _controller.ActiveProfile?.Mappings
            .FirstOrDefault(m => string.Equals(m.PhysicalKey, physicalKey, StringComparison.OrdinalIgnoreCase));

    private static string? Empty(string? s) => string.IsNullOrEmpty(s) ? null : s;
}
