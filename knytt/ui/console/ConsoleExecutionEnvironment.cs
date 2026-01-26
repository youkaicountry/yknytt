using YKnyttLib.Parser;

public partial class ConsoleExecutionEnvironment
{
    public CommandParser Parser { get; }
    public Console Console { get; }

    public ConsoleExecutionEnvironment(CommandParser parser, Console console)
    {
        Parser = parser;
        Console = console;
    }
}
