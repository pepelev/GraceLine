using System;

namespace GraceLine.Text
{
    public sealed class VerbatimToken : Token
    {
        public VerbatimToken(Source.Segment segment)
        {
            WholeSegment = segment;
        }

        public override TokenValue Value => new(new string(WholeSegment.Raw));
        public override Source.Segment WholeSegment { get; }
        public override Source.Segment Segment(Range valueRange) => WholeSegment[valueRange];
        public override string ToString() => WholeSegment.ToString();
    }
}