using Godot;
using System;
using YKnyttLib.Logging;

public class GodotLoggerTarget : IKnyttLoggerTarget
{
    public void NewMessage(KnyttLogMessage message)
    {
        GD.Print(message.Render());
    }
}
