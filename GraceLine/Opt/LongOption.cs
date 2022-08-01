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
            // todo introduce general place for such constants
            const int LongOptionPrefix = 2;
            return cursor.MatchWholeToken()
                .Filter(static token => token.CurrentToken.Value.Type == TokenType.DoubleHyphenPrefixed)
                .FlatMap(
                    this,
                    static (@this, token) =>
                    {
                        var currentToken = token.CurrentToken;
                        var value = currentToken.Value.Content;
                        if (@this.key.AsSpan().SequenceEqual(value.AsSpan(LongOptionPrefix)))
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedLongOption(
                                    new Located<Option>.WholeToken(
                                        @this,
                                        currentToken
                                    ),
                                    LongOptionMatch.Full
                                ),
                                token.Next.Upcast()
                            ).Some();
                        }

                        if (@this.key.AsSpan().StartsWith(value.AsSpan(LongOptionPrefix)))
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedLongOption(
                                    new Located<Option>.WholeToken(
                                        @this,
                                        currentToken
                                    ),
                                    LongOptionMatch.Prefix
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