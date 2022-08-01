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
                            if (delimiterIndex < 0)
                            {
                                return new Cursor.Item<ParsedArgument>(
                                    new ParsedParametrizedOption(
                                        @this,
                                        null
                                    ),
                                    token.Next.Upcast()
                                ).Some();
                            }

                            if (delimiterIndex == 0)
                            {
                                return Optional.Option.None<Cursor.Item<ParsedArgument>>();
                            }

                            if (keySpan.StartsWith(argumentSpan[..delimiterIndex]))
                            {
                                var value = argumentSpan[(delimiterIndex + 1)..];
                                return new Cursor.Item<ParsedArgument>(
                                    new ParsedParametrizedOption(@this, new string(value)),
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