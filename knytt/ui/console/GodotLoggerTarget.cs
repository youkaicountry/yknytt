using Godot;
using YKnyttLib.Logging;

// Sends KnyttLogger output to Godot's own output/log. Without a target registered,
// KnyttLogger.Log() iterates an empty list and every message is silently dropped.
public class GodotLoggerTarget : IKnyttLoggerTarget
{
    public void NewMessage(KnyttLogMessage message)
    {
        if (message.Level >= KnyttLogger.Level.ERROR) { GD.PrintErr(message.Render()); }
        else { GD.Print(message.Render()); }
    }
}
