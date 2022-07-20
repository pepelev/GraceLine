using System;
using CommandLine.Opt.Parsed;
using Optional;

namespace CommandLine.Opt
{
    public sealed class LongOption : Option2
    {
        private readonly string key;

        public LongOption(string key)
        {
            this.key = key;
        }

        public override string ToString() => $"--{key}";

        public override Optional.Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor)
        {
            const int LongOptionPrefix = 2;
            return cursor.MatchWholeToken()
                .Filter(cursor.CurrentToken.Type == TokenType.DoubleHyphenPrefixed)
                .FlatMap(
                    token =>
                    {
                        var value = token.CurrentToken.Value;
                        if (key.AsSpan().SequenceEqual(value.AsSpan(LongOptionPrefix)))
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedLongOption(
                                    this,
                                    LongOptionMatch.Full
                                ),
                                token.Next
                            ).Some();
                        }


                        if (key.AsSpan().StartsWith(value.AsSpan(LongOptionPrefix)))
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedLongOption(
                                    this,
                                    LongOptionMatch.Prefix
                                ),
                                token.Next
                            ).Some();
                        }

                        return Option.None<Cursor.Item<ParsedArgument>>();
                    }
                );
        }
    }
}