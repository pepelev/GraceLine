using System;
using CommandLine.Opt.Parsed;
using Optional;
using Optional.Unsafe;

namespace CommandLine
{
    public abstract class Cursor
    {
        public abstract int Offset { get; }

        public Item<UnrecognizedOption> Skip() => MatchShort().Map(
            option => new Item<UnrecognizedOption>(
                new UnrecognizedOption(
                    option.Content[0].ToString()
                ),
                option.Skip(1)
            )
        ).Else(
            () => MatchWholeToken().Map(
                token => new Item<UnrecognizedOption>(
                    new UnrecognizedOption(token.CurrentToken.Value),
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
            public Item<K> Map<K>(Func<T, K> map) => new(map(Value), Next);
        }
    }
}