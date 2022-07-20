namespace CommandLine.Opt.Parsed
{
    public sealed class ParsedShortOption : ParsedArgument
    {
        public ParsedShortOption(Option option)
        {
            Option = option;
        }

        public Option Option { get; }

        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}