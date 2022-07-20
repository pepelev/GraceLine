using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine.Opt;
using CommandLine.Opt.Parsed;
using Optional;
using Optional.Collections;
using Optional.Unsafe;

namespace CommandLine
{
    public sealed class Options2
    {
        private readonly Option2[] options;

        public Options2(params Option2[] options)
        {
            this.options = options;
        }

        public IEnumerable<ParsedArgument> Parse(params string[] arguments)
        {
            var cursor = arguments.Length == 0
                ? Option.None<Cursor>()
                : new TokenStart(arguments).Some<Cursor>();

            while (cursor.HasValue)
            {
                var cursorValue = cursor.ValueOrFailure();
                if (OptionTerminator.Match(cursorValue) is { HasValue: true } tail)
                {
                    yield return OptionTerminator.Singleton;

                    var restArguments = new OptionSequence<TokenStart>(tail.ValueOrFailure(), token => token.NextA);
                    foreach (var argument in restArguments)
                    {
                        yield return new ParsedNonOptionArgument(argument.CurrentToken.Value);
                    }

                    yield break;
                }

                var currentToken = cursorValue.CurrentToken;
                if (currentToken.Type is TokenType.Hyphen or TokenType.Plain)
                {
                    yield return new ParsedNonOptionArgument(currentToken.Value);
                    cursor = cursorValue.Next;
                }
                else
                {
                    var items = options
                        .Select(option => option.Match(cursorValue))
                        .Values()
                        .ToList();

                    if (items.Count == 0)
                    {
                        var item = cursorValue.Skip();
                        yield return item.Value;
                        cursor = item.Next;
                        continue;
                    }

                    if (items.Count == 1)
                    {
                        yield return items[0].Value;
                        cursor = items[0].Next;
                        continue;
                    }

                    var list = items
                        .Select(
                            item => item.Value.Accept(OnlyLongOption.Singleton).Map(
                                option => new Cursor.Item<ParsedLongOption>(
                                    option,
                                    item.Next
                                )
                            )
                        )
                        .Values()
                        .ToList();

                    if (list.Count == items.Count)
                    {
                        if (list.Count(item => item.Value.Match == LongOptionMatch.Full) == 1)
                        {
                            var item = list.Single(item => item.Value.Match == LongOptionMatch.Full);
                            yield return item.Value;
                            cursor = item.Next;
                            continue;
                        }

                        yield return new LongOptionAmbiguity(
                            list.Select(item => item.Value.Option).ToList()
                        );

                        cursor = list.OrderBy(item => item.Next.Map(c => (long)c.Offset).Or(long.MaxValue)).First().Next;
                        continue;
                    }

                    throw new NotSupportedException();
                }
            }
        }

        private sealed class Visitor : ParsedArgument.Visitor<Opt.Option2>
        {
            public override Option2 Visit(ParsedNonOptionArgument argument) => throw new NotSupportedException();
            public override Option2 Visit(ParsedShortOption argument) => throw new NotSupportedException();
            public override Option2 Visit(ParsedLongOption argument) => argument.Option;
            public override Option2 Visit(ParsedNumber argument) => throw new NotSupportedException();
            public override Option2 Visit(ParsedParametrizedOption argument) => argument.Option;
            public override Option2 Visit(OptionTerminator argument) => throw new NotSupportedException();
            public override Option2 Visit(UnrecognizedOption argument) => throw new NotSupportedException();
            public override Option2 Visit(LongOptionAmbiguity argument) => throw new NotSupportedException();
            public override Option2 Visit(MissingParameter argument) => argument.Option;
        }

        private sealed class OnlyLongOption : ParsedArgument.Visitor<Optional.Option<ParsedLongOption>>
        {
            public static OnlyLongOption Singleton { get; } = new OnlyLongOption();

            public override Optional.Option<ParsedLongOption> Visit(ParsedNonOptionArgument argument) => Option.None<ParsedLongOption>();
            public override Optional.Option<ParsedLongOption> Visit(ParsedShortOption argument) => Option.None<ParsedLongOption>();
            public override Optional.Option<ParsedLongOption> Visit(ParsedLongOption argument) => argument.Some();
            public override Optional.Option<ParsedLongOption> Visit(ParsedNumber argument) => Option.None<ParsedLongOption>();
            public override Optional.Option<ParsedLongOption> Visit(ParsedParametrizedOption argument) => Option.None<ParsedLongOption>();
            public override Optional.Option<ParsedLongOption> Visit(OptionTerminator argument) => Option.None<ParsedLongOption>();
            public override Optional.Option<ParsedLongOption> Visit(UnrecognizedOption argument) => Option.None<ParsedLongOption>();
            public override Optional.Option<ParsedLongOption> Visit(LongOptionAmbiguity argument) => Option.None<ParsedLongOption>();
            public override Optional.Option<ParsedLongOption> Visit(MissingParameter argument) => Option.None<ParsedLongOption>();
        }
    }
}