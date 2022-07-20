using System;
using System.Collections.Generic;

namespace CommandLine
{
    public readonly struct Token
    {
        public Token(string value)
            : this(value, Array.Empty<Source.Segment>())
        {
        }

        public Token(string value, IReadOnlyList<Source.Segment> segments)
        {
            Value = value;
            Segments = segments;
        }

        public override string ToString() => Value;

        public string Value { get; }
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

        public int Length => Value.Length;
    }
}