using System;
using System.Globalization;
using CommandLine.Inputs;
using CommandLine.Opt.Parsed;
using Optional;

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

    public sealed class NumberOption : Option2
    {
        public static NumberOption Singleton { get; } = new();

        public override Optional.Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor)
        {
            // todo inspect char.IsDigit();
            static bool IsDigit(char @char) => '0' <= @char && @char <= '9';

            return cursor.MatchShort()
                .Filter(@short => @short.AtTokenStart)
                .FlatMap(
                @short =>
                {
                    var content = @short.Content;
                    var end = 0;
                    while (end < content.Length && IsDigit(content[end]))
                        end++;

                    if (long.TryParse(content[..end], NumberStyles.None, CultureInfo.InvariantCulture, out var value))
                    {
                        return new Cursor.Item<ParsedArgument>(
                            new ParsedNumber(value),
                            @short.Skip(end)
                        ).Some();
                    }

                    return Option.None<Cursor.Item<ParsedArgument>>();
                }
            );
        }
    }
}