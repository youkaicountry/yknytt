using System.Collections.Generic;
using System.Linq;

namespace YKnyttLib.Parser
{
    public class CommandSet
    {
        public List<CommandDeclaration> Commands { get; }
        public Dictionary<string, CommandDeclaration> Name2Command { get; }
        public int Revision { get; private set; }

        public CommandSet()
        {
            Commands = new List<CommandDeclaration>();
            Name2Command = new Dictionary<string, CommandDeclaration>();
        }

        public void AddCommand(CommandDeclaration decl)
        {
            Commands.Add(decl);
            Name2Command.Add(decl.Name, decl);
            Revision++;
        }

        public string[] MakeList(bool pretty)
        {
            var com = Commands.Where(x => !x.Hidden).OrderBy(x => x.Name);
            
            if (!pretty) { return com.Select(x => x.Name).ToArray(); }
            
            int max = com.Max(x => x.Name.Length);
            return com.Select(x => $"{x.Name.PadRight(max)} - {x.Description}").ToArray();
        }
    }
}
