using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace GraceLine.Text
{
    // https://www.gnu.org/software/bash/manual/html_node/Quoting.html
    public sealed class ShellTokens : IEnumerable<Token>
    {
        private readonly Source source;

        public ShellTokens(Source source)
        {
            this.source = source;
        }

        private sealed class ThisToken : Token
        {
            private readonly List<int> escaping;

            public ThisToken(
                Source.Segment segment,
                List<int> escaping)
            {
                WholeSegment = segment;
                this.escaping = escaping;
            }

            public override TokenValue Value => new(
                string.Create(
                    Length,
                    this,
                    static (span, @this) =>
                    {
                        var content = @this.WholeSegment.Raw;
                        var j = 0;
                        for (var i = 0; i < content.Length; i++)
                        {
                            var found = @this.escaping.BinarySearch(i) >= 0;
                            if (found)
                            {
                                continue;
                            }

                            span[j++] = content[i];
                        }
                    }
                )
            );
            

            private int Length => WholeSegment.Length - escaping.Count;

            [Pure]
            private int CountLessOrEqual(int value) => escaping.BinarySearch(value) switch
            {
                var found and >= 0 => found,
                var negative => ~negative
            };

            private int Locate(int index)
            {
                var result = index;
                while (result - CountLessOrEqual(result) < index)
                {
                    result++;
                }

                return result;
            }

            public override Source.Segment Segment(Range valueRange)
            {
                var (offset, length) = valueRange.GetOffsetAndLength(Length);
                var start = Locate(offset);
                var end = Locate(start + length);
                return WholeSegment[start..end];
            }

            public override Source.Segment WholeSegment { get; }
            public override string ToString() => Value.ToString();
        }

        public IEnumerator<Token> GetEnumerator()
        {
            var segment = new Source.Segment(source);
            while (true)
            {
                var offset = 0;
                var content = segment.Raw;
                while (offset < segment.Length && char.IsWhiteSpace(content[offset]))
                {
                    offset++;
                }

                if (offset >= segment.Length)
                {
                    yield break;
                }

                var token = ParseToken(segment[offset..]);
                yield return token;

                segment = new Source.Segment(
                    source,
                    token.WholeSegment.ToExclusive,
                    segment.ToExclusive
                );
            }
        }

        private static Token ParseToken(Source.Segment tail)
        {
            var content = tail.Raw;
            var escapes = new List<int>();
            var offset = 0;
            while (offset < content.Length)
            {
                if (char.IsWhiteSpace(content[offset]))
                {
                    break;
                }

                if (content[offset] == '\'')
                {
                    escapes.Add(offset);
                    var pairIndex = content.IndexOf('\'', startIndex: offset + 1);
                    if (pairIndex < 0)
                    {
                        offset = content.Length;
                        break;
                    }

                    offset = pairIndex + 1;
                    escapes.Add(pairIndex);
                    continue;
                }

                if (content[offset] == '"')
                {
                    escapes.Add(offset);
                    var pairIndex = content.IndexOf('"', startIndex: offset + 1);
                    if (pairIndex < 0)
                    {
                        offset = content.Length;
                        break;
                    }

                    offset = pairIndex + 1;
                    escapes.Add(pairIndex);
                    continue;
                }

                offset++;
                while (offset < content.Length)
                {
                    if (char.IsWhiteSpace(content[offset]))
                    {
                        break;
                    }

                    if (content[offset] == '\\')
                    {
                        escapes.Add(offset);
                        offset = Math.Min(content.Length, offset + 2);
                    }
                    else
                    {
                        offset++;
                    }
                }
            }

            return new ThisToken(tail[..offset], escapes);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class WholeInput : Source
    {
        private readonly string content;

        public WholeInput(string content)
        {
            this.content = content;
        }

        public override string Name => nameof(WholeInput);
        public override string Content => content;
        public override string ToString() => content;
    }
}