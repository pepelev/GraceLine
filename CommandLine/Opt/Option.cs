using System;
using CommandLine.Inputs;
using CommandLine.Opt.Parsed;
using Optional;

namespace CommandLine.Opt
{
    public abstract class Option<T> where T : ParsedArgument
    {
        public abstract Optional.Option<(T Result, Input Tail)> Match(Input input);
    }

    public abstract class Pattern<T>
    {
        public abstract Optional.Option<Cursor.Item<T>> Match(Cursor cursor);
    }

    public abstract class Option2
    {
        public abstract Optional.Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor);
    }

    public sealed class LongOption2 : Option2
    {
        private readonly string key;

        public LongOption2(string key)
        {
            this.key = key;
        }

        public override Optional.Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor)
        {
            if (cursor.CurrentToken.Type == TokenType.DoubleHyphenPrefixed)
            {
                var value = cursor.CurrentToken.Value;
                if (key.AsSpan().StartsWith(value.AsSpan(2)))
                {
                    //return new Cursor.Item<ParsedArgument>(
                    //    new ParsedLongOption(

                    //    )
                    //)
                }
            }

            return Option.None<Cursor.Item<ParsedArgument>>();
        }
    }
}