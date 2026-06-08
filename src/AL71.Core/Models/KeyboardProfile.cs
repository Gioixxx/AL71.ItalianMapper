namespace AL71.Core.Models;

/// <summary>
/// Un profilo completo: insieme di rimappature + macro. L'utente può avere più
/// profili (Italiano, Gaming, Programmazione, ...) e passare dall'uno all'altro.
/// </summary>
public sealed class KeyboardProfile
{
    public string Name { get; set; } = "Nuovo profilo";

    public string Description { get; set; } = string.Empty;

    /// <summary>Mappature per tasto fisico, indicizzate da <see cref="KeyMapping.PhysicalKey"/>.</summary>
    public List<KeyMapping> Mappings { get; set; } = new();

    public List<MacroDefinition> Macros { get; set; } = new();

    /// <summary>Versione dello schema, per migrazioni future.</summary>
    public int SchemaVersion { get; set; } = 1;
}
