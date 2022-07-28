using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed class UnrecognizedOption : ParsedArgument
    {
        public UnrecognizedOption(string content, Source.Segment segment)
        {
            Content = content;
            Segment = segment;
        }

        public string Content { get; }
        public Source.Segment Segment { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}