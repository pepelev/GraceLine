namespace CommandLine
{
    public readonly struct Token
    {
        public Token(string value)
        {
            Value = value;
        }

        public string Value { get; }

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