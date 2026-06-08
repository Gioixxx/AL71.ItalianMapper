using System.Runtime.InteropServices;
using AL71.Core.Abstractions;
using static AL71.Hook.NativeMethods;

namespace AL71.Hook;

/// <summary>
/// Installa l'hook tastiera a basso livello (WH_KEYBOARD_LL) su un thread dedicato
/// con message loop e instrada gli eventi al <see cref="IRemapEngine"/>.
/// </summary>
public sealed class KeyboardHookService : IKeyboardHookService
{
    private readonly IRemapEngine _engine;
    private readonly Action<string>? _log;

    // Mantiene viva la delegate per evitare che il GC la raccolga mentre l'hook è attivo.
    private readonly LowLevelKeyboardProc _proc;

    private IntPtr _hookId = IntPtr.Zero;
    private Thread? _thread;
    private uint _threadId;
    private readonly ManualResetEventSlim _ready = new(false);

    public KeyboardHookService(IRemapEngine engine, Action<string>? log = null)
    {
        _engine = engine;
        _log = log;
        _proc = HookCallback;
    }

    public bool IsRunning => _hookId != IntPtr.Zero;

    public void Start()
    {
        if (IsRunning)
            return;

        _ready.Reset();
        _thread = new Thread(ThreadProc)
        {
            IsBackground = true,
            Name = "AL71-KeyboardHook"
        };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();

        // Attendi che l'hook sia installato (o fallito) prima di proseguire.
        _ready.Wait(TimeSpan.FromSeconds(5));
    }

    public void Stop()
    {
        if (_thread is null)
            return;

        if (_threadId != 0)
            PostThreadMessage(_threadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);

        _thread.Join(TimeSpan.FromSeconds(2));
        _thread = null;
        _threadId = 0;
    }

    private void ThreadProc()
    {
        _threadId = GetCurrentThreadId();
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(null), 0);

        if (_hookId == IntPtr.Zero)
        {
            var err = Marshal.GetLastWin32Error();
            _log?.Invoke($"Installazione hook fallita (Win32 error {err}).");
            _ready.Set();
            return;
        }

        _log?.Invoke("Hook tastiera installato.");
        _ready.Set();

        // Message loop: necessario per ricevere le callback dell'hook a basso livello.
        while (GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
        {
            TranslateMessage(msg);
            DispatchMessage(msg);
        }

        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
            _log?.Invoke("Hook tastiera disinstallato.");
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode == HC_ACTION)
        {
            var msg = (int)wParam;
            var isDown = msg is WM_KEYDOWN or WM_SYSKEYDOWN;
            var isUp = msg is WM_KEYUP or WM_SYSKEYUP;

            if (isDown || isUp)
            {
                try
                {
                    var data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                    var injected = (data.flags & LLKHF_INJECTED) != 0
                                   || data.dwExtraInfo == InjectionSignature;

                    var info = new LowLevelKeyInfo(
                        VkCode: (int)data.vkCode,
                        ScanCode: (int)data.scanCode,
                        IsExtended: (data.flags & LLKHF_EXTENDED) != 0,
                        IsInjected: injected,
                        IsKeyDown: isDown);

                    if (_engine.ProcessKey(info))
                        return (IntPtr)1; // sopprime l'evento originale
                }
                catch (Exception ex)
                {
                    // Una eccezione nell'hook non deve mai propagarsi: degrada al pass-through.
                    _log?.Invoke($"Errore hook: {ex.Message}");
                }
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        Stop();
        _ready.Dispose();
    }
}
