using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommandLine.Opt;
using CommandLine.Opt.Parsed;
using Optional;

namespace CommandLine.Inputs
{
    internal sealed class ArgumentInput : Input
    {
        private readonly ImmutableQueue<string> arguments;

        public ArgumentInput(ImmutableQueue<string> arguments)
        {
            this.arguments = arguments;
        }

        public ArgumentInput(IEnumerable<string> arguments)
            : this(ImmutableQueue.CreateRange(arguments))
        {
        }

        public override bool Empty => arguments.IsEmpty;

        public override Optional.Option<(ParsedNonOptionArgument Argument, Input Tail)> ParseNonOption()
        {
            var current = arguments.Peek();
            if (current == "-" || !current.StartsWith("-"))
            {
                Input tail = new ArgumentInput(arguments.Dequeue());
                var argument = new ParsedNonOptionArgument(current);
                return Option.Some((argument, tail));
            }

            return Option.None<(ParsedNonOptionArgument Argument, Input Tail)>();
        }

        public override Optional.Option<Input> ParseShortOption(char key)
        {
            if (arguments.IsEmpty)
            {
                return Option.None<Input>();
            }

            var current = arguments.Peek();
            if (current == "-")
            {
                return Option.None<Input>();
            }

            if (current.StartsWith("-", StringComparison.Ordinal) && !current.StartsWith("--", StringComparison.Ordinal))
            {
                var input = new ShortArgumentInput(current, 1, arguments.Dequeue());
                return input.ParseShortOption(key);
            }

            return Option.None<Input>();
        }

        public override Optional.Option<(string Parameter, Input Tail)> ParseParametrizedShortOption(char key)
        {
            if (arguments.IsEmpty)
            {
                return Option.None<(string Parameter, Input Tail)>();
            }

            var current = arguments.Peek();
            if (current == "-")
            {
                return Option.None<(string Parameter, Input Tail)>();
            }

            if (current.StartsWith("-", StringComparison.Ordinal) && !current.StartsWith("--", StringComparison.Ordinal))
            {
                var input = new ShortArgumentInput(current, 1, arguments.Dequeue());
                return input.ParseParametrizedShortOption(key);
            }

            return Option.None<(string Parameter, Input Tail)>();
        }

        private const int LongOptionPrefix = 2;

        public override Optional.Option<(LongOptionMatch Match, Input Tail)> ParseLongOption(string key)
        {
            var current = arguments.Peek();

            // todo extract
            if (!current.StartsWith("--"))
            {
                return Option.None<(LongOptionMatch Match, Input Tail)>();
            }

            if (current.Length <= key.Length + LongOptionPrefix)
            {
                var keySpan = key.AsSpan();
                var argumentSpan = current.AsSpan(LongOptionPrefix);
                if (keySpan.SequenceEqual(argumentSpan))
                {
                    return Option.Some(
                        (LongOptionMatch.Full, new ArgumentInput(arguments.Dequeue()) as Input)
                    );
                }

                if (keySpan.StartsWith(argumentSpan))
                {
                    return Option.Some(
                        (LongOptionMatch.Prefix, new ArgumentInput(arguments.Dequeue()) as Input)
                    );
                }
            }

            return Option.None<(LongOptionMatch Match, Input Tail)>();
        }

        public override Optional.Option<(string Parameter, Input Tail)> ParseParametrizedLongOption(string key)
        {
            var current = arguments.Peek();
            if (!current.StartsWith("--"))
            {
                return Option.None<(string Parameter, Input Tail)>();
            }

            var keySpan = key.AsSpan();
            var argumentSpan = current.AsSpan(LongOptionPrefix);
            var delimiterIndex = argumentSpan.IndexOf('=');
            if (delimiterIndex == -1)
            {
                if (keySpan.StartsWith(argumentSpan))
                {
                    var queue = arguments.Dequeue();
                    if (queue.IsEmpty)
                    {
                        return Option.None<(string Parameter, Input Tail)>();
                    }

                    var peek = queue.Peek();
                    return Option.Some((peek, new ArgumentInput(queue.Dequeue()) as Input));
                }

                return Option.None<(string Parameter, Input Tail)>();
            }

            return delimiterIndex switch
            {
                var index and >= 0 when keySpan.StartsWith(argumentSpan[..index]) =>
                    Option.Some(
                        (new string(argumentSpan[(index + 1)..]), new ArgumentInput(arguments.Dequeue()) as Input)
                    ),
                _ => Option.None<(string Parameter, Input Tail)>()
            };
        }

        public override Optional.Option<(ParsedArgument Result, Input Tail)> ParseNumberOption()
        {
            var current = arguments.Peek();
            if (current != "-" && current.StartsWith("-", StringComparison.Ordinal) && !current.StartsWith("--", StringComparison.Ordinal))
            {
                var input = new ShortArgumentInput(current, 1, arguments.Dequeue());
                return input.ParseNumberOption();
            }

            return Option.None<(ParsedArgument Result, Input Tail)>();
        }

        public override Optional.Option<IEnumerable<ParsedArgument>> ParseOptionTerminator()
        {
            if (arguments.IsEmpty)
            {
                return Option.None<IEnumerable<ParsedArgument>>();
            }

            var current = arguments.Peek();
            if (current == "--")
            {
                var parsedArguments = arguments
                    .Skip(1)
                    .Select(nonOption => new ParsedNonOptionArgument(nonOption) as ParsedArgument)
                    .Prepend(OptionTerminator.Singleton);
                return Option.Some(parsedArguments);
            }

            return Option.None<IEnumerable<ParsedArgument>>();
        }

        public override (UnrecognizedOption Option, Input Tail) Skip()
        {
            var current = arguments.Peek();
            if (current != "-" && current.StartsWith("-", StringComparison.Ordinal) && !current.StartsWith("--", StringComparison.Ordinal))
            {
                var input = new ShortArgumentInput(current, 1, arguments.Dequeue());
                return input.Skip();
            }

            return (new UnrecognizedOption(current), new ArgumentInput(arguments.Dequeue()));
        }
    }
}