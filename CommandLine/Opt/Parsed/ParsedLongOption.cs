namespace CommandLine.Opt.Parsed
{
    public sealed class ParsedLongOption : ParsedArgument
    {
        public ParsedLongOption(Opt.Option<ParsedArgument> option, LongOptionMatch match)
        {
            Option = option;
            Match = match;
        }

        public Opt.Option<ParsedArgument> Option { get; }
        public LongOptionMatch Match { get; }

        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}