using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed class UnrecognizedOption : ParsedArgument
    {
        public UnrecognizedOption(Located<string> content)
        {
            Content = content;
        }

        public Located<string> Content { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
        public override string ToString() => Content.Value;
    }
}