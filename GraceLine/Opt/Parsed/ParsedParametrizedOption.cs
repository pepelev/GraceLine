namespace GraceLine.Opt.Parsed
{
    public sealed class ParsedParametrizedOption : ParsedArgument
    {
        public ParsedParametrizedOption(Option option, string argument)
        {
            Option = option;
            Argument = argument;
        }

        public Option Option { get; }
        public string Argument { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}