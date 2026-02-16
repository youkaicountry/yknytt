using Godot;

// Thin C# wrapper over GDScript console addon.
// Game code references this for IsOpen checks and signal wiring.
public partial class Console : CanvasLayer
{
    [Signal] public delegate void ConsoleOpenEventHandler();
    [Signal] public delegate void ConsoleClosedEventHandler();

    public bool IsOpen { get; private set; }

    public void toggleConsole()
    {
        IsOpen = !IsOpen;
        if (IsOpen)
            EmitSignal(SignalName.ConsoleOpen);
        else
            EmitSignal(SignalName.ConsoleClosed);
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event.IsActionPressed("debug_console"))
        {
            toggleConsole();
            GetViewport().SetInputAsHandled();
        }
    }
}
