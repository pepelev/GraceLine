using System;
using GraceLine.Cursors;
using Optional;
using Optional.Unsafe;

namespace GraceLine
{
    internal static class Extensions
    {
        public static Option<Cursor> Upcast(this Option<TokenStart> token) => token.Map(start => (Cursor)start);

        public static Option<R> Map<T, Context, R>(
            this Option<T> option,
            Context context,
            Func<Context, T, R> map)
        {
            if (!option.HasValue)
            {
                return Option.None<R>();
            }

            var value = option.ValueOrFailure();
            return Option.Some(map(context, value));
        }

        public static Option<R> FlatMap<T, Context, R>(
            this Option<T> option,
            Context context,
            Func<Context, T, Option<R>> map)
        {
            if (!option.HasValue)
            {
                return Option.None<R>();
            }

            var value = option.ValueOrFailure();
            return map(context, value);
        }

        public static Option<T> Filter<T, Context>(
            this Option<T> option,
            Context context,
            Func<Context, T, bool> filter)
        {
            if (!option.HasValue)
            {
                return Option.None<T>();
            }

            var value = option.ValueOrFailure();
            return filter(context, value)
                ? option
                : Option.None<T>();
        }

        public static R Match<T, Context, R>(
            this Option<T> option,
            Context context,
            Func<Context, T, R> some,
            Func<Context, R> none)
            => option.HasValue
                ? some(context, option.ValueOrFailure())
                : none(context);
    }
}