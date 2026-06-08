namespace AL71.Core.Abstractions;

/// <summary>
/// Servizio che installa l'hook tastiera a basso livello e instrada gli eventi
/// al <see cref="IRemapEngine"/>. Va eseguito su un thread con message loop.
/// </summary>
public interface IKeyboardHookService : IDisposable
{
    bool IsRunning { get; }

    /// <summary>Installa l'hook e avvia il pump dei messaggi sul thread dedicato.</summary>
    void Start();

    /// <summary>Disinstalla l'hook e ferma il thread.</summary>
    void Stop();
}
