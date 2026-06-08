using AL71.Core.Models;

namespace AL71.Core.Abstractions;

/// <summary>Persistenza delle impostazioni globali.</summary>
public interface ISettingsStore
{
    AppSettings Load();

    void Save(AppSettings settings);
}
