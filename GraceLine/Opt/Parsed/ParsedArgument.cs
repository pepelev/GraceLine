namespace GraceLine.Opt.Parsed
{
    public abstract class ParsedArgument
    {
        public abstract T Accept<T>(Visitor<T> visitor);

        public abstract class Visitor<T>
        {
            public abstract T Visit(ParsedNonOptionArgument argument);
            public abstract T Visit(ParsedShortOption option);
            public abstract T Visit(ParsedShortOption.WithParameter option);
            public abstract T Visit(ParsedLongOption option);
            public abstract T Visit(ParsedLongOption.WithParameter option);
            public abstract T Visit(ParsedLongOption.WithOptionalParameter option);
            public abstract T Visit(ParsedLongOption.WithMissingArgument option);
            public abstract T Visit(ParsedNumber option);
            public abstract T Visit(OptionTerminator argument);
            public abstract T Visit(UnrecognizedOption option);
            public abstract T Visit(LongOptionAmbiguity argument);
            public abstract T Visit(MissingArgument argument);
        }
    }
}