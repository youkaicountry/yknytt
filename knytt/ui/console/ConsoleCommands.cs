using Godot;
using YKnyttLib.Parser;

public static class ConsoleCommands
{
    public static CommandSet BuildCommandSet()
    {
        var cs = new CommandSet();
        cs.AddCommand(new CommandDeclaration("speed", "View or set the speed of the game", false, SpeedCommand.NewSpeedCommand, new CommandArg("value", CommandArg.Type.FloatArg, optional: true)));

        return cs;
    }

    public class SpeedCommand : ICommand
    {
        float? value;
        const float MIN_SPEED = 0.1f;
        const float MAX_SPEED = 5f;

        public SpeedCommand(CommandParseResult result)
        {
            if (result.Args["value"] == null) { value = null; return; }
            value = float.Parse(result.Args["value"]);
        }

        public static ICommand NewSpeedCommand(CommandParseResult result)
        {
            return new SpeedCommand(result);
        }

        public string Execute(object environment)
        {
            var env = (ConsoleExecutionEnvironment)environment;

            if (value == null)
            {
                env.Console.AddMessage($"Current speed: {GDKnyttDataStore.CurrentSpeed}");
                return null;
            }

            if (value < MIN_SPEED || value > MAX_SPEED) { return $"value should be between {MIN_SPEED} and {MAX_SPEED}"; }
            
            GDKnyttDataStore.CurrentSpeed = (float)value;
            env.Console.AddMessage($"Speed changed to {value}");

            return null;
        }
    }

}
