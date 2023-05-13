namespace YKnyttLib.Parser
{
    public struct CommandArg
    {
        public enum Type
        {
			StringArg,
			FloatArg,
			CmdNameArg,
			IntArg,
			UIntArg,
			BoolArg,
			HexArg,
			FileArg,
			NoArg,
		}

        public string Name { get; }
		public Type ArgType { get; }
		public bool Optional { get; }

		public CommandArg(string name, Type type, bool optional = false)
        {
			Name = name;
			ArgType = type;
			Optional = optional;
        }

		public override string ToString()
        {
			string brackets = Optional ? "[]" : "<>";
			return $"{brackets[0]}{Name}:{TypeToName(ArgType)}{brackets[1]}";
        }

		public static string TypeToName(Type arg)
        {
			switch (arg)
            {
				case Type.StringArg:
					return "string";

				case Type.FloatArg:
					return "float";

				case Type.CmdNameArg:
					return "command";

				case Type.FileArg:
					return "file";

				case Type.BoolArg:
					return "bool";

				case Type.HexArg:
					return "hex";

				case Type.IntArg:
					return "int";

				case Type.UIntArg:
					return "uint";

				case Type.NoArg:
					return "none";
            }

			throw new System.Exception("invalid type");
        }
    }
}
