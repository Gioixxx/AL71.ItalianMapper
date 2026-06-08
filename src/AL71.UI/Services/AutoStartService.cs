using System.Diagnostics;
using Microsoft.Win32;

namespace AL71.UI.Services;

/// <summary>
/// Gestisce l'avvio automatico con Windows tramite la chiave di registro
/// <c>HKCU\Software\Microsoft\Windows\CurrentVersion\Run</c>.
/// </summary>
public static class AutoStartService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "AL71LayoutManager";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
        return key?.GetValue(ValueName) is not null;
    }

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)
                        ?? Registry.CurrentUser.CreateSubKey(RunKey);
        if (key is null)
            return;

        if (enabled)
        {
            var exe = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(exe))
                key.SetValue(ValueName, $"\"{exe}\" --minimized");
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }
}
