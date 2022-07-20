namespace CommandLine.Opt.Parsed
{
    public sealed class ParsedParametrizedOption : ParsedArgument
    {
        public ParsedParametrizedOption(Option option, string parameter)
        {
            Option = option;
            Parameter = parameter;
        }

        public Option Option { get; }
        public string Parameter { get; }

        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}