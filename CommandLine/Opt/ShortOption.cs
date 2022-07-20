using CommandLine.Cursors;
using CommandLine.Opt.Parsed;
using Optional;

namespace CommandLine.Opt
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
            .Filter(option => option.Content[0] == key)
            .Map(option => new Cursor.Item<ParsedArgument>(
                    new ParsedShortOption(this),
                    option.Skip(1)
                )
            );
    }
}