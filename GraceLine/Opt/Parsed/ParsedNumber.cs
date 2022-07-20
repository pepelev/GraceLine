using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed class ParsedNumber : ParsedArgument
    {
        public ParsedNumber(long value, Token token, Source.Segment segment)
        {
            Value = value;
            Token = token;
            Segment = segment;
        }

        public long Value { get; }
        public Token Token { get; }
        public Source.Segment Segment { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}