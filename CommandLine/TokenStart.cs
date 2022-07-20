using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Optional;

namespace CommandLine
{
    public sealed class TokenStart : Cursor
    {
        private readonly ImmutableQueue<Token> tokens;

        public TokenStart(IEnumerable<string> tokens)
            : this(
                ImmutableQueue.CreateRange(
                    tokens.Select(
                        token => new Token(token)
                    )
                ),
                0
            )
        {
        }

        private TokenStart(ImmutableQueue<Token> tokens, int offset)
        {
            this.tokens = tokens;
            Offset = offset;
        }

        public override int Offset { get; }
        public override Optional.Option<TokenStart> MatchWholeToken() => this.Some();

        public override Optional.Option<TokenMiddle> MatchShort() => CurrentToken
            .SomeWhen(token => token.Type == TokenType.HyphenPrefixed)
            .Map(token => new TokenMiddle(token, 1, this));

        public override Optional.Option<Item<Token>> MatchEntireToken() => new Item<Token>(tokens.Peek(), Next).Some();

        public override Optional.Option<ShortOption2> MatchShortOption() => CurrentToken.Some()
            .Filter(token => token.Type == TokenType.HyphenPrefixed)
            .FlatMap(token => new TokenMiddle(token, 1, this).MatchShortOption());

        public override Optional.Option<Cursor> Next => tokens.Dequeue() is { IsEmpty: false } rest
            ? new TokenStart(rest, Offset + CurrentToken.Length).Some<Cursor>()
            : Option.None<Cursor>();

        public Optional.Option<TokenStart> NextA => tokens.Dequeue() is { IsEmpty: false } rest
            ? new TokenStart(rest, Offset + CurrentToken.Length).Some<TokenStart>()
            : Option.None<TokenStart>();

        public override Token CurrentToken => tokens.Peek();
    }
}