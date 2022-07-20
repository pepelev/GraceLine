﻿namespace CommandLine.Opt.Parsed
{
    public sealed class MissingParameter : ParsedArgument
    {
        public MissingParameter(Option2 option)
        {
            Option = option;
        }

        public Option2 Option { get; }

        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}