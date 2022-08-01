using GraceLine.Opt.Parsed;
using GraceLine.Text;
using Optional;
using Optional.Unsafe;

namespace GraceLine.Cursors
{
    public abstract class Cursor
    {
        public abstract int Offset { get; }

        public Item<UnrecognizedOption> Skip() => MatchShort().Map(
            cursor => new Item<UnrecognizedOption>(
                new UnrecognizedOption(
                    new Located<string>.Plain(
                        cursor.Content[0].ToString(),
                        cursor.CurrentToken,
                        cursor.Segment(1)
                    )
                ),
                cursor.Feed(1)
            )
        ).Else(
            () => MatchWholeToken().Map(
                token => new Item<UnrecognizedOption>(
                    new UnrecognizedOption(
                        new Located<string>.WholeToken(
                            token.CurrentToken.Value.Content,
                            token.CurrentToken
                        )
                    ),
                    token.Next.Upcast()
                )
            )
        ).ValueOrFailure();

        public abstract Option<TokenStart> MatchWholeToken();
        public abstract Option<TokenMiddle> MatchShort();

        public readonly struct Item<T>
        {
            public Item(T value, Option<Cursor> next)
            {
                Value = value;
                Next = next;
            }

            public T Value { get; }
            public Option<Cursor> Next { get; }
        }
    }
}