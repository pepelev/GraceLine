﻿using GraceLine.Opt.Parsed;
using Optional;
using Optional.Unsafe;

namespace GraceLine.Cursors
{
    public abstract class Cursor
    {
        public abstract int Offset { get; }

        public Item<UnrecognizedOption> Skip() => MatchShort().Map(
            option => new Item<UnrecognizedOption>(
                new UnrecognizedOption(
                    option.Content[0].ToString(),
                    option.Segment(1)
                ),
                option.Feed(1)
            )
        ).Else(
            () => MatchWholeToken().Map(
                token => new Item<UnrecognizedOption>(
                    new UnrecognizedOption(token.CurrentToken.Value.Content, token.CurrentToken.WholeSegment),
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