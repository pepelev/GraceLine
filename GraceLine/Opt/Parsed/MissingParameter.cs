using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed class MissingParameter : ParsedOption
    {
        public MissingParameter(Located<Option> option)
        {
            Option = option;
        }

        public override Located<Option> Option { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}