using System;
using System.Collections.Generic;
using System.Linq;
using GraceLine.Cursors;
using GraceLine.Opt;
using GraceLine.Opt.Parsed;
using GraceLine.Text;
using Optional;
using Optional.Collections;
using Optional.Unsafe;
using Option = GraceLine.Opt.Option;

namespace GraceLine
{
    public sealed class Arguments
    {
        private readonly Option[] options;

        public Arguments(params Option[] options)
        {
            this.options = options;
        }

        public IEnumerable<ParsedArgument> Parse(params string[] arguments)
        {
            var cursor = arguments.Length == 0
                ? Optional.Option.None<Cursor>()
                : new TokenStart(arguments).Some<Cursor>();

            while (cursor.HasValue)
            {
                var cursorValue = cursor.ValueOrFailure();
                if (OptionTerminator.Match(cursorValue) is { HasValue: true } terminator)
                {
                    yield return terminator.ValueOrFailure().Value;

                    var restArguments = new OptionSequence<TokenStart>(
                        terminator.ValueOrFailure().Next.Map(static token => (TokenStart)token),
                        token => token.Next
                    );
                    foreach (var argument in restArguments)
                    {
                        yield return new ParsedNonOptionArgument(argument.CurrentToken);
                    }

                    yield break;
                }

                var nonOption = cursorValue.MatchWholeToken().Filter(
                    token => token.CurrentToken.Value.Type is TokenType.Hyphen or TokenType.Plain
                ).Map(
                    token => new Cursor.Item<ParsedArgument>(
                        new ParsedNonOptionArgument(token.CurrentToken),
                        token.Next.Upcast()
                    )
                );

                if (nonOption.HasValue)
                {
                    var item = nonOption.ValueOrFailure();
                    yield return item.Value;
                    cursor = item.Next;
                    continue;
                }

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
                        list.Select(item => item.Value.Option.Value).ToList()
                    );

                    cursor = list.OrderBy(item => item.Next.Map(c => (long)c.Offset).Or(long.MaxValue)).First().Next;
                    continue;
                }

                throw new NotSupportedException();
            }
        }

        private sealed class Visitor : ParsedArgument.Visitor<Option>
        {
            public override Option Visit(ParsedNonOptionArgument argument) => throw new NotSupportedException();
            public override Option Visit(ParsedShortOption argument) => throw new NotSupportedException();
            public override Option Visit(ParsedLongOption argument) => argument.Option.Value;
            public override Option Visit(ParsedNumber argument) => throw new NotSupportedException();
            public override Option Visit(ParsedParametrizedOption argument) => argument.Option;
            public override Option Visit(OptionTerminator argument) => throw new NotSupportedException();
            public override Option Visit(UnrecognizedOption argument) => throw new NotSupportedException();
            public override Option Visit(LongOptionAmbiguity argument) => throw new NotSupportedException();
            public override Option Visit(MissingParameter argument) => argument.Option.Value;
        }

        private sealed class OnlyLongOption : ParsedArgument.Visitor<Option<ParsedLongOption>>
        {
            public static OnlyLongOption Singleton { get; } = new();

            public override Option<ParsedLongOption> Visit(ParsedNonOptionArgument argument) => Optional.Option.None<ParsedLongOption>();
            public override Option<ParsedLongOption> Visit(ParsedShortOption argument) => Optional.Option.None<ParsedLongOption>();
            public override Option<ParsedLongOption> Visit(ParsedLongOption argument) => argument.Some();
            public override Option<ParsedLongOption> Visit(ParsedNumber argument) => Optional.Option.None<ParsedLongOption>();
            public override Option<ParsedLongOption> Visit(ParsedParametrizedOption argument) => Optional.Option.None<ParsedLongOption>();
            public override Option<ParsedLongOption> Visit(OptionTerminator argument) => Optional.Option.None<ParsedLongOption>();
            public override Option<ParsedLongOption> Visit(UnrecognizedOption argument) => Optional.Option.None<ParsedLongOption>();
            public override Option<ParsedLongOption> Visit(LongOptionAmbiguity argument) => Optional.Option.None<ParsedLongOption>();
            public override Option<ParsedLongOption> Visit(MissingParameter argument) => Optional.Option.None<ParsedLongOption>();
        }
    }
}