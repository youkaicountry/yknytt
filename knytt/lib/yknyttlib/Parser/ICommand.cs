namespace YKnyttLib.Parser
{
    public interface ICommand
    {
        string Execute(object environment);
    }
}
