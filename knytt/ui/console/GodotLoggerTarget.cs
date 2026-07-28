using Godot;
using YKnyttLib.Logging;

// Sends KnyttLogger output to Godot's output.
public class GodotLoggerTarget : IKnyttLoggerTarget
{
    public void NewMessage(KnyttLogMessage message)
    {
        if (message.Level >= KnyttLogger.Level.ERROR) { GD.PrintErr(message.Render()); }
        else { GD.Print(message.Render()); }
    }
}
