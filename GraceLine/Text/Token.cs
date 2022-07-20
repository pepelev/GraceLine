using System;

namespace GraceLine.Text
{
    public abstract class Token
    {
        public abstract TokenValue Value { get; }
        public abstract Source.Segment Segment(Range valueRange);
        public abstract Source.Segment WholeSegment { get; }
    }
}