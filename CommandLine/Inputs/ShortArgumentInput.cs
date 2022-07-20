using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using CommandLine.Opt;
using CommandLine.Opt.Parsed;
using Optional;

namespace CommandLine.Inputs
{
    internal sealed class ShortArgumentInput : Input
    {
        private readonly string current;
        private readonly int index;
        private readonly ImmutableQueue<string> tail;

        public ShortArgumentInput(string current, int index, ImmutableQueue<string> tail)
        {
            this.current = current;
            this.index = index;
            this.tail = tail;
        }

        public override bool Empty => false;

        public override Optional.Option<(ParsedNonOptionArgument Argument, Input Tail)> ParseNonOption() =>
            Option.None<(ParsedNonOptionArgument Argument, Input Tail)>();

        public override Optional.Option<Input> ParseShortOption(char key)
        {
            if (current[index] == key)
            {
                return Option.Some<Input>(
                    index == current.Length - 1
                        ? new ArgumentInput(tail)
                        : new ShortArgumentInput(current, index + 1, tail)
                );
            }

            return Option.None<Input>();
        }

        public override Optional.Option<(string Parameter, Input Tail)> ParseParametrizedShortOption(char key)
        {
            if (current[index] == key && index + 1 < current.Length)
            {
                return Option.Some(
                    (current.Substring(index + 1), new ArgumentInput(tail) as Input)
                );
            }

            if (current[index] == key && !tail.IsEmpty)
            {
                var parameter = tail.Peek();
                return Option.Some(
                    (parameter, new ArgumentInput(tail.Dequeue()) as Input)
                );
            }

            return Option.None<(string Parameter, Input Tail)>();
        }

        public override Optional.Option<(LongOptionMatch Match, Input Tail)> ParseLongOption(string key) =>
            Option.None<(LongOptionMatch Match, Input Tail)>();

        public override Optional.Option<(string Parameter, Input Tail)> ParseParametrizedLongOption(string key) =>
            Option.None<(string Parameter, Input Tail)>();

        public override Optional.Option<(ParsedArgument Result, Input Tail)> ParseNumberOption()
        {
            // todo inspect char.IsDigit();
            bool IsDigit(char @char) => '0' <= @char && @char <= '9';

            if (index == 1)
            {
                var end = 1;
                while (end < current.Length && IsDigit(current[end]))
                    end++;

                // todo check overflow
                if (long.TryParse(current.AsSpan()[1..end], NumberStyles.None, CultureInfo.InvariantCulture, out var value))
                {
                    ParsedArgument option = new ParsedNumber(value);
                    Input next = end == current.Length
                        ? new ArgumentInput(tail)
                        : new ShortArgumentInput(current, end, tail);
                    return Option.Some((option, next));
                }
            }

            return Option.None<(ParsedArgument Result, Input Tail)>();
        }

        public override Optional.Option<IEnumerable<ParsedArgument>> ParseOptionTerminator() =>
            Option.None<IEnumerable<ParsedArgument>>();

        public override (UnrecognizedOption Option, Input Tail) Skip()
        {
            var option = new UnrecognizedOption(
                index == 1
                    ? current[..2]
                    : current[index].ToString()
            );
            // todo surrogate pairs
            Input next = index == current.Length - 1
                ? new ArgumentInput(tail)
                : new ShortArgumentInput(current, index + 1, tail);
            return (option, next);
        }
    }
}