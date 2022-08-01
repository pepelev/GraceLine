namespace GraceLine.Opt.Parsed
{
    public abstract class ParsedArgument
    {
        public abstract T Accept<T>(Visitor<T> visitor);

        public abstract class Visitor<T>
        {
            public abstract T Visit(ParsedNonOptionArgument argument);
            public abstract T Visit(ParsedShortOption argument);
            public abstract T Visit(ParsedShortOption.WithParameter argument);
            public abstract T Visit(ParsedLongOption argument);
            public abstract T Visit(ParsedLongOption.WithParameter argument);
            public abstract T Visit(ParsedLongOption.WithOptionalParameter argument);
            public abstract T Visit(ParsedNumber argument);
            public abstract T Visit(OptionTerminator argument);
            public abstract T Visit(UnrecognizedOption argument);
            public abstract T Visit(LongOptionAmbiguity argument);
            public abstract T Visit(MissingParameter argument);
        }
    }
}