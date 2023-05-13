using System.Collections.Generic;

namespace YKnyttLib.Parser
{
    public class CommandParseResult
    {
        public string CommandName { get; }
        public Dictionary<string, string> Args;
        public CommandDeclaration Decl { get; internal set; }
        public int CurrentArg { get; private set; }
        public CommandParser.TerminationStatus Termination { get; internal set; }

        public CommandParseResult(string name)
        {
            CommandName = name;
            Args = new Dictionary<string, string>();
            CurrentArg = -1;
        }

        public void IncCurrentArg()
        {
            CurrentArg++;
        }
    }
}
