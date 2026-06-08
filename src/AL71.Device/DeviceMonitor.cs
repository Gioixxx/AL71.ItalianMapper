using System.Management;
using AL71.Core.Abstractions;
using AL71.Core.Models;
using HidSharp;

namespace AL71.Device;

/// <summary>
/// Monitora la connessione della tastiera target tramite eventi WMI
/// (<c>Win32_DeviceChangeEvent</c>) ed enumera i dispositivi HID con HidSharp.
/// Nell'MVP serve ad attivare/disattivare il motore e ad aggiornare la tray.
/// </summary>
public sealed class DeviceMonitor : IDeviceMonitor
{
    private readonly Action<string>? _log;
    private ManagementEventWatcher? _watcher;
    private int _vendorId;
    private int _productId;

    public DeviceMonitor(Action<string>? log = null) => _log = log;

    public bool IsTargetConnected { get; private set; }

    public event EventHandler<DeviceInfo>? Connected;
    public event EventHandler<DeviceInfo>? Disconnected;

    public void Start(int vendorId, int productId)
    {
        _vendorId = vendorId;
        _productId = productId;

        // Stato iniziale.
        IsTargetConnected = FindTarget() is not null;

        try
        {
            // Win32_DeviceChangeEvent scatta su arrivo/rimozione di dispositivi.
            var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
            _watcher = new ManagementEventWatcher(query);
            _watcher.EventArrived += OnDeviceChange;
            _watcher.Start();
            _log?.Invoke("Monitoraggio dispositivi avviato.");
        }
        catch (Exception ex)
        {
            _log?.Invoke($"Impossibile avviare il monitoraggio dispositivi: {ex.Message}");
        }
    }

    public void Stop()
    {
        if (_watcher is null)
            return;

        _watcher.EventArrived -= OnDeviceChange;
        try { _watcher.Stop(); } catch { /* ignore */ }
        _watcher.Dispose();
        _watcher = null;
    }

    public IReadOnlyList<DeviceInfo> EnumerateKeyboards()
    {
        var list = new List<DeviceInfo>();
        foreach (var dev in DeviceList.Local.GetHidDevices())
        {
            string name;
            try { name = dev.GetFriendlyName(); }
            catch { name = "(HID)"; }

            list.Add(new DeviceInfo
            {
                VendorId = dev.VendorID,
                ProductId = dev.ProductID,
                DeviceName = name
            });
        }

        return list;
    }

    private HidDevice? FindTarget()
    {
        if (_vendorId == 0 && _productId == 0)
            return null;

        foreach (var dev in DeviceList.Local.GetHidDevices())
        {
            if (dev.VendorID == _vendorId && dev.ProductID == _productId)
                return dev;
        }

        return null;
    }

    private void OnDeviceChange(object sender, EventArrivedEventArgs e)
    {
        var target = FindTarget();
        var nowConnected = target is not null;
        if (nowConnected == IsTargetConnected)
            return;

        IsTargetConnected = nowConnected;
        var info = new DeviceInfo
        {
            VendorId = _vendorId,
            ProductId = _productId,
            DeviceName = target is not null ? SafeName(target) : "YUNZII AL71"
        };

        if (nowConnected)
        {
            _log?.Invoke($"Tastiera connessa: {info}");
            Connected?.Invoke(this, info);
        }
        else
        {
            _log?.Invoke($"Tastiera scollegata: {info}");
            Disconnected?.Invoke(this, info);
        }
    }

    private static string SafeName(HidDevice dev)
    {
        try { return dev.GetFriendlyName(); }
        catch { return "YUNZII AL71"; }
    }

    public void Dispose() => Stop();
}
