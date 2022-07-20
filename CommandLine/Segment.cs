using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CommandLine
{
    public abstract class Source
    {
        public abstract string Name { get; }
        public abstract string Content { get; }

        public sealed class Segment
        {
            public ReadOnlySpan<char> Raw => Source.Content.AsSpan()[FromInclusive..ToExclusive];

            public Segment(Source source, int fromInclusive, int toExclusive)
            {
                Source = source;
                FromInclusive = fromInclusive;
                ToExclusive = toExclusive;
            }

            public Source Source { get; }
            public int FromInclusive { get; }
            public int ToExclusive { get; }
            public int Length => ToExclusive - FromInclusive;
            public override string ToString() => new(Raw);
        }
    }

    public sealed class ShellTokens : IEnumerable<Token>
    {
        private readonly Source source;

        public ShellTokens(Source source)
        {
            this.source = source;
        }

        public IEnumerator<Token> GetEnumerator()
        {
            var content = source.Content;
            var temporary = new StringBuilder();
            var i = 0;
            while (i < content.Length)
            {
                var current = content[i];
                if (char.IsWhiteSpace(current))
                {
                    if (temporary.Length > 0)
                    {
                        yield return new Token(temporary.ToString());
                        temporary.Clear();
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

                    i = end + 1;
                    continue;
                }

                temporary.Append(content[i]);
                i++;
            }

            if (temporary.Length > 0)
            {
                yield return new Token(temporary.ToString());
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