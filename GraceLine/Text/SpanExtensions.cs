using System;
using GraceLine.Opt;

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

        public static LongOptionMatch? TryMatch(this ReadOnlySpan<char> argument, ReadOnlySpan<char> key)
        {
            if (argument.SequenceEqual(key))
            {
                return LongOptionMatch.Full;
            }

            if (key.StartsWith(argument))
            {
                return LongOptionMatch.Prefix;
            }

            return null;
        }
    }
}