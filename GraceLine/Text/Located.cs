namespace GraceLine.Text
{
    public abstract partial class Located<T>
    {
        public abstract T Value { get; }
        public abstract Token Token { get; }
        public abstract Source.Segment Segment { get; }
    }
}