using System;

namespace CommandLine.Text
{
    public abstract partial class Source
    {
        public sealed class Segment
        {
            public ReadOnlySpan<char> Raw => Source.Content.AsSpan()[FromInclusive..ToExclusive];

            public Segment(Source source)
                : this(source, 0, source.Content.Length)
            {
            }

            public Segment(Source source, int fromInclusive, int toExclusive)
            {
                if (fromInclusive > toExclusive)
                {
                    throw new ArgumentException(
                        nameof(fromInclusive) + " must be less or equal to " + nameof(toExclusive)
                    );
                }

                Source = source;
                FromInclusive = fromInclusive;
                ToExclusive = toExclusive;
            }

            public Source Source { get; }
            public int FromInclusive { get; }
            public int ToExclusive { get; }
            public int Length => ToExclusive - FromInclusive;

            public Segment this[Range range]
            {
                get
                {
                    if (range.Equals(..))
                    {
                        return this;
                    }

                    var (offset, length) = range.GetOffsetAndLength(Length);
                    return new Segment(
                        Source,
                        FromInclusive + offset,
                        FromInclusive + offset + length
                    );
                }
            }

            public override string ToString() => new(Raw);
        }
    }
}