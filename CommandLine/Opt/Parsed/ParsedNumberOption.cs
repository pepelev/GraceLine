namespace CommandLine.Opt.Parsed
{
    public sealed class ParsedNumberOption : ParsedArgument
    {
        public ParsedNumberOption(long value)
        {
            Value = value;
        }

        public long Value { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}