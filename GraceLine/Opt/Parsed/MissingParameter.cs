using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed class MissingParameter : ParsedArgument
    {
        public MissingParameter(Option option, Source.Segment segment)
        {
            Option = option;
            Segment = segment;
        }

        public Option Option { get; }
        public Source.Segment Segment { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}