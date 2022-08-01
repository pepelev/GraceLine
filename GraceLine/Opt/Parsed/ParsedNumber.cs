using System.Globalization;
using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed class ParsedNumber : ParsedOption
    {
        public ParsedNumber(Located<Option> option, long value)
        {
            Option = option;
            Value = value;
        }

        public override Located<Option> Option { get; }
        public long Value { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
        public override T Accept<T>(ParsedArgument.Visitor<T> visitor) => visitor.Visit(this);
        public override string ToString() => $"-{Value.ToString(CultureInfo.InvariantCulture)}";
    }
}