using System.Collections.Generic;

namespace GraceLine
{
    public sealed class ParsedArguments
    {
        public IEnumerable<string> NonOptions { get; }
        public OptionArguments Options { get; }
    }
}