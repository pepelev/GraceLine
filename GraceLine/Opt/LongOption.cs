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
                        var argument = value.AsSpan(LongOptionPrefixLength);
                        if (argument.TryMatch(@this.key) is { } match)
                        {
                            return new Cursor.Item<ParsedOption>(
                                new ParsedLongOption(
                                    new Located<Option>.WholeToken(@this, currentToken),
                                    match
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