using CommandLine.Inputs;
using CommandLine.Opt.Parsed;

namespace CommandLine.Opt
{
    public sealed class LongParametrizedOption : Option<ParsedArgument>
    {
        private readonly string key;

        public LongParametrizedOption(string key)
        {
            this.key = key;
        }

        public override Optional.Option<(ParsedArgument Result, Input Tail)> Match(Input input) =>
            input.ParseParametrizedLongOption(key).Map(
                pair => (new ParsedParametrizedOption(this, pair.Parameter) as ParsedArgument, pair.Tail)
            );
    }
}