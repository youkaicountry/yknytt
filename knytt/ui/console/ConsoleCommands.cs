using Godot;
using YKnyttLib.Parser;

public static class ConsoleCommands
{
    public static CommandSet BuildCommandSet()
    {
        var cs = new CommandSet();
        cs.AddCommand(new CommandDeclaration("speed", "View or set the speed of the game", false, null, new CommandArg("value", CommandArg.Type.FloatArg, optional: true)));

        return cs;
    }
}
