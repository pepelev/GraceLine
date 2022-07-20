namespace CommandLine.Opt.Parsed
{
    public sealed class ParsedLongOption : ParsedArgument
    {
        public ParsedLongOption(Option option, LongOptionMatch match)
        {
            Option = option;
            Match = match;
        }

        public Option Option { get; }
        public LongOptionMatch Match { get; }

        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}