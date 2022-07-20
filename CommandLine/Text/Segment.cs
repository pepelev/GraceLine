using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace CommandLine.Text
{
    // https://www.gnu.org/software/bash/manual/html_node/Quoting.html
    public sealed class ShellTokens : IEnumerable<Token>, IEnumerable<Token2>
    {
        private readonly Source source;

        public ShellTokens(Source source)
        {
            this.source = source;
        }

        private sealed class ThisToken : Token2
        {
            private readonly List<int> escaping;

            public ThisToken(
                Source.Segment segment,
                List<int> escaping)
            {
                WholeSegment = segment;
                this.escaping = escaping;
            }

            public override string Value => string.Create(
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
        }

        IEnumerator<Token2> IEnumerable<Token2>.GetEnumerator()
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

        private static Token2 ParseToken(Source.Segment tail)
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

        public IEnumerator<Token> GetEnumerator()
        {
            var content = source.Content;
            var temporary = new StringBuilder();
            var i = 0;
            int? start = null;
            while (i < content.Length)
            {
                var current = content[i];
                if (char.IsWhiteSpace(current))
                {
                    if (temporary.Length > 0)
                    {
                        yield return new Token(
                            temporary.ToString(),
                            new[]
                            {
                                new Source.Segment(source, start.Value, i)
                            }
                        );
                        temporary.Clear();
                        start = null;
                    }

                    i++;
                    continue;
                }

                if (current == '\'')
                {
                    var end = content.IndexOf('\'', startIndex: i + 1) switch
                    {
                        -1 => content.Length,
                        var index => index
                    };
                    temporary.Append(content.AsSpan()[(i + 1)..end]);
                    start ??= i;
                    i = end + 1;
                    continue;
                }

                if (current == '"')
                {
                    var end = i + 1;
                    while (end < content.Length)
                    {
                        if (content[end] == '\\')
                        {
                            if (end + 1 < content.Length)
                            {
                                temporary.Append(content[end + 1]);
                                end += 2;
                            }
                            else
                            {
                                temporary.Append('\\');
                                end++;
                            }
                        }
                        else if (content[end] == '\"')
                        {
                            break;
                        }
                        else
                        {
                            temporary.Append(content[end]);
                            end++;
                        }
                    }

                    start ??= i;
                    i = end + 1;
                    continue;
                }

                temporary.Append(content[i]);
                start ??= i;
                i++;
            }

            if (temporary.Length > 0)
            {
                yield return new Token(
                    temporary.ToString(),
                    new[]
                    {
                        new Source.Segment(source, start.Value, content.Length)
                    }
                );
            }
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