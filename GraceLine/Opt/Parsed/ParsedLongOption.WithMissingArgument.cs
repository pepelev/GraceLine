using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed partial class ParsedLongOption
    {
        public sealed class WithMissingArgument : ParsedOption
        {
            public WithMissingArgument(Located<Option> option, LongOptionMatch match)
            {
                Option = option;
                Match = match;
            }

            public override Located<Option> Option { get; }
            public LongOptionMatch Match { get; }
            public override T Accept<T>(ParsedArgument.Visitor<T> visitor) => visitor.Visit(this);
            public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
        }
    }
}