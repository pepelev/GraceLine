using System.Globalization;
using GraceLine.Cursors;
using GraceLine.Opt.Parsed;
using Optional;

namespace GraceLine.Opt
{
    public sealed class NumberOption : Option
    {
        public static NumberOption Singleton { get; } = new();

        public override string ToString() => "-{Digits}";

        public override Option<Cursor.Item<ParsedOption>> Match(Cursor cursor)
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
                            return new Cursor.Item<ParsedOption>(
                                new ParsedNumber(
                                    value,
                                    @short.CurrentToken,
                                    @short.CurrentToken.Segment(..end)
                                ),
                                @short.Feed(end)
                            ).Some();
                        }

                        return Optional.Option.None<Cursor.Item<ParsedOption>>();
                    }
                );
        }
    }
}