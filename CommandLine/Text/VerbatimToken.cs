using System;

namespace CommandLine.Text
{
    public sealed class VerbatimToken : Token2
    {
        public VerbatimToken(Source.Segment segment)
        {
            WholeSegment = segment;
        }

        public override string Value => new(WholeSegment.Raw);
        public override Source.Segment WholeSegment { get; }
        public override Source.Segment Segment(Range valueRange) => WholeSegment[valueRange];
    }
}