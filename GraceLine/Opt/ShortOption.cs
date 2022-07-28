using GraceLine.Cursors;
using GraceLine.Opt.Parsed;
using Optional;

namespace GraceLine.Opt
{
    public sealed class ShortOption : Option
    {
        private readonly char key;

        public ShortOption(char key)
        {
            this.key = key;
        }

        public override string ToString() => $"-{key}";

        public override Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor) => cursor.MatchShort()
            .Filter(this, static (@this, cursor) => cursor.Content[0] == @this.key)
            .Map(
                this,
                static (@this, cursor) => new Cursor.Item<ParsedArgument>(
                    new ParsedShortOption(@this, cursor.Segment(1)),
                    cursor.Feed(1)
                )
            );
    }
}