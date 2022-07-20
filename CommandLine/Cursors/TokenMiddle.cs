using System;
using CommandLine.Text;
using Optional;

namespace CommandLine.Cursors
{
    public sealed class TokenMiddle : Cursor
    {
        private readonly Token token;
        private readonly int index;
        private readonly TokenStart start;

        public TokenMiddle(Token token, int index, TokenStart start)
        {
            this.token = token;
            this.index = index;
            this.start = start;
        }

        public override int Offset => start.Offset + index;
        public ReadOnlySpan<char> Content => token.Value.AsSpan(index);
        public bool AtTokenStart => index == 1;
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
            if (newIndex == token.Length)
            {
                return start.Next.Upcast();
            }

            if (newIndex < token.Length)
            {
                return new TokenMiddle(token, newIndex, start).Some<Cursor>();
            }

            throw new ArgumentOutOfRangeException(nameof(chars), "must not exceed chars left in current token");
        }
    }
}