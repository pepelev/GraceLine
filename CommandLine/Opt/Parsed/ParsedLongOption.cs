﻿namespace CommandLine.Opt.Parsed
{
    public sealed class ParsedLongOption : ParsedArgument
    {
        public ParsedLongOption(Option2 option, LongOptionMatch match)
        {
            Option = option;
            Match = match;
        }

        public Option2 Option { get; }
        public LongOptionMatch Match { get; }

        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}