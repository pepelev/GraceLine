using System;
using CommandLine.Opt.Parsed;
using Optional;

namespace CommandLine.Opt
{
    public sealed class LongOptionallyParametrizedOption : Option
    {
        private readonly string key;

        public LongOptionallyParametrizedOption(string key)
        {
            this.key = key;
        }

        public override Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor)
        {
            const int LongOptionPrefix = 2;
            return cursor.MatchWholeToken()
                .Filter(token => token.CurrentToken.Type == TokenType.DoubleHyphenPrefixed)
                .FlatMap(
                    token =>
                    {
                        var keySpan = key.AsSpan();
                        var argumentSpan = token.CurrentToken.Value.AsSpan(LongOptionPrefix);
                        var delimiterIndex = argumentSpan.IndexOf('=');
                        if (delimiterIndex < 0)
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedParametrizedOption(this, null),
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
                                new ParsedParametrizedOption(this, new string(value)),
                                token.Next.Upcast()
                            ).Some();
                        }

                        return Optional.Option.None<Cursor.Item<ParsedArgument>>();
                    }
                );
        }
    }
}