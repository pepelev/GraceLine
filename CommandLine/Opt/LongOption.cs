using CommandLine.Inputs;
using CommandLine.Opt.Parsed;

namespace CommandLine.Opt
{
    public sealed class LongOption : Option<ParsedArgument>
    {
        private readonly string key;

        public LongOption(string key)
        {
            this.key = key;
        }

        public override string ToString() => $"--{key}";

        public override Optional.Option<(ParsedArgument Result, Input Tail)> Match(Input input) =>
            input.ParseLongOption(key).Map(
                result => (new ParsedLongOption(this, result.Match) as ParsedArgument, result.Tail)
            );
    }
}