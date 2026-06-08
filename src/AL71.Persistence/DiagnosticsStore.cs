using System.Text.Json;
using AL71.Core.Models;
using AL71.Core.Serialization;

namespace AL71.Persistence;

/// <summary>
/// Salva gli output della diagnostica (Fase 1): device.json (VID/PID/nome) e
/// keymap-al71.json (mappa tasto fisico -> scanCode/vkCode).
/// </summary>
public sealed class DiagnosticsStore
{
    public DiagnosticsStore() => AppPaths.EnsureCreated();

    public string SaveDevice(DeviceInfo device)
    {
        File.WriteAllText(AppPaths.DeviceFile, JsonSerializer.Serialize(device, JsonDefaults.Options));
        return AppPaths.DeviceFile;
    }

    public string SaveKeyMap(IReadOnlyDictionary<string, KeyScanInfo> map)
    {
        File.WriteAllText(AppPaths.KeyMapFile, JsonSerializer.Serialize(map, JsonDefaults.Options));
        return AppPaths.KeyMapFile;
    }

    public DeviceInfo? LoadDevice()
    {
        if (!File.Exists(AppPaths.DeviceFile))
            return null;
        try { return JsonSerializer.Deserialize<DeviceInfo>(File.ReadAllText(AppPaths.DeviceFile), JsonDefaults.Options); }
        catch { return null; }
    }
}
