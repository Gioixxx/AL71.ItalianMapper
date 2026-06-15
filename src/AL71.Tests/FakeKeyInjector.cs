using AL71.Core.Abstractions;

namespace AL71.Tests;

/// <summary>Injector di test: registra ciò che verrebbe inviato, senza toccare il sistema.</summary>
internal sealed class FakeKeyInjector : IKeyInjector
{
    public List<string> SentText { get; } = new();
    public List<ushort> SentVirtualKeys { get; } = new();

    public void SendText(string text) => SentText.Add(text);

    public void SendVirtualKey(ushort vk, bool extended = false) => SentVirtualKeys.Add(vk);
}
