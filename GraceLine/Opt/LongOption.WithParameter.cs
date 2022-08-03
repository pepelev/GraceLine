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
                            if (delimiterIndex == -1)
                            {
                                if (argumentSpan.TryMatch(keySpan) is { } match)
                                {
                                    return token.Next.Match(
                                        none: () => new Cursor.Item<ParsedOption>(
                                            new ParsedLongOption.WithMissingArgument(
                                                new Located<Option>.WholeToken(
                                                    @this,
                                                    token.CurrentToken
                                                ),
                                                match
                                            ),
                                            Optional.Option.None<Cursor>()
                                        ),
                                        some: parameterToken => new Cursor.Item<ParsedOption>(
                                            new ParsedLongOption.WithParameter(
                                                new Located<Option>.WholeToken(
                                                    @this,
                                                    token.CurrentToken
                                                ),
                                                match,
                                                new Located<string>.WholeToken(
                                                    parameterToken.CurrentToken.Value.Content,
                                                    parameterToken.CurrentToken
                                                )
                                            ),
                                            parameterToken.Next.Upcast()
                                        )
                                    ).Some();
                                }

                                return Optional.Option.None<Cursor.Item<ParsedOption>>();
                            }
                            else
                            {
                                if (argumentSpan[..delimiterIndex].TryMatch(keySpan) is { } match)
                                {
                                    return new Cursor.Item<ParsedOption>(
                                        new ParsedLongOption.WithParameter(
                                            new Located<Option>.Plain(
                                                @this,
                                                token.CurrentToken,
                                                token.CurrentToken.Segment(..delimiterIndex)
                                            ),
                                            match,
                                            new Located<string>.Plain(
                                                new string(argumentSpan[(delimiterIndex + 1)..]),
                                                token.CurrentToken,
                                                token.CurrentToken.Segment((delimiterIndex + 1)..)
                                            )
                                        ),
                                        token.Next.Upcast()
                                    ).Some();
                                }
                            }

                            return Optional.Option.None<Cursor.Item<ParsedOption>>();
                        }
                    );
            }
        }
    }
}