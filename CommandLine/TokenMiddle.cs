using System;
using Optional;

namespace CommandLine
{
    public sealed class TokenMiddle : Cursor
    {
        public TokenMiddle(Token token, int index, TokenStart start)
        {
            this.token = token;
            this.index = index;
            this.start = start;
        }

        private readonly Token token;
        private readonly int index;
        private readonly TokenStart start;

        public override int Offset => start.Offset + index;
        public override Token CurrentToken => start.CurrentToken;
        public override Option<TokenStart> MatchWholeToken() => Option.None<TokenStart>();
        public override Option<TokenMiddle> MatchShort() => this.Some();
        public override Option<Item<Token>> MatchEntireToken() => Option.None<Item<Token>>();

        public override Option<ShortOption2> MatchShortOption() => new ShortOption2(
            new Item<char>(
                token.Value[index],
                Next
            ),
            new Item<TokenTail>(
                new TokenTail(token.Value[(index + 1)..]),
                start.Next
            ),
            new Item<TokenTail>(
                new TokenTail(token.Value[index..]),
                Next
            )
        ).Some();

        public Option<Cursor> Skip(int chars)
        {
            if (chars == 0)
            {
                return this.Some<Cursor>();
            }

            if (chars < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chars), "must not be negative");
            }

            var newIndex = index + chars;
            if (newIndex == token.Length)
            {
                return start.Next;
            }

            if (newIndex < token.Length)
            {
                return new TokenMiddle(token, newIndex, start).Some<Cursor>();
            }

            throw new ArgumentOutOfRangeException(nameof(chars), "must not exceed chars left in current token");
        }

        public ReadOnlySpan<char> Content => token.Value.AsSpan(index);
        public bool AtTokenStart => index == 1;

        public override Option<Cursor> Next => index == token.Length - 1
            ? start.Next
            : new TokenMiddle(token, index + 1, start).Some<Cursor>();

        // todo skip
        // С одной стороны хочется унести знание о структуре в конкретные опции, но здесь нужно знать что пропускать короткую опцию или целый токен
    }
}