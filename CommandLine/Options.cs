using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommandLine.Inputs;
using CommandLine.Opt;
using CommandLine.Opt.Parsed;
using Optional;
using Optional.Collections;
using Optional.Unsafe;

namespace CommandLine
{
    public abstract class Cursor
    {
        public abstract bool AtEnd { get; }
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
        }

        public abstract Optional.Option<Item<Token>> MatchEntireToken();
        public abstract Optional.Option<ShortOption2> MatchShortOption();
    }

    internal sealed class TokenStart : Cursor
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

        public override bool AtEnd => false;
        public override int Offset { get; }

        public override Optional.Option<Item<Token>> MatchEntireToken() => new Item<Token>(tokens.Peek(), Next).Some();
        public override Optional.Option<ShortOption2> MatchShortOption() => Option.None<ShortOption2>();

        public override Optional.Option<Cursor> Next => tokens.Dequeue() is { IsEmpty: false } rest
            ? new TokenStart(rest, Offset + CurrentToken.Length).Some<Cursor>()
            : Option.None<Cursor>();

        public override Token CurrentToken => tokens.Peek();
    }

    internal sealed class TokenMiddle : Cursor
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

        public override bool AtEnd => false;
        public override int Offset => start.Offset + index;
        public override Token CurrentToken => start.CurrentToken;
        public override Optional.Option<Item<Token>> MatchEntireToken() => Option.None<Item<Token>>();

        public override Optional.Option<ShortOption2> MatchShortOption() => new ShortOption2(
            new Item<char>(
                token.Value[index],
                Next
            ),
            new Item<TokenTail>(
                new TokenTail(token.Value[(index + 1)..]),
                start.Next
            )
        ).Some();

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
        public ShortOption2(Cursor.Item<char> key, Cursor.Item<TokenTail> rest)
        {
            Key = key;
            Rest = rest;
        }

        public Cursor.Item<char> Key { get; }
        public Cursor.Item<TokenTail> Rest { get; }
    }

    public readonly struct TokenTail
    {
        public TokenTail(string value)
        {
            Value = value;
        }

        public string Value { get; }
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
            var input = new TokenStart(arguments).Some<Cursor>();
            while (input.HasValue)
            {
                var inputValue = input.ValueOrFailure();
                var currentToken = inputValue.CurrentToken;
                if (currentToken.Type == TokenType.DoubleHyphen)
                {
                    yield return OptionTerminator.Singleton;
                    input = inputValue.Next;
                }
                else if (currentToken.Type is TokenType.Hyphen or TokenType.Plain)
                {
                    yield return new ParsedNonOptionArgument(currentToken.Value);
                    input = inputValue.Next;
                }
                else if (currentToken.Type == TokenType.DoubleHyphenPrefixed)
                {
                    var items = options
                        .Select(option => option.Match(inputValue))
                        .Values()
                        .ToList();

                    if (items.Count == 0)
                    {
                        yield return new UnrecognizedOption(currentToken.Value);
                        input = inputValue.Next;
                        continue;
                    }

                    if (items.Count == 1)
                    {
                        yield return items[0].Value;
                        input = items[0].Next;
                        continue;
                    }

                    throw new Exception("abc");
                }
                else
                {
                    var items = options
                        .Select(option => option.Match(inputValue))
                        .Values()
                        .ToList();

                    throw new Exception("abc");
                }
            }
        }

        private sealed class Visitor : ParsedArgument.Visitor<Opt.Option<ParsedArgument>>
        {
            public override Opt.Option<ParsedArgument> Visit(ParsedNonOptionArgument argument) => throw new NotSupportedException();
            public override Opt.Option<ParsedArgument> Visit(ParsedShortOption argument) => throw new NotSupportedException();
            public override Opt.Option<ParsedArgument> Visit(ParsedLongOption argument) => argument.Option;
            public override Opt.Option<ParsedArgument> Visit(ParsedNumberOption argument) => throw new NotSupportedException();
            public override Opt.Option<ParsedArgument> Visit(ParsedParametrizedOption argument) => argument.Option;
            public override Opt.Option<ParsedArgument> Visit(OptionTerminator argument) => throw new NotSupportedException();
            public override Opt.Option<ParsedArgument> Visit(UnrecognizedOption argument) => throw new NotSupportedException();
            public override Opt.Option<ParsedArgument> Visit(LongOptionAmbiguity argument) => throw new NotSupportedException();
        }
    }
    
    public sealed class Options
    {
        private readonly Opt.Option<ParsedArgument>[] options;

        public Options(params Opt.Option<ParsedArgument>[] options)
        {
            this.options = options;
        }

        public IEnumerable<ParsedArgument> Parse(params string[] arguments)
        {
            Input input = new ArgumentInput(arguments);
            while (!input.Empty)
            {
                var nonOption = input.ParseNonOption();
                if (nonOption.HasValue)
                {
                    var (argument, tail) = nonOption.ValueOrFailure();
                    yield return argument;
                    input = tail;
                    continue;
                }

                var optionTerminator = input.ParseOptionTerminator();
                if (optionTerminator.HasValue)
                {
                    foreach (var argument in optionTerminator.ValueOrFailure())
                    {
                        yield return argument;
                    }

                    yield break;
                }

                var pairs = options
                    .Select(option => option.Match(input))
                    .Values()
                    .ToList();

                if (pairs.Count == 0)
                {
                    var (option, tail) = input.Skip();
                    yield return option;
                    input = tail;
                    continue;
                }

                if (pairs.Count == 1)
                {
                    yield return pairs[0].Result;
                    input = pairs[0].Tail;
                    continue;
                }

                {
                    if (pairs.All(pair => pair.Result is ParsedLongOption { Match: LongOptionMatch.Prefix } or ParsedParametrizedOption))
                    {
                        yield return new LongOptionAmbiguity(
                            pairs.Select(pair => pair.Result.Accept(new Visitor())).ToList()
                        );
                        // todo test and fix
                        input = pairs.First().Tail;
                        continue;
                    }

                    var (result, tail) = pairs.Single(pair => ((ParsedLongOption)pair.Result).Match == LongOptionMatch.Full);
                    yield return result;
                    input = tail;
                    continue;

                    throw new Exception("abc");
                }
            }
        }

        private sealed class Visitor : ParsedArgument.Visitor<Opt.Option<ParsedArgument>>
        {
            public override Opt.Option<ParsedArgument> Visit(ParsedNonOptionArgument argument) => throw new NotSupportedException();
            public override Opt.Option<ParsedArgument> Visit(ParsedShortOption argument) => throw new NotSupportedException();
            public override Opt.Option<ParsedArgument> Visit(ParsedLongOption argument) => argument.Option;
            public override Opt.Option<ParsedArgument> Visit(ParsedNumberOption argument) => throw new NotSupportedException();
            public override Opt.Option<ParsedArgument> Visit(ParsedParametrizedOption argument) => argument.Option;
            public override Opt.Option<ParsedArgument> Visit(OptionTerminator argument) => throw new NotSupportedException();
            public override Opt.Option<ParsedArgument> Visit(UnrecognizedOption argument) => throw new NotSupportedException();
            public override Opt.Option<ParsedArgument> Visit(LongOptionAmbiguity argument) => throw new NotSupportedException();
        }
    }
}