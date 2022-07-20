using System;
using System.Collections;
using System.Collections.Generic;
using Optional;
using Optional.Unsafe;

namespace GraceLine
{
    internal sealed class OptionSequence<T> : IEnumerable<T>
    {
        private readonly Option<T> start;
        private readonly Func<T, Option<T>> next;

        public OptionSequence(Option<T> start, Func<T, Option<T>> next)
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