using GraceLine.Text;
using Optional;

namespace GraceLine.Opt.Parsed
{
    public sealed partial class ParsedLongOption
    {
        public sealed class WithOptionalParameter : ParsedOption
        {
            public WithOptionalParameter(
                Located<Option> option,
                Option<Located<string>> argument)
            {
                Option = option;
                Argument = argument;
            }

            public Located<Option> Option { get; }
            public Option<Located<string>> Argument { get; }
            public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
        }
    }
}