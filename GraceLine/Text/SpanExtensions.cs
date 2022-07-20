using System;

namespace GraceLine.Text
{
    internal static class SpanExtensions
    {
        public static int IndexOf(this ReadOnlySpan<char> span, char target, int startIndex)
            => span[startIndex..].IndexOf(target) switch
            {
                < 0 => -1,
                var index => index + startIndex
            };
    }
}