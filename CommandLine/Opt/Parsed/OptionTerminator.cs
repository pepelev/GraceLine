using CommandLine.Cursors;
using CommandLine.Text;
using Optional;

namespace CommandLine.Opt.Parsed
{
    public sealed class OptionTerminator : ParsedArgument
    {
        public static OptionTerminator Singleton { get; } = new();

        public static Option<Option<TokenStart>> Match(Cursor cursor) => cursor.MatchWholeToken()
            .Filter(token => new OldToken(token.CurrentToken.Value).Type == TokenType.DoubleHyphen)
            .Map(token => token.Next);

        public override string ToString() => "--";
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}