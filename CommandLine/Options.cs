using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommandLine.Opt;
using CommandLine.Opt.Parsed;
using Optional;
using Optional.Collections;
using Optional.Unsafe;

namespace CommandLine
{
    public abstract class Cursor
    {
        public Item<UnrecognizedOption> Skip() => MatchShortOption().Map(
            option => option.Key.Map(key => new UnrecognizedOption(key.ToString()))
        ).ValueOr(
            () => MatchEntireToken().Map(
                item => item.Map(token => new UnrecognizedOption(token.Value))
            ).ValueOrFailure()
        );

        public abstract int Offset { get; }
        public abstract Token CurrentToken { get; }
        public abstract Optional.Option<Cursor> Next { get; }


        public readonly struct Item<T>
        {
            public Item(T value, Optional.Option<Cursor> next)
            {
                Value = value;
                Next = next;
            }

            public T Value { get; }
            public Optional.Option<Cursor> Next { get; }
            public Item<K> Map<K>(Func<T, K> map) => new(map(Value), Next);
        }

        public abstract Optional.Option<TokenStart> MatchWholeToken();
        public abstract Optional.Option<TokenMiddle> MatchShort();

        public abstract Optional.Option<Item<Token>> MatchEntireToken();
        public abstract Optional.Option<ShortOption2> MatchShortOption();
    }

    public sealed class TokenStart : Cursor
    {
        private readonly ImmutableQueue<Token> tokens;

        public TokenStart(IEnumerable<string> tokens)
            : this(
                ImmutableQueue.CreateRange(
                    tokens.Select(
                        token => new Token(token)
                    )
                ),
                0
            )
        {
        }

        private TokenStart(ImmutableQueue<Token> tokens, int offset)
        {
            this.tokens = tokens;
            Offset = offset;
        }

        public override int Offset { get; }
        public override Optional.Option<TokenStart> MatchWholeToken() => this.Some();

        public override Optional.Option<TokenMiddle> MatchShort() => CurrentToken
            .SomeWhen(token => token.Type == TokenType.HyphenPrefixed)
            .Map(token => new TokenMiddle(token, 1, this));

        public override Optional.Option<Item<Token>> MatchEntireToken() => new Item<Token>(tokens.Peek(), Next).Some();

        public override Optional.Option<ShortOption2> MatchShortOption() => CurrentToken.Some()
            .Filter(token => token.Type == TokenType.HyphenPrefixed)
            .FlatMap(token => new TokenMiddle(token, 1, this).MatchShortOption());

        public override Optional.Option<Cursor> Next => tokens.Dequeue() is { IsEmpty: false } rest
            ? new TokenStart(rest, Offset + CurrentToken.Length).Some<Cursor>()
            : Option.None<Cursor>();

        public Optional.Option<TokenStart> NextA => tokens.Dequeue() is { IsEmpty: false } rest
            ? new TokenStart(rest, Offset + CurrentToken.Length).Some<TokenStart>()
            : Option.None<TokenStart>();

        public override Token CurrentToken => tokens.Peek();
    }

    public sealed class TokenMiddle : Cursor
    {
        public TokenMiddle(Token token, int index, TokenStart start)
        {
            this.token = token;
            this.index = index;
            this.start = start;
        }

        private readonly Token token;
        private readonly int index;
        private readonly TokenStart start;

        public override int Offset => start.Offset + index;
        public override Token CurrentToken => start.CurrentToken;
        public override Optional.Option<TokenStart> MatchWholeToken() => Option.None<TokenStart>();
        public override Optional.Option<TokenMiddle> MatchShort() => this.Some();
        public override Optional.Option<Item<Token>> MatchEntireToken() => Option.None<Item<Token>>();

        public override Optional.Option<ShortOption2> MatchShortOption() => new ShortOption2(
            new Item<char>(
                token.Value[index],
                Next
            ),
            new Item<TokenTail>(
                new TokenTail(token.Value[(index + 1)..]),
                start.Next
            ),
            new Item<TokenTail>(
                new TokenTail(token.Value[index..]),
                Next
            )
        ).Some();

        public Optional.Option<Cursor> Skip(int chars)
        {
            if (chars == 0)
            {
                return this.Some<Cursor>();
            }

            if (chars < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chars), "must not be negative");
            }

            var newIndex = index + chars;
            if (newIndex == token.Length)
            {
                return start.Next;
            }

            if (newIndex < token.Length)
            {
                return new TokenMiddle(token, newIndex, start).Some<Cursor>();
            }

            throw new ArgumentOutOfRangeException(nameof(chars), "must not exceed chars left in current token");
        }

        public ReadOnlySpan<char> Content => token.Value.AsSpan(index);
        public bool AtTokenStart => index == 1;

        public override Optional.Option<Cursor> Next => index == token.Length - 1
            ? start.Next
            : new TokenMiddle(token, index + 1, start).Some<Cursor>();

        // todo skip
        // С одной стороны хочется унести знание о структуре в конкретные опции, но здесь нужно знать что пропускать короткую опцию или целый токен
    }

    //internal sealed class End : Cursor
    //{
    //    public static End Singleton { get; } = new();
    //    public override bool AtEnd => true;
    //    public override int Offset => int.MaxValue;
    //    public override Optional.Option<Item<Token>> MatchEntireToken() => Option.None<Item<Token>>();
    //    public override Optional.Option<ShortOption> MatchShortOption() => Option.None<ShortOption>();
    //}

    public readonly struct Token
    {
        public Token(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public TokenType Type
        {
            get
            {
                if (Value == "-")
                    return TokenType.Hyphen;

                if (Value == "--")
                    return TokenType.DoubleHyphen;

                if (Value.StartsWith("--"))
                    return TokenType.DoubleHyphenPrefixed;

                if (Value.StartsWith("-"))
                    return TokenType.HyphenPrefixed;

                return TokenType.Plain;
            }
        }
        public int Length => Value.Length;
    }

    public enum TokenType
    {
        Plain,
        Hyphen,
        DoubleHyphen,
        HyphenPrefixed,
        DoubleHyphenPrefixed
    }

    public readonly struct ShortOption2
    {
        public ShortOption2(Cursor.Item<char> key, Cursor.Item<TokenTail> tail, Cursor.Item<TokenTail> whole)
        {
            Key = key;
            Tail = tail;
            Whole = whole;
        }

        public Cursor.Item<char> Key { get; }
        public Cursor.Item<TokenTail> Tail { get; }
        public Cursor.Item<TokenTail> Whole { get; }
    }

    public readonly struct TokenTail
    {
        public TokenTail(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    internal sealed class OptionSequence<T> : IEnumerable<T>
    {
        private readonly Optional.Option<T> start;
        private readonly Func<T, Optional.Option<T>> next;

        public OptionSequence(Optional.Option<T> start, Func<T, Optional.Option<T>> next)
        {
            this.start = start;
            this.next = next;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var current = start;
            while (current.HasValue)
            {
                var value = current.ValueOrFailure();
                yield return value;

                current = next(value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

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