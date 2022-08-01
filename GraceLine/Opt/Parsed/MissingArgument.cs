using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed class MissingArgument : ParsedOption
    {
        public MissingArgument(Located<Option> option)
        {
            Option = option;
        }

        public override Located<Option> Option { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
        public override T Accept<T>(ParsedArgument.Visitor<T> visitor) => visitor.Visit(this);
        public override string ToString() => Option.Value.ToString();
    }
}