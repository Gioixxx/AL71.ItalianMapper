using AL71.Core.Input;
using Xunit;

namespace AL71.Tests;

public class KeyNamesTests
{
    [Theory]
    [InlineData(0x41, "A")]
    [InlineData(0x5A, "Z")]
    [InlineData(0x30, "0")]
    [InlineData(0x39, "9")]
    [InlineData(0x20, "SPACE")]
    [InlineData(0x0D, "ENTER")]
    [InlineData(0xBA, "OEM_1")]
    [InlineData(0xDE, "OEM_7")]
    [InlineData(0x70, "F1")]
    [InlineData(0x87, "F24")]
    [InlineData(0x60, "NUMPAD0")]
    public void FromVirtualKey_MapsKnownKeys(int vk, string expected) =>
        Assert.Equal(expected, KeyNames.FromVirtualKey(vk));

    [Fact]
    public void FromVirtualKey_UnknownReturnsHexFallback() =>
        Assert.Equal("VK_FF", KeyNames.FromVirtualKey(0xFF));

    [Theory]
    [InlineData(0xA0)] // LSHIFT
    [InlineData(0xA2)] // LCTRL
    [InlineData(0xA5)] // RALT
    [InlineData(0x5B)] // LWIN
    public void IsModifier_TrueForModifiers(int vk) => Assert.True(KeyNames.IsModifier(vk));

    [Theory]
    [InlineData(0x41)] // A
    [InlineData(0x20)] // SPACE
    public void IsModifier_FalseForRegularKeys(int vk) => Assert.False(KeyNames.IsModifier(vk));
}
