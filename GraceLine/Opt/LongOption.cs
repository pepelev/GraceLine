using System;
using GraceLine.Cursors;
using GraceLine.Opt.Parsed;
using GraceLine.Text;
using Optional;

namespace GraceLine.Opt
{
    public sealed partial class LongOption : Option
    {
        private const int LongOptionPrefixLength = 2;

        private readonly string key;

        public LongOption(string key)
        {
            this.key = key;
        }

        public override string ToString() => $"--{key}";

        public override Option<Cursor.Item<ParsedOption>> Match(Cursor cursor)
        {
            return cursor.MatchWholeToken()
                .Filter(static token => token.CurrentToken.Value.Type == TokenType.DoubleHyphenPrefixed)
                .FlatMap(
                    this,
                    static (@this, token) =>
                    {
                        var currentToken = token.CurrentToken;
                        var value = currentToken.Value.Content;
                        if (@this.key.AsSpan().SequenceEqual(value.AsSpan(LongOptionPrefixLength)))
                        {
                            return new Cursor.Item<ParsedOption>(
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

                        if (@this.key.AsSpan().StartsWith(value.AsSpan(LongOptionPrefixLength)))
                        {
                            return new Cursor.Item<ParsedOption>(
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

                        return Optional.Option.None<Cursor.Item<ParsedOption>>();
                    }
                );
        }
    }
}