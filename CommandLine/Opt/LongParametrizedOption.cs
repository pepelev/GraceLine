using System;
using CommandLine.Cursors;
using CommandLine.Opt.Parsed;
using Optional;

namespace CommandLine.Opt
{
    public sealed class LongParametrizedOption : Option
    {
        private readonly string key;

        public LongParametrizedOption(string key)
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
                        if (delimiterIndex == -1)
                        {
                            if (keySpan.StartsWith(argumentSpan))
                            {
                                return token.Next.Match(
                                    none: () => new Cursor.Item<ParsedArgument>(
                                        new MissingParameter(this),
                                        Optional.Option.None<Cursor>()
                                    ),
                                    some: parameterToken =>
                                    {
                                        var parameter = parameterToken.CurrentToken.Value;
                                        return new Cursor.Item<ParsedArgument>(
                                            new ParsedParametrizedOption(this, parameter),
                                            parameterToken.Next.Upcast()
                                        );
                                    }
                                ).Some();
                            }

                            return Optional.Option.None<Cursor.Item<ParsedArgument>>();
                        }

                        if (keySpan.StartsWith(argumentSpan[..delimiterIndex]))
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedParametrizedOption(
                                    this,
                                    new string(argumentSpan[(delimiterIndex + 1)..])
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