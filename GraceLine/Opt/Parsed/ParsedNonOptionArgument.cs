namespace GraceLine.Opt.Parsed
{
    public sealed class ParsedNonOptionArgument : ParsedArgument
    {
        public ParsedNonOptionArgument(string value)
        {
            Value = value;
        }

        public string Value { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}