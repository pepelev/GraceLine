namespace GraceLine.Text
{
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