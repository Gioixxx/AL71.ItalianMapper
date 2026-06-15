namespace AL71.Core.Abstractions;

/// <summary>
/// Inietta input nel sistema operativo. Astratto per disaccoppiare il motore di
/// remap dall'implementazione Win32 (e renderlo testabile senza inviare input reale).
/// </summary>
public interface IKeyInjector
{
    /// <summary>Digita una stringa (uno o più caratteri Unicode).</summary>
    void SendText(string text);

    /// <summary>Invia un Virtual-Key (down+up), per tasti speciali e funzionali.</summary>
    void SendVirtualKey(ushort vk, bool extended = false);
}
