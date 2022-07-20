namespace CommandLine.Opt.Parsed
{
    public sealed class OptionTerminator : ParsedArgument
    {
        public static Optional.Option<Optional.Option<TokenStart>> Match(Cursor cursor) => cursor.MatchWholeToken()
            .Filter(token => token.CurrentToken.Type == TokenType.DoubleHyphen)
            .Map(token => token.NextA);

        public static OptionTerminator Singleton { get; } = new();
        public override string ToString() => "--";
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}