using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed partial class ParsedLongOption
    {
        public sealed class WithParameter : ParsedOption
        {
            public WithParameter(
                Located<Option> option,
                LongOptionMatch match,
                Located<string> argument)
            {
                Option = option;
                Argument = argument;
                Match = match;
            }

            public override Located<Option> Option { get; }
            public LongOptionMatch Match { get; }
            public Located<string> Argument { get; }
            public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
            public override T Accept<T>(ParsedArgument.Visitor<T> visitor) => visitor.Visit(this);
            public override string ToString() => $"{Option.Value} {Argument.Value}";
        }
    }
}