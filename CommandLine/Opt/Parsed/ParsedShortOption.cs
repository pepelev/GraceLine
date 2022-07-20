namespace CommandLine.Opt.Parsed
{
    public sealed class ParsedShortOption : ParsedArgument
    {
        public ParsedShortOption(Option<ParsedArgument> option)
        {
            Option = option;
        }

        public Option<ParsedArgument> Option { get; }

        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}