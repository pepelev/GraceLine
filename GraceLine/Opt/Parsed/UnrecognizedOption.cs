namespace GraceLine.Opt.Parsed
{
    public sealed class UnrecognizedOption : ParsedArgument
    {
        public UnrecognizedOption(string content)
        {
            Content = content;
        }

        public string Content { get; }

        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}