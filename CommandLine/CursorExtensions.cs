using Optional;

namespace CommandLine
{
    internal static class CursorExtensions
    {
        public static Option<Cursor> Upcast(this Option<TokenStart> token) => token.Map(start => (Cursor)start);
    }
}