using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed partial class ParsedLongOption : ParsedOption
    {
        public ParsedLongOption(Located<Option> option, LongOptionMatch match)
        {
            Option = option;
            Match = match;
        }

        public Located<Option> Option { get; }
        public LongOptionMatch Match { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}