namespace CommandLine.Opt.Parsed
{
    public sealed class ParsedNumber : ParsedArgument
    {
        public ParsedNumber(long value)
        {
            Value = value;
        }

        public long Value { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}