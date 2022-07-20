using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GraceLine.Text;
using Optional;

namespace GraceLine.Cursors
{
    public sealed class TokenStart : Cursor
    {
        private readonly ImmutableQueue<Token> tokens;

        public TokenStart(IEnumerable<string> tokens)
            : this(
                ImmutableQueue.CreateRange<Token>(
                    tokens.Select(
                        token => new VerbatimToken(
                            new Source.Segment(
                                new WholeInput(token)
                            )
                        )
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

        public Option<TokenStart> Next => tokens.Dequeue() is { IsEmpty: false } rest
            ? new TokenStart(rest, Offset + CurrentToken.WholeSegment.Length).Some()
            : Option.None<TokenStart>();

        public Token CurrentToken => tokens.Peek();
        public override Option<TokenStart> MatchWholeToken() => this.Some();

        public override Option<TokenMiddle> MatchShort()
            => this.SomeWhen(
                cursor => cursor.CurrentToken.Value.Type == TokenType.HyphenPrefixed
            ).Map(cursor => new TokenMiddle(1, cursor));
    }
}