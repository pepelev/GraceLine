using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed class ParsedShortOption : ParsedArgument
    {
        public ParsedShortOption(Located<Option> option)
        {
            Option = option;
        }

        public Located<Option> Option { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}