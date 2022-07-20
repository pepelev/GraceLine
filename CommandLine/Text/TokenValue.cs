namespace CommandLine.Text
{
    public readonly struct TokenValue
    {
        public TokenValue(string content)
        {
            Content = content;
        }

        public string Content { get; }

        public TokenType Type
        {
            get
            {
                if (Content == "-")
                {
                    return TokenType.Hyphen;
                }

                if (Content == "--")
                {
                    return TokenType.DoubleHyphen;
                }

                if (Content.StartsWith("--"))
                {
                    return TokenType.DoubleHyphenPrefixed;
                }

                if (Content.StartsWith("-"))
                {
                    return TokenType.HyphenPrefixed;
                }

                return TokenType.Plain;
            }
        }

        public override string ToString() => Content;
    }
}