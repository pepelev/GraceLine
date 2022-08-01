using GraceLine.Cursors;
using GraceLine.Opt.Parsed;
using GraceLine.Text;
using Optional;

namespace GraceLine.Opt
{
    public sealed partial class ShortOption : Option
    {
        private readonly char key;

        public ShortOption(char key)
        {
            this.key = key;
        }

        public override string ToString() => $"-{key}";

        public override Option<Cursor.Item<ParsedOption>> Match(Cursor cursor) => cursor.MatchShort()
            .Filter(this, static (@this, cursor) => cursor.Content[0] == @this.key)
            .Map(
                this,
                static (@this, cursor) => new Cursor.Item<ParsedOption>(
                    new ParsedShortOption(
                        new Located<Option>.Plain(
                            @this,
                            cursor.CurrentToken,
                            cursor.Segment(1)
                        )
                    ),
                    cursor.Feed(1)
                )
            );
    }
}