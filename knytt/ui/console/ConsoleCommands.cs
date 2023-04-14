using Godot;
using YKnyttLib.Parser;

public static class ConsoleCommands
{
    public static CommandSet BuildCommandSet()
    {
        var cs = new CommandSet();
        cs.AddCommand(new CommandDeclaration("speed", "View or set the speed of the game", false, SpeedCommand.NewSpeedCommand, new CommandArg("value", CommandArg.Type.FloatArg, optional: true)));
        cs.AddCommand(new CommandDeclaration("help", "View help for a given command", false, HelpCommand.NewHelpCommand, new CommandArg("name", CommandArg.Type.StringArg, optional: true)));
        cs.AddCommand(new CommandDeclaration("list", "View the list of commands", false, ListCommand.NewListCommand));
        return cs;
    }

    public partial class SpeedCommand : ICommand
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

    public partial class HelpCommand : ICommand
    {
        string name;

        public HelpCommand(CommandParseResult result)
        {
            name = result.Args["name"];
        }

        public static ICommand NewHelpCommand(CommandParseResult result)
        {
            return new HelpCommand(result);
        }

        public string Execute(object environment)
        {
            var env = (ConsoleExecutionEnvironment)environment;

            if (name == null)
            {
                env.Console.AddMessage($"Type list to see all commands, or help <command> to see help on a specific command");
                return null;
            }

            if (!env.Parser.Commands.Name2Command.ContainsKey(name))
            {
                return $"Cannot show help for unknown command: {name}";
            }

            var decl = env.Parser.Commands.Name2Command[name];
            env.Console.AddMessage($"{decl.Description}");
            env.Console.AddMessage($"Usage: {decl}");

            return null;
        }
    }

    public partial class ListCommand : ICommand
    {
        public ListCommand(CommandParseResult result)
        {
        }

        public static ICommand NewListCommand(CommandParseResult result)
        {
            return new ListCommand(result);
        }

        public string Execute(object environment)
        {
            var env = (ConsoleExecutionEnvironment)environment;

            var list = env.Parser.Commands.MakeList(true);
            env.Console.AddMessage(string.Join("\n", list));

            return null;
        }
    }
}
