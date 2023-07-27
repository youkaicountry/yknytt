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

        public bool GetArgAsBool(string arg)
        {
            var a = Args[arg];
            if (a == null) { return false; }
            a = a.ToLower();
            switch(a)
            {
                case "true":
                case "1":
                return true;
                
                case "false":
                case "0":
                return false;
            }

            throw new System.Exception("Invalid bool");
        }
    }
}
