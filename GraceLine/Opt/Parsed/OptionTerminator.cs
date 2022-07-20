using GraceLine.Cursors;
using GraceLine.Text;
using Optional;

namespace GraceLine.Opt.Parsed
{
    public sealed class OptionTerminator : ParsedArgument
    {
        public OptionTerminator(Token token)
        {
            Token = token;
        }

        public Token Token { get; }

        public static Option<Cursor.Item<OptionTerminator>> Match(Cursor cursor) => cursor.MatchWholeToken()
            .Filter(token => token.CurrentToken.Value.Type == TokenType.DoubleHyphen)
            .Map(
                token => new Cursor.Item<OptionTerminator>(
                    new OptionTerminator(token.CurrentToken),
                    token.Next.Upcast()
                )
            );

        public override string ToString() => "--";
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}