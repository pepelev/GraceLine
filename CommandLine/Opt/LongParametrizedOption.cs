﻿using System;
using CommandLine.Opt.Parsed;
using Optional;

namespace CommandLine.Opt
{
    public sealed class LongOptionallyParametrizedOption : Option2
    {
        public LongOptionallyParametrizedOption(string key)
        {
            this.key = key;
        }

        private readonly string key;

        public override Optional.Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor)
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
                                token.Next
                            ).Some();
                        }

                        if (delimiterIndex == 0)
                        {
                            return Option.None<Cursor.Item<ParsedArgument>>();
                        }

                        if (keySpan.StartsWith(argumentSpan[..delimiterIndex]))
                        {
                            var value = argumentSpan[(delimiterIndex + 1)..];
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedParametrizedOption(this, new string(value)),
                                token.Next
                            ).Some();
                        }

                        return Option.None<Cursor.Item<ParsedArgument>>();
                    }
                );
        }
    }

    public sealed class LongParametrizedOption : Option2
    {
        private readonly string key;

        public LongParametrizedOption(string key)
        {
            this.key = key;
        }

        public override Optional.Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor)
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
                                return token.NextA.Match(
                                    none: () => new Cursor.Item<ParsedArgument>(
                                        new MissingParameter(this),
                                        Option.None<Cursor>()
                                    ),
                                    some: parameterToken =>
                                    {
                                        var parameter = parameterToken.CurrentToken.Value;
                                        return new Cursor.Item<ParsedArgument>(
                                            new ParsedParametrizedOption(this, parameter),
                                            parameterToken.Next
                                        );
                                    }
                                ).Some();
                            }

                            return Option.None<Cursor.Item<ParsedArgument>>();
                        }

                        if (keySpan.StartsWith(argumentSpan[..delimiterIndex]))
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedParametrizedOption(
                                    this,
                                    new string(argumentSpan[(delimiterIndex + 1)..])
                                ),
                                token.Next
                            ).Some();
                        }

                        return Option.None<Cursor.Item<ParsedArgument>>();
                    }
                );
        }
    }
}