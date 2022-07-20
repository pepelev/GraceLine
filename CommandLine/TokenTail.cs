namespace CommandLine
{
    public readonly struct TokenTail
    {
        public TokenTail(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}