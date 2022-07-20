using System.Collections.Generic;

namespace CommandLine.Opt.Parsed
{
    public sealed class LongOptionAmbiguity : ParsedArgument
    {
        public LongOptionAmbiguity(IReadOnlyCollection<Option> options)
        {
            Options = options;
        }

        // todo maybe restrict arguments only by long options
        public IReadOnlyCollection<Option> Options { get; }

        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}