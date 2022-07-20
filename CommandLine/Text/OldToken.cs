using System;
using System.Collections.Generic;

namespace CommandLine.Text
{
    internal readonly struct OldToken
    {
        public OldToken(string value)
            : this(value, Array.Empty<Source.Segment>())
        {
        }

        public OldToken(string value, IReadOnlyList<Source.Segment> segments)
        {
            Value = value;
            Segments = segments;
        }

        public string Value { get; }
        public int Length => Value.Length;
        public IReadOnlyList<Source.Segment> Segments { get; }

        public TokenType Type
        {
            get
            {
                if (Value == "-")
                {
                    return TokenType.Hyphen;
                }

                if (Value == "--")
                {
                    return TokenType.DoubleHyphen;
                }

                if (Value.StartsWith("--"))
                {
                    return TokenType.DoubleHyphenPrefixed;
                }

                if (Value.StartsWith("-"))
                {
                    return TokenType.HyphenPrefixed;
                }

                return TokenType.Plain;
            }
        }

        public override string ToString() => Value;
    }
}