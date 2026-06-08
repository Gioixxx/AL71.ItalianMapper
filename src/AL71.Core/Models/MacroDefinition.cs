namespace AL71.Core.Models;

/// <summary>
/// Definizione di una macro: una sequenza di azioni eseguite al trigger.
/// Nell'MVP le azioni sono testo da digitare; in v2.0 si aggiungeranno
/// tasti speciali, ritardi e modificatori.
/// </summary>
public sealed class MacroDefinition
{
    /// <summary>Tasto fisico che attiva la macro, es. "F13".</summary>
    public string Trigger { get; set; } = string.Empty;

    /// <summary>Nome leggibile della macro.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Sequenza di stringhe da emettere in ordine.</summary>
    public List<string> Actions { get; set; } = new();
}
