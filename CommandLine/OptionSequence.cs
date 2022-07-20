using System;
using System.Collections;
using System.Collections.Generic;
using Optional.Unsafe;

namespace CommandLine
{
    internal sealed class OptionSequence<T> : IEnumerable<T>
    {
        private readonly Optional.Option<T> start;
        private readonly Func<T, Optional.Option<T>> next;

        public OptionSequence(Optional.Option<T> start, Func<T, Optional.Option<T>> next)
        {
            this.start = start;
            this.next = next;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var current = start;
            while (current.HasValue)
            {
                var value = current.ValueOrFailure();
                yield return value;

                current = next(value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}