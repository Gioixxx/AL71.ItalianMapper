using System.Runtime.InteropServices;

namespace AL71.UI.Services;

internal static class Win32
{
    private const uint MAPVK_VK_TO_VSC = 0;

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    /// <summary>Restituisce lo scancode hardware corrispondente a un Virtual-Key.</summary>
    public static int VirtualKeyToScanCode(int vk) => (int)MapVirtualKey((uint)vk, MAPVK_VK_TO_VSC);
}
