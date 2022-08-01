namespace GraceLine.Text
{
    public abstract partial class Located<T>
    {
        public sealed class WholeToken : Located<T>
        {
            public WholeToken(T value, Token token)
            {
                Value = value;
                Token = token;
            }

            public override T Value { get; }
            public override Token Token { get; }
            public override Source.Segment Segment => Token.WholeSegment;
        }
    }
}