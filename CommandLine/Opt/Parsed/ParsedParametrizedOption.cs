namespace CommandLine.Opt.Parsed
{
    public sealed class ParsedParametrizedOption : ParsedArgument
    {
        public ParsedParametrizedOption(Option<ParsedArgument> option, string parameter)
        {
            Option = option;
            Parameter = parameter;
        }

        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
        public Option<ParsedArgument> Option { get; }
        public string Parameter { get; }
    }
}