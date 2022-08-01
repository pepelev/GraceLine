using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public abstract class ParsedOption : ParsedArgument
    {
        public abstract Located<Option> Option { get; }
        public abstract T Accept<T>(Visitor<T> visitor);

        public new abstract class Visitor<T>
        {
            public abstract T Visit(ParsedShortOption argument);
            public abstract T Visit(ParsedShortOption.WithParameter argument);
            public abstract T Visit(ParsedLongOption argument);
            public abstract T Visit(ParsedLongOption.WithParameter argument);
            public abstract T Visit(ParsedLongOption.WithOptionalParameter argument);
            public abstract T Visit(ParsedNumber argument);
            public abstract T Visit(MissingArgument argument);
        }
    }
}