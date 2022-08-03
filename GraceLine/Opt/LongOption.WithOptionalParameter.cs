using System;
using GraceLine.Cursors;
using GraceLine.Opt.Parsed;
using GraceLine.Text;
using Optional;

namespace GraceLine.Opt
{
    public sealed partial class LongOption
    {
        public sealed class WithOptionalParameter : Option
        {
            private readonly string key;

            public WithOptionalParameter(string key)
            {
                this.key = key;
            }

            public override string ToString() => $"--{key}[=?]";

            public override Option<Cursor.Item<ParsedOption>> Match(Cursor cursor)
            {
                return cursor.MatchWholeToken()
                    .Filter(static token => token.CurrentToken.Value.Type == TokenType.DoubleHyphenPrefixed)
                    .FlatMap(
                        this,
                        static (@this, token) =>
                        {
                            var keySpan = @this.key.AsSpan();
                            var argumentSpan = token.CurrentToken.Value.Content.AsSpan(LongOptionPrefixLength);
                            var delimiterIndex = argumentSpan.IndexOf('=');
                            if (delimiterIndex < 0)
                            {
                                return new Cursor.Item<ParsedOption>(
                                    new ParsedLongOption.WithOptionalParameter(
                                        new Located<Option>.WholeToken(
                                            @this,
                                            token.CurrentToken
                                        ),
                                        Optional.Option.None<Located<string>>()
                                    ),
                                    token.Next.Upcast()
                                ).Some();
                            }

                            if (delimiterIndex == 0)
                            {
                                return Optional.Option.None<Cursor.Item<ParsedOption>>();
                            }

                            var keyRange = ..delimiterIndex;
                            var valueRange = (delimiterIndex + 1)..;
                            if (argumentSpan[keyRange].TryMatch(keySpan) is { })
                            {
                                return new Cursor.Item<ParsedOption>(
                                    new ParsedLongOption.WithOptionalParameter(
                                        new Located<Option>.Plain(
                                            @this,
                                            token.CurrentToken,
                                            token.CurrentToken.Segment(..delimiterIndex)
                                        ),
                                        new Located<string>.Plain(
                                            new string(argumentSpan[valueRange]),
                                            token.CurrentToken,
                                            token.CurrentToken.Segment(valueRange)
                                        ).Some<Located<string>>()
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
}