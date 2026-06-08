using AL71.Core.Models;

namespace AL71.Core.Abstractions;

/// <summary>Persistenza dei profili (load/save/import/export) su disco.</summary>
public interface IProfileStore
{
    IReadOnlyList<string> ListProfiles();

    KeyboardProfile Load(string name);

    void Save(KeyboardProfile profile);

    void Delete(string name);

    /// <summary>Esporta un profilo in un file arbitrario (JSON).</summary>
    void Export(KeyboardProfile profile, string filePath);

    /// <summary>Importa un profilo da un file arbitrario (JSON).</summary>
    KeyboardProfile Import(string filePath);
}
