﻿using CommandLine.Opt.Parsed;

namespace CommandLine.Opt
{
    public sealed class ShortOption : Option2
    {
        private readonly char key;

        public ShortOption(char key)
        {
            this.key = key;
        }

        public override string ToString() => $"-{key}";

        public override Optional.Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor) => cursor.MatchShort()
            .Filter(option => option.Content[0] == key)
            .Map(option => new Cursor.Item<ParsedArgument>(
                    new ParsedShortOption(this),
                    option.Skip(1)
                )
            );
    }
}