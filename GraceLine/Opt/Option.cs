using GraceLine.Cursors;
using GraceLine.Opt.Parsed;
using Optional;

namespace GraceLine.Opt
{
    public abstract class Option
    {
        public abstract Option<Cursor.Item<ParsedOption>> Match(Cursor cursor);
    }
}