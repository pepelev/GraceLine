namespace CommandLine.Opt.Parsed
{
    public sealed class OptionTerminator : ParsedArgument
    {
        public static OptionTerminator Singleton { get; } = new();
        public override string ToString() => "--";
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}