using GraceLine.Text;
using Optional;

namespace GraceLine.Opt.Parsed
{
    public sealed partial class ParsedLongOption : ParsedArgument
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