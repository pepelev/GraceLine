namespace CommandLine.Opt.Parsed
{
    public sealed class ParsedParametrizedOption : ParsedArgument
    {
        public ParsedParametrizedOption(Option2 option, string parameter)
        {
            Option = option;
            Parameter = parameter;
        }

        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
        public Option2 Option { get; }
        public string Parameter { get; }
    }
}