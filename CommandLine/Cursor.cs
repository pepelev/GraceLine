using System;
using CommandLine.Opt.Parsed;
using Optional.Unsafe;

namespace CommandLine
{
    public abstract class Cursor
    {
        public Item<UnrecognizedOption> Skip() => MatchShortOption().Map(
            option => option.Key.Map(key => new UnrecognizedOption(key.ToString()))
        ).ValueOr(
            () => MatchEntireToken().Map(
                item => item.Map(token => new UnrecognizedOption(token.Value))
            ).ValueOrFailure()
        );

        public abstract int Offset { get; }
        public abstract Token CurrentToken { get; }
        public abstract Optional.Option<Cursor> Next { get; }


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

        public abstract Optional.Option<Item<Token>> MatchEntireToken();
        public abstract Optional.Option<ShortOption2> MatchShortOption();
    }

    //internal sealed class End : Cursor
    //{
    //    public static End Singleton { get; } = new();
    //    public override bool AtEnd => true;
    //    public override int Offset => int.MaxValue;
    //    public override Optional.Option<Item<Token>> MatchEntireToken() => Option.None<Item<Token>>();
    //    public override Optional.Option<ShortOption> MatchShortOption() => Option.None<ShortOption>();
    //}
}