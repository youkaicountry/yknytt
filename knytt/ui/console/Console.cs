using Godot;

// Thin C# bridge to LimboConsole (GDScript addon).
// Game code references this for IsOpen checks and signal wiring.
public partial class Console : Node
{
    [Signal] public delegate void ConsoleOpenEventHandler();
    [Signal] public delegate void ConsoleClosedEventHandler();

    private Node _limbo;

    public bool IsOpen => _limbo?.Call("is_open").AsBool() ?? false;

    public override void _Ready()
    {
        _limbo = GetNode("/root/LimboConsole");
        _limbo.Connect("toggled", new Callable(this, nameof(OnLimboToggled)));
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
