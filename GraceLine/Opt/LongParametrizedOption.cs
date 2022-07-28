using System;
using GraceLine.Cursors;
using GraceLine.Opt.Parsed;
using GraceLine.Text;
using Optional;

namespace GraceLine.Opt
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
                .Filter(token => token.CurrentToken.Value.Type == TokenType.DoubleHyphenPrefixed)
                .FlatMap(
                    token =>
                    {
                        var keySpan = key.AsSpan();
                        var argumentSpan = token.CurrentToken.Value.Content.AsSpan(LongOptionPrefix);
                        var delimiterIndex = argumentSpan.IndexOf('=');
                        if (delimiterIndex == -1)
                        {
                            if (keySpan.StartsWith(argumentSpan))
                            {
                                return token.Next.Match(
                                    none: () => new Cursor.Item<ParsedArgument>(
                                        new MissingParameter(this, token.CurrentToken.WholeSegment),
                                        Optional.Option.None<Cursor>()
                                    ),
                                    some: parameterToken =>
                                    {
                                        var parameter = parameterToken.CurrentToken.Value.Content;
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