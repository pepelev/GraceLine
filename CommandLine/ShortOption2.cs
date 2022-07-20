namespace CommandLine
{
    public readonly struct ShortOption2
    {
        public ShortOption2(Cursor.Item<char> key, Cursor.Item<TokenTail> tail, Cursor.Item<TokenTail> whole)
        {
            Key = key;
            Tail = tail;
            Whole = whole;
        }

        public Cursor.Item<char> Key { get; }
        public Cursor.Item<TokenTail> Tail { get; }
        public Cursor.Item<TokenTail> Whole { get; }
    }
}