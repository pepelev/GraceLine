using System;
using GraceLine.Cursors;
using GraceLine.Opt.Parsed;
using GraceLine.Text;
using Optional;

namespace GraceLine.Opt
{
    public sealed class LongOption : Option
    {
        private readonly string key;

        public LongOption(string key)
        {
            this.key = key;
        }

        public override string ToString() => $"--{key}";

        public override Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor)
        {
            const int LongOptionPrefix = 2;
            return cursor.MatchWholeToken()
                .Filter(token => token.CurrentToken.Value.Type == TokenType.DoubleHyphenPrefixed)
                .FlatMap(
                    token =>
                    {
                        var currentToken = token.CurrentToken;
                        var value = currentToken.Value.Content;
                        if (key.AsSpan().SequenceEqual(value.AsSpan(LongOptionPrefix)))
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedLongOption(
                                    this,
                                    LongOptionMatch.Full,
                                    currentToken
                                ),
                                token.Next.Upcast()
                            ).Some();
                        }

                        if (key.AsSpan().StartsWith(value.AsSpan(LongOptionPrefix)))
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedLongOption(
                                    this,
                                    LongOptionMatch.Prefix,
                                    currentToken
                                ),
                                token.Next.Upcast()
                            ).Some();
                        }

                        return Optional.Option.None<Cursor.Item<ParsedArgument>>();
                    }
                );
        }
    }
}