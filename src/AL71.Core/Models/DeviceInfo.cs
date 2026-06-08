namespace AL71.Core.Models;

/// <summary>
/// Informazioni identificative di una tastiera HID.
/// </summary>
public sealed class DeviceInfo
{
    public int VendorId { get; set; }
    public int ProductId { get; set; }
    public string DeviceName { get; set; } = string.Empty;

    public override string ToString() =>
        $"{DeviceName} (VID=0x{VendorId:X4}, PID=0x{ProductId:X4})";
}
