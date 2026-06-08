using AL71.Core.Models;

namespace AL71.Core.Abstractions;

/// <summary>
/// Monitora connessione/disconnessione della tastiera target (per VID/PID) e
/// consente di enumerare le tastiere HID presenti (usato dalla diagnostica).
/// </summary>
public interface IDeviceMonitor : IDisposable
{
    /// <summary>True se la tastiera target risulta attualmente connessa.</summary>
    bool IsTargetConnected { get; }

    /// <summary>Avvia il monitoraggio per il VID/PID indicato.</summary>
    void Start(int vendorId, int productId);

    void Stop();

    /// <summary>Elenca le tastiere HID attualmente collegate.</summary>
    IReadOnlyList<DeviceInfo> EnumerateKeyboards();

    event EventHandler<DeviceInfo>? Connected;
    event EventHandler<DeviceInfo>? Disconnected;
}
