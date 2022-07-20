using CommandLine.Inputs;
using CommandLine.Opt.Parsed;

namespace CommandLine.Opt
{
    public sealed class ShortParameterizedOption : Option<ParsedArgument>
    {
        private readonly char key;

        public ShortParameterizedOption(char key)
        {
            this.key = key;
        }

        public override Optional.Option<(ParsedArgument Result, Input Tail)> Match(Input input) =>
            input.ParseParametrizedShortOption(key).Map(
                pair => (new ParsedParametrizedOption(this, pair.Parameter) as ParsedArgument, pair.Tail)
            );
    }
}