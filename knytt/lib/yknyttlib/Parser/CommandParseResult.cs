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
            var b = GetArgAsNullableBool(arg);
            return b.GetValueOrDefault();
        }

        public bool? GetArgAsNullableBool(string arg)
        {
            var a = Args[arg];
            if (a == null) { return null; }
            switch(a)
            {
                case "true":
                return true;
                
                case "false":
                return false;
            }

            throw new System.Exception("Invalid bool");
        }
    }
}
