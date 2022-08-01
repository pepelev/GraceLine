using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed partial class ParsedShortOption : ParsedOption
    {
        public ParsedShortOption(Located<Option> option)
        {
            Option = option;
        }

        public override Located<Option> Option { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
        public override T Accept<T>(ParsedArgument.Visitor<T> visitor) => visitor.Visit(this);
        public override string ToString() => Option.Value.ToString();
    }
}