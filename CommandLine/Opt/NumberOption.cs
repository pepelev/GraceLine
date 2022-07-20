using System.Globalization;
using CommandLine.Cursors;
using CommandLine.Opt.Parsed;
using Optional;

namespace CommandLine.Opt
{
    public sealed class NumberOption : Option
    {
        public static NumberOption Singleton { get; } = new();

        public override Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor)
        {
            // char.IsDigit() is not used because it works not only for 0..9
            static bool IsDigit(char @char) => '0' <= @char && @char <= '9';

            return cursor.MatchShort()
                .Filter(static @short => @short.AtTokenStart)
                .FlatMap(
                    static @short =>
                    {
                        var content = @short.Content;
                        var end = 0;
                        while (end < content.Length && IsDigit(content[end]))
                        {
                            end++;
                        }

                        if (long.TryParse(content[..end], NumberStyles.None, CultureInfo.InvariantCulture, out var value))
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedNumber(
                                    value,
                                    @short.CurrentToken,
                                    @short.CurrentToken.Segment(..end)
                                ),
                                @short.Skip(end)
                            ).Some();
                        }

                        return Optional.Option.None<Cursor.Item<ParsedArgument>>();
                    }
                );
        }
    }
}