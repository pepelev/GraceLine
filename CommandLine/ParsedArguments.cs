﻿using System.Collections.Generic;

namespace CommandLine
{
    public sealed class ParsedArguments
    {
        public IEnumerable<string> NonOptions { get; }
        public OptionArguments Options { get; }
    }

    public sealed class OptionArguments
    {
        //bool Has(CommandLine.Option option) => false;
    }
}