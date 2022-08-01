namespace GraceLine.Text
{
    public abstract partial class Located<T>
    {
        public sealed class Plain : Located<T>
        {
            public Plain(T value, Token token, Source.Segment segment)
            {
                Value = value;
                Token = token;
                Segment = segment;
            }

            public override T Value { get; }
            public override Token Token { get; }
            public override Source.Segment Segment { get; }
        }
    }
}