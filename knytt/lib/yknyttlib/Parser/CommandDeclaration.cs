using System.Linq;
using YKnyttLib.Logging;

namespace YKnyttLib.Parser
{
    public class CommandDeclaration
    {
		public delegate ICommand CommandInstantiation(CommandParseResult result);

        public string Name { get; }
        public string Description { get; }
        public CommandInstantiation Instantiation { get; }
        public CommandArg[] Args { get; }
        public bool Hidden { get; }

        public CommandDeclaration(string name, string description, bool hidden, CommandInstantiation instantiation, params CommandArg[] args)
        {
			var req = true;
			foreach (var arg in args)
            {
				if (!arg.Optional)
                {
					if (!req)
					{
						string m = "cannot have a non-optional argument following an optional";
						KnyttLogger.Error(m);
						throw new System.Exception(m);
					}
                }
				else { req = false; }
            }

			Name = name;
			Description = description;
			Hidden = hidden;
			Instantiation = instantiation;
			Args = args;
        }

        public override string ToString()
        {
            return $"{Name} {string.Join(" ", Args.Select(x => x.ToString()))}";
        }
    }
}
