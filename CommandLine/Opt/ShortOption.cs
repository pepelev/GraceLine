using CommandLine.Inputs;
using CommandLine.Opt.Parsed;

namespace CommandLine.Opt
{
    public sealed class ShortOption : Option<ParsedArgument>
    {
        private readonly char key;

        public ShortOption(char key)
        {
            this.key = key;
        }

        public override string ToString()
        {
            return $"-{key}";
        }

        public override Optional.Option<(ParsedArgument Result, Input Tail)> Match(Input input)
        {
            return input.ParseShortOption(key).Map(
                tail => (new ParsedShortOption(this) as ParsedArgument, tail)
            );
        }
    }

    public sealed class NumberOption : Option<ParsedArgument>
    {
        public static NumberOption Singleton { get; } = new();

        public override Optional.Option<(ParsedArgument Result, Input Tail)> Match(Input input) =>
            input.ParseNumberOption();
    }
}