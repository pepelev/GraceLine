﻿using GraceLine.Text;
using Optional;

namespace GraceLine.Opt.Parsed
{
    public sealed partial class ParsedLongOption
    {
        public sealed class WithOptionalParameter : ParsedOption
        {
            public WithOptionalParameter(
                Located<Option> option,
                Option<Located<string>> argument)
            {
                Option = option;
                Argument = argument;
            }

            public override Located<Option> Option { get; }
            public Option<Located<string>> Argument { get; }
            public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
            public override T Accept<T>(ParsedArgument.Visitor<T> visitor) => visitor.Visit(this);
            public override string ToString() => Argument.Match(
                this,
                none: static @this => @this.Option.Value.ToString(),
                some: static (@this, argument) => $"{@this.Option.Value}={argument.Value}"
            );
        }
    }
}