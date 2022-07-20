using CommandLine.Opt.Parsed;

namespace CommandLine.Opt
{
    public abstract class Option2
    {
        public abstract Optional.Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor);
    }
}