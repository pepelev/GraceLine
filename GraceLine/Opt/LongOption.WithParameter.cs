using System;
using GraceLine.Cursors;
using GraceLine.Opt.Parsed;
using GraceLine.Text;
using Optional;

namespace GraceLine.Opt
{
    public sealed partial class LongOption
    {
        public sealed class WithParameter : Option
        {
            private readonly string key;

            public WithParameter(string key)
            {
                this.key = key;
            }

            public override string ToString() => $"--{key} ?";

            public override Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor)
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
                            if (delimiterIndex == -1)
                            {
                                if (keySpan.StartsWith(argumentSpan))
                                {
                                    return token.Next.Match(
                                        none: () => new Cursor.Item<ParsedArgument>(
                                            new MissingParameter(
                                                new Located<Option>.WholeToken(
                                                    @this,
                                                    token.CurrentToken
                                                )
                                            ),
                                            Optional.Option.None<Cursor>()
                                        ),
                                        some: parameterToken => new Cursor.Item<ParsedArgument>(
                                            new ParsedLongOption.WithParameter(
                                                new Located<Option>.WholeToken(
                                                    @this,
                                                    token.CurrentToken
                                                ),
                                                new Located<string>.WholeToken(
                                                    parameterToken.CurrentToken.Value.Content,
                                                    parameterToken.CurrentToken
                                                )
                                            ),
                                            parameterToken.Next.Upcast()
                                        )
                                    ).Some();
                                }

                                return Optional.Option.None<Cursor.Item<ParsedArgument>>();
                            }

                            if (keySpan.StartsWith(argumentSpan[..delimiterIndex]))
                            {
                                return new Cursor.Item<ParsedArgument>(
                                    new ParsedLongOption.WithParameter(
                                        new Located<Option>.Plain(
                                            @this,
                                            token.CurrentToken,
                                            token.CurrentToken.Segment(..delimiterIndex)
                                        ),
                                        new Located<string>.Plain(
                                            new string(argumentSpan[(delimiterIndex + 1)..]),
                                            token.CurrentToken,
                                            token.CurrentToken.Segment((delimiterIndex + 1)..)
                                        )
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
}