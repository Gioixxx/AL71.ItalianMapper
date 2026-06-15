using AL71.Core.Abstractions;
using AL71.Core.Models;
using AL71.Hook;
using Xunit;

namespace AL71.Tests;

public class RemapEngineTests
{
    private const int VkA = 0x41;       // "A"
    private const int VkB = 0x42;       // "B" (non mappato nei test)
    private const int VkLShift = 0xA0;
    private const int VkRAlt = 0xA5;    // AltGr
    private const int VkRCtrl = 0xA3;
    private const int VkLWin = 0x5B;

    private static LowLevelKeyInfo Down(int vk, bool injected = false) => new(vk, 0, false, injected, true);
    private static LowLevelKeyInfo Up(int vk, bool injected = false) => new(vk, 0, false, injected, false);

    private static KeyboardProfile ProfileForA() => new()
    {
        Name = "Test",
        Mappings = { new KeyMapping { PhysicalKey = "A", Normal = "à", Shift = "À", AltGr = "@", ShiftAltGr = "#" } }
    };

    private static (RemapEngine engine, FakeKeyInjector injector) Build(KeyboardProfile? profile = null)
    {
        var injector = new FakeKeyInjector();
        var engine = new RemapEngine(injector) { ActiveProfile = profile, Enabled = true };
        return (engine, injector);
    }

    [Fact]
    public void Disabled_PassesThrough()
    {
        var (engine, injector) = Build(ProfileForA());
        engine.Enabled = false;
        Assert.False(engine.ProcessKey(Down(VkA)));
        Assert.Empty(injector.SentText);
    }

    [Fact]
    public void InjectedEvents_AreIgnored()
    {
        var (engine, injector) = Build(ProfileForA());
        Assert.False(engine.ProcessKey(Down(VkA, injected: true)));
        Assert.Empty(injector.SentText);
    }

    [Fact]
    public void UnmappedKey_PassesThrough()
    {
        var (engine, injector) = Build(ProfileForA());
        Assert.False(engine.ProcessKey(Down(VkB)));
        Assert.Empty(injector.SentText);
    }

    [Fact]
    public void Modifier_UpdatesStateButIsNotRemapped()
    {
        var (engine, injector) = Build(ProfileForA());
        Assert.False(engine.ProcessKey(Down(VkLShift)));
        Assert.Empty(injector.SentText);
    }

    [Fact]
    public void NormalLayer_EmitsNormalAndSuppresses()
    {
        var (engine, injector) = Build(ProfileForA());
        Assert.True(engine.ProcessKey(Down(VkA)));
        Assert.Equal("à", Assert.Single(injector.SentText));
    }

    [Fact]
    public void ShiftLayer_EmitsShift()
    {
        var (engine, injector) = Build(ProfileForA());
        engine.ProcessKey(Down(VkLShift));
        Assert.True(engine.ProcessKey(Down(VkA)));
        Assert.Equal("À", Assert.Single(injector.SentText));
    }

    [Fact]
    public void AltGrLayer_EmitsAltGr()
    {
        var (engine, injector) = Build(ProfileForA());
        engine.ProcessKey(Down(VkRAlt));
        Assert.True(engine.ProcessKey(Down(VkA)));
        Assert.Equal("@", Assert.Single(injector.SentText));
    }

    [Fact]
    public void ShiftAltGrLayer_EmitsShiftAltGr()
    {
        var (engine, injector) = Build(ProfileForA());
        engine.ProcessKey(Down(VkLShift));
        engine.ProcessKey(Down(VkRAlt));
        Assert.True(engine.ProcessKey(Down(VkA)));
        Assert.Equal("#", Assert.Single(injector.SentText));
    }

    [Fact]
    public void RealCtrlShortcut_PassesThrough()
    {
        // Ctrl+A deve restare una scorciatoia: nessun remap.
        var (engine, injector) = Build(ProfileForA());
        engine.ProcessKey(Down(VkRCtrl));
        Assert.False(engine.ProcessKey(Down(VkA)));
        Assert.Empty(injector.SentText);
    }

    [Fact]
    public void WinShortcut_PassesThrough()
    {
        var (engine, injector) = Build(ProfileForA());
        engine.ProcessKey(Down(VkLWin));
        Assert.False(engine.ProcessKey(Down(VkA)));
        Assert.Empty(injector.SentText);
    }

    [Fact]
    public void KeyUp_AfterRemappedDown_IsSuppressed()
    {
        var (engine, _) = Build(ProfileForA());
        Assert.True(engine.ProcessKey(Down(VkA)));
        Assert.True(engine.ProcessKey(Up(VkA))); // up soppresso per simmetria
    }

    [Fact]
    public void KeyUp_WithoutRemappedDown_PassesThrough()
    {
        var (engine, _) = Build(ProfileForA());
        Assert.False(engine.ProcessKey(Up(VkB)));
    }

    [Fact]
    public void EmptyLayerValue_PassesThrough()
    {
        var injector = new FakeKeyInjector();
        var profile = new KeyboardProfile
        {
            Name = "T",
            Mappings = { new KeyMapping { PhysicalKey = "A", Normal = "à" } } // niente Shift
        };
        var engine = new RemapEngine(injector) { ActiveProfile = profile };
        engine.ProcessKey(Down(VkLShift));
        Assert.False(engine.ProcessKey(Down(VkA))); // layer Shift vuoto
        Assert.Empty(injector.SentText);
    }

    [Fact]
    public void SwitchingProfile_RebuildsLookup()
    {
        var (engine, injector) = Build(ProfileForA());
        Assert.True(engine.ProcessKey(Down(VkA)));

        engine.ActiveProfile = new KeyboardProfile { Name = "Vuoto" };
        Assert.False(engine.ProcessKey(Down(VkA))); // ora "A" non è più mappato
        Assert.Equal("à", Assert.Single(injector.SentText));
    }
}
