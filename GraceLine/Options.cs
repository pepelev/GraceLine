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
    public sealed class Options
    {
        private readonly Option[] options;

        public Options(params Option[] options)
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

                if (items.All(item => item.Value.Accept(IsLongOption.Singleton)))
                {
                    var fullMatches = items
                        .Where(item => item.Value.Accept(GetLongOptionMatch.Singleton) == LongOptionMatch.Full)
                        .ToList();
                    if (fullMatches.Count == 0)
                    {
                        yield return new LongOptionAmbiguity(
                            items.Select(item => item.Value.Option.Value).ToList()
                        );

                        cursor = items.OrderBy(item => item.Next.Map(c => (long)c.Offset).Or(long.MaxValue)).First().Next;
                        continue;
                    }

                    if (fullMatches.Count == 1)
                    {
                        yield return fullMatches[0].Value;
                        cursor = fullMatches[0].Next;
                        continue;
                    }
                }

                throw new ArgumentException("Ambiguous options passed. See Data.Options for details")
                {
                    Data =
                    {
                        { "Options", items.Select(item => item.Value) }
                    }
                };
            }
        }

        private sealed class IsLongOption : ParsedOption.Visitor<bool>
        {
            public static IsLongOption Singleton { get; } = new();

            public override bool Visit(ParsedShortOption argument) => false;
            public override bool Visit(ParsedShortOption.WithParameter argument) => false;
            public override bool Visit(ParsedLongOption argument) => true;
            public override bool Visit(ParsedLongOption.WithParameter argument) => true;
            public override bool Visit(ParsedLongOption.WithOptionalParameter argument) => true;
            public override bool Visit(ParsedLongOption.WithMissingArgument option) => true;
            public override bool Visit(ParsedNumber argument) => false;
            public override bool Visit(MissingArgument argument) => false;
        }

        private sealed class GetLongOptionMatch : ParsedOption.Visitor<LongOptionMatch>
        {
            public static GetLongOptionMatch Singleton { get; } = new();

            public override LongOptionMatch Visit(ParsedShortOption argument) => throw new InvalidOperationException();
            public override LongOptionMatch Visit(ParsedShortOption.WithParameter argument) => throw new InvalidOperationException();
            public override LongOptionMatch Visit(ParsedLongOption argument) => argument.Match;
            public override LongOptionMatch Visit(ParsedLongOption.WithParameter argument) => argument.Match;
            public override LongOptionMatch Visit(ParsedLongOption.WithOptionalParameter argument) => LongOptionMatch.Full;
            public override LongOptionMatch Visit(ParsedLongOption.WithMissingArgument option) => option.Match;
            public override LongOptionMatch Visit(ParsedNumber argument) => throw new InvalidOperationException();
            public override LongOptionMatch Visit(MissingArgument argument) => throw new InvalidOperationException();
        }
    }
}