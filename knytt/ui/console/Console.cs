using Godot;
using YKnyttLib.Logging;

// Thin C# bridge to LimboConsole (GDScript addon).
// Game code references this for IsOpen checks and signal wiring.
// Also the sink for KnyttLogger, mirroring what the pre-LimboConsole Console did.
public partial class Console : Node, IKnyttLoggerTarget
{
    [Signal] public delegate void ConsoleOpenEventHandler();
    [Signal] public delegate void ConsoleClosedEventHandler();

    private Node _limbo;

    public bool IsOpen => _limbo?.Call("is_open").AsBool() ?? false;

    public override void _Ready()
    {
        _limbo = GetNode("/root/LimboConsole");
        _limbo.Connect("toggled", new Callable(this, nameof(OnLimboToggled)));

        // Godot's output, plus the in-game console.
        KnyttLogger.AddTarget(new GodotLoggerTarget());
        KnyttLogger.AddTarget(this);
    }

    // IKnyttLoggerTarget: forward to LimboConsole at the matching severity.
    public void NewMessage(KnyttLogMessage message)
    {
        if (_limbo == null) { return; }
        string text = message.Render();
        switch (message.Level)
        {
            case KnyttLogger.Level.ERROR:
            case KnyttLogger.Level.FATAL:
                _limbo.Call("error", text);
                break;
            case KnyttLogger.Level.WARN:
                _limbo.Call("warn", text);
                break;
            default:
                _limbo.Call("info", text);
                break;
        }
    }

    private void OnLimboToggled(bool isShown)
    {
        if (isShown)
            EmitSignal(SignalName.ConsoleOpen);
        else
            EmitSignal(SignalName.ConsoleClosed);
    }

    public void toggleConsole() => _limbo.Call("toggle_console");
}
