﻿using System;

namespace CommandLine.Text
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
    }
}