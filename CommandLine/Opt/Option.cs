using CommandLine.Opt.Parsed;
using Optional;

namespace CommandLine.Opt
{
    public abstract class Option
    {
        public abstract Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor);
    }
}