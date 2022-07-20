using System.Collections.Generic;
using CommandLine.Opt;
using CommandLine.Opt.Parsed;

namespace CommandLine.Inputs
{
    public abstract class Input
    {
        public abstract bool Empty { get; }
        public abstract Optional.Option<(ParsedNonOptionArgument Argument, Input Tail)> ParseNonOption();
        public abstract Optional.Option<Input> ParseShortOption(char key);
        public abstract Optional.Option<(string Parameter, Input Tail)> ParseParametrizedShortOption(char key);
        public abstract Optional.Option<(LongOptionMatch Match, Input Tail)> ParseLongOption(string key);
        public abstract Optional.Option<(string Parameter, Input Tail)> ParseParametrizedLongOption(string key);
        public abstract Optional.Option<(ParsedArgument Result, Input Tail)> ParseNumberOption();
        public abstract Optional.Option<IEnumerable<ParsedArgument>> ParseOptionTerminator();
        public abstract (UnrecognizedOption Option, Input Tail) Skip();
    }
}