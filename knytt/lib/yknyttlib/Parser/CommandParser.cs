using System.Text;
using System.Text.RegularExpressions;

namespace YKnyttLib.Parser
{
    public class CommandParser
    {
        public enum TerminationStatus
        {
            NoTermination,
            InputTermination,
            CommandTermination,
        }

        struct SkipResult
        {
            public string Remaining;
            public TerminationStatus Termination;
            public bool Skipped;
        }

        struct ParseNextResult
        {
            public CommandParseResult Results;
            public string Remaining;
            public string Error;
        }

        struct CommandNameResult
        {
            public string Name;
            public string Error;
        }

        struct ParseArgsResult
        {
            public string Remaining;
            public string Error;
        }

        struct TypeParseResult
        {
            public string Value;
            public int Consumed;
            public string Error;
        }

        public struct ParseCallResult
        {
            public ParseResults Results;
            public string Error;
        }

        const char CommandTerminator = ';';

        private Regex commandNameRE = new Regex(@"^(\S+\.)?\S+", RegexOptions.Compiled);
        private Regex skipRE = new Regex(@"^\s*", RegexOptions.Compiled);
        private Regex terminatorRE = new Regex(@"^(;|$)", RegexOptions.Compiled);
        private Regex simpleStringRE = new Regex(@"^[^\s""';]+", RegexOptions.Compiled);
        private Regex floatRE = new Regex(@"^[+-]?((\d+(\.\d*)?)|(\.\d+))", RegexOptions.Compiled);
        private Regex uintRE = new Regex(@"^[+]?[0-9]+", RegexOptions.Compiled);
        private Regex intRE = new Regex(@"^[+-]?[0-9]+", RegexOptions.Compiled);
        private Regex boolRE = new Regex(@"^(?<false>[Ff][Aa][Ll][Ss][Ee]|0)|(?<true>[Tt][Rr][Uu][Ee]|1)", RegexOptions.Compiled);
        private Regex hexRE = new Regex(@"^0x[0-9a-fA-F]+", RegexOptions.Compiled);

        public CommandSet Commands { get; }

        public CommandParser(CommandSet commands)
        {
            Commands = commands;
        }

        public ParseCallResult Parse(string commands)
        {
            var input = commands;
            var invs = new ParseResults();

            // Initial skip
            var s = parseSkip(input, null, false);
            input = s.Remaining;

            // Loop until all input has been consumed
            while (input.Length > 0)
            {
                var p = parseNextCommand(input);
                if (p.Results != null) { invs.AddResult(p.Results); }
                if (p.Error != null) { return new ParseCallResult { Results = invs, Error = p.Error }; }

                if (p.Results.Termination == TerminationStatus.NoTermination || p.Results.Termination == TerminationStatus.InputTermination) { break; }
            }

            return new ParseCallResult { Results = invs, Error = null };
        }

        private ParseNextResult parseNextCommand(string input)
        {
            // Parse command name
            var n = parseCommandName(input);
            if (n.Error != null) { return new ParseNextResult { Remaining = null, Results = null, Error = n.Error }; }

            // Advance the input buffer
            input = input.Substring(n.Name.Length);

            // Create the invocation object
            var inv = new CommandParseResult(n.Name);
            if (Commands.Name2Command.ContainsKey(n.Name)) { inv.Decl = Commands.Name2Command[n.Name]; }
            else
            {
                parseSkip(input, inv, true);
                return new ParseNextResult { Remaining = null, Results = inv, Error = "unknown Command" };
            }

            var r = parseArgs(input, inv);
            if (r.Error != null) { return new ParseNextResult { Results = inv, Remaining = input, Error = r.Error }; }

            // Skip space and check termination
            var s = parseSkip(input, inv, false);
            input = s.Remaining;
            inv.Termination = s.Termination;

            return new ParseNextResult { Remaining = input, Results = inv, Error = null };

        }

        private CommandNameResult parseCommandName(string input)
        {
            var m = commandNameRE.Match(input);
            if (!m.Success)
            {
                return new CommandNameResult { Name = null, Error = "invalid command name" };
            }

            return new CommandNameResult { Name = m.Value, Error = null };
        }

        private SkipResult parseSkip(string input, CommandParseResult inv, bool incArgs)
        {
            var term = TerminationStatus.NoTermination;
            var skipped = false;

            // skip whitespace
            var m = skipRE.Match(input);
            if (m.Length > 0)
            {
                skipped = true;
                input = input.Substring(m.Length);
            }

            //search for terminator
            m = terminatorRE.Match(input);
            if (m.Success)
            {
                input = input.Substring(m.Length);
                term = (m.Length > 0 && m.Value[0] == CommandTerminator) ? TerminationStatus.CommandTermination : TerminationStatus.InputTermination;
            }

            m = skipRE.Match(input);
            if (m.Length > 0)
            {
                skipped = true;
                input = input.Substring(m.Length);
            }

            if (skipped && incArgs) { inv.IncCurrentArg(); }
            
            return new SkipResult { Remaining = input, Termination = term, Skipped = skipped };
        }

        private ParseArgsResult parseArgs(string input, CommandParseResult inv)
        {
            // Loop through the expected arguments
            int i = 0;
            foreach(var arg in inv.Decl.Args)
            {
                // Skip whitespace
                var s = parseSkip(input, inv, true);
                input = s.Remaining;
                if (s.Termination != TerminationStatus.NoTermination)
                {
                    if (arg.Optional)
                    {
                        inv.Args[arg.Name] = null;
                        return new ParseArgsResult { Remaining = input, Error = null };
                    }

                    return new ParseArgsResult { Remaining = input, Error = $"missing param: {arg.Name}" };
                }

                // If there was no skip here, the parameters have been melded together
                if (!s.Skipped)
                {
                    if (i == 0) { return new ParseArgsResult { Remaining = input, Error = $"invalid param: {inv.Decl.Args[i].Name}" }; }
                    return new ParseArgsResult { Remaining = input, Error = $"invalid param: {inv.Decl.Args[i - 1].Name}" };
                }

                TypeParseResult res = new TypeParseResult(); ;
                switch (arg.ArgType)
                {
                    case CommandArg.Type.StringArg:
                        res = parseString(input);
                        break;

                    case CommandArg.Type.FloatArg:
                        res = parseBasicRE(floatRE, input);
                        break;

                    case CommandArg.Type.IntArg:
                        res = parseBasicRE(intRE, input);
                        break;

                    case CommandArg.Type.UIntArg:
                        res = parseBasicRE(uintRE, input);
                        break;

                    case CommandArg.Type.BoolArg:
                        res = parseBool(input);
                        break;

                    case CommandArg.Type.HexArg:
                        res = parseBasicRE(hexRE, input);
                        break;

                    case CommandArg.Type.FileArg:
                        res = parseString(input);
                        break;
                }

                // Consume the match
                input = input.Substring(res.Consumed);

                // Check for an error during the match
                if (res.Error != null) { return new ParseArgsResult { Remaining = input, Error = $"{res.Error}: {arg.Name}" }; }

                // Store the argument value in the invocation
                inv.Args[arg.Name] = res.Value;

                i++;
            }

            return new ParseArgsResult { Remaining = input, Error = null };
        }

        private TypeParseResult parseBool(string input)
        {
            if (input.Length == 0) { return new TypeParseResult { Value = null, Consumed = 0, Error = "missing param" }; }

            var m = boolRE.Match(input);
            if (!m.Success) { return new TypeParseResult { Value = null, Consumed = 0, Error = "invalid param" }; }

            if (m.Groups[1].Name.Equals("true")) { return new TypeParseResult { Value = "true", Consumed = m.Length, Error = null }; }
            else if (m.Groups[1].Name.Equals("false")) { return new TypeParseResult { Value = "true", Consumed = m.Length, Error = null }; }

            return new TypeParseResult { Value = null, Consumed = 0, Error = "invalid param" };
        }

        private TypeParseResult parseBasicRE(Regex re, string input)
        {
            if (input.Length == 0) { return new TypeParseResult { Value = null, Consumed = 0, Error = "missing param" }; }

            var m = re.Match(input);
            if (!m.Success) { return new TypeParseResult { Value = null, Consumed = 0, Error = "invalid param" }; }

            return new TypeParseResult { Value = m.Value, Consumed = m.Length, Error = null };
        }

        private TypeParseResult parseString(string input)
        {
            if (input.Length == 0) { return new TypeParseResult { Value = null, Consumed = 0, Error = "missing param" }; }

            if (input[0] == '"' || input[0] == '\'') { return parseQuotedString(input); }

            return parseBasicRE(simpleStringRE, input);
        }

        private TypeParseResult parseQuotedString(string input)
        {
            // Record the quote type
            var quote = input[0];

            var output = new StringBuilder();
            bool escape = false; // true if we are in an escape sequence

            // Iterate through the input until we find the closing quote
            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i + 1];
                if (escape)
                {
                    escape = false;

                    // If we're in an escape sequence, append the character and continue to the next character
                    if (c == '\\' || c == '"' || c == '\'')
                    {
                        output.Append(c);
                        continue;
                    }

                    // Otherwise just append the slash and carry on parsing this character
                    output.Append('\\');
                }

                // If this is the start of an escape sequence, continue to the next character in escape mode
                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                // If end quote, return the string
                if (c == quote) { return new TypeParseResult { Value = output.ToString(), Consumed = i + 2, Error = null }; }

                // Otherwise just append the character and continue
                output.Append(c);
            }

            // No closing quote was found
            return new TypeParseResult { Value = null, Consumed = 0, Error = "invalid param: missing closed quote" };
        }
    }
}
