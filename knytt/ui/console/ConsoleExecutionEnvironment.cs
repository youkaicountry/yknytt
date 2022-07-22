using YKnyttLib.Parser;

public class ConsoleExecutionEnvironment
{
    public CommandParser Parser { get; }
    public Console Console { get; }

    public ConsoleExecutionEnvironment(CommandParser parser, Console console)
    {
        Parser = parser;
        Console = console;
    }
}
