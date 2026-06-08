using System.Runtime.InteropServices;

namespace AL71.UI.Services;

internal static class Win32
{
    private const uint MAPVK_VK_TO_VSC = 0;

    // Modificatori per RegisterHotKey
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_NOREPEAT = 0x4000;
    public const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    /// <summary>Restituisce lo scancode hardware corrispondente a un Virtual-Key.</summary>
    public static int VirtualKeyToScanCode(int vk) => (int)MapVirtualKey((uint)vk, MAPVK_VK_TO_VSC);
}
