using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public abstract class ParsedOption : ParsedArgument
    {
        public abstract Located<Option> Option { get; }
        public abstract T Accept<T>(Visitor<T> visitor);

        public new abstract class Visitor<T>
        {
            public abstract T Visit(ParsedShortOption option);
            public abstract T Visit(ParsedShortOption.WithParameter option);
            public abstract T Visit(ParsedLongOption option);
            public abstract T Visit(ParsedLongOption.WithParameter option);
            public abstract T Visit(ParsedLongOption.WithOptionalParameter option);
            public abstract T Visit(ParsedLongOption.WithMissingArgument option);
            public abstract T Visit(ParsedNumber option);
            public abstract T Visit(MissingArgument option);
        }
    }
}