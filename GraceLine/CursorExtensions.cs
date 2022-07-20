using GraceLine.Cursors;
using Optional;

namespace GraceLine
{
    internal static class CursorExtensions
    {
        public static Option<Cursor> Upcast(this Option<TokenStart> token) => token.Map(start => (Cursor)start);
    }
}