using CommandLine.Opt.Parsed;

namespace CommandLine.Opt
{
    public abstract class Option
    {
        public abstract Optional.Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor);
    }
}