using System;
using GraceLine.Text;
using Optional;

namespace GraceLine.Cursors
{
    public sealed class TokenMiddle : Cursor
    {
        private readonly int index;
        private readonly TokenStart start;

        public TokenMiddle(int index, TokenStart start)
        {
            this.index = index;
            this.start = start;
        }

        public override int Offset => start.Offset + index;
        public ReadOnlySpan<char> Content => CurrentToken.Value.Content.AsSpan(index);
        public bool AtTokenStart => index == 1;
        public Token CurrentToken => start.CurrentToken;
        public override Option<TokenStart> MatchWholeToken() => Option.None<TokenStart>();
        public override Option<TokenMiddle> MatchShort() => this.Some();

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
            if (newIndex == CurrentToken.Value.Content.Length)
            {
                return start.Next.Upcast();
            }

            if (newIndex < CurrentToken.Value.Content.Length)
            {
                return new TokenMiddle(newIndex, start).Some<Cursor>();
            }

            throw new ArgumentOutOfRangeException(nameof(chars), "must not exceed chars left in current token");
        }
    }
}