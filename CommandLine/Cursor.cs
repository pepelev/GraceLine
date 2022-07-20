using System;
using CommandLine.Opt.Parsed;
using Optional.Unsafe;

namespace CommandLine
{
    public abstract class Cursor
    {
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

        public abstract int Offset { get; }

        public readonly struct Item<T>
        {
            public Item(T value, Optional.Option<Cursor> next)
            {
                Value = value;
                Next = next;
            }

            public T Value { get; }
            public Optional.Option<Cursor> Next { get; }
            public Item<K> Map<K>(Func<T, K> map) => new(map(Value), Next);
        }

        public abstract Optional.Option<TokenStart> MatchWholeToken();
        public abstract Optional.Option<TokenMiddle> MatchShort();
    }
}