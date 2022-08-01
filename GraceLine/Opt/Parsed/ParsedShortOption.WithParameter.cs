using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed partial class ParsedShortOption
    {
        public sealed class WithParameter : ParsedOption
        {
            public WithParameter(Located<Option> option, Located<string> argument)
            {
                Option = option;
                Argument = argument;
            }

            public override Located<Option> Option { get; }
            public Located<string> Argument { get; }

            public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
        }
    }
}