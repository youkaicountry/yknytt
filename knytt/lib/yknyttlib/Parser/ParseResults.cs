using System.Collections.Generic;

namespace YKnyttLib.Parser
{
    public partial class ParseResults
    {
        public List<CommandParseResult> Results { get; }

        public ParseResults()
        {
            Results = new List<CommandParseResult>();
        }

        public void AddResult(CommandParseResult result)
        {
            Results.Add(result);
        }
    }
}
