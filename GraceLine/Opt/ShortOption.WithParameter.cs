using GraceLine.Cursors;
using GraceLine.Opt.Parsed;
using GraceLine.Text;
using Optional;

namespace GraceLine.Opt
{
    public sealed partial class ShortOption
    {
        public sealed class WithParameter : Option
        {
            private readonly char key;

            public WithParameter(char key)
            {
                this.key = key;
            }

            public override string ToString() => $"-{key} ?";

            public override Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor) =>
                cursor.MatchShort()
                    .Filter(this, static (@this, option) => option.Content[0] == @this.key)
                    .Map(
                        this,
                        static (@this, cursor) =>
                        {
                            if (cursor.Content.Length > 1)
                            {
                                return new Cursor.Item<ParsedArgument>(
                                    new ParsedLongOption.WithParameter(
                                        new Located<Option>.Plain(
                                            @this,
                                            cursor.CurrentToken,
                                            cursor.Segment(1)
                                        ),
                                        new Located<string>.Plain(
                                            new string(cursor.Content[1..]),
                                            cursor.CurrentToken,
                                            cursor.FeedPart(1).SegmentToEnd
                                        )
                                    ),
                                    cursor.FeedToNextToken().Upcast()
                                );
                            }

                            return cursor.FeedToNextToken().FlatMap(
                                static token => token.MatchWholeToken()
                            ).Match(
                                none: () => new Cursor.Item<ParsedArgument>(
                                    new MissingParameter(
                                        new Located<Option>.Plain(
                                            @this,
                                            cursor.CurrentToken,
                                            cursor.Segment(1)
                                        )
                                    ),
                                    Optional.Option.None<Cursor>()
                                ),
                                some: token => new Cursor.Item<ParsedArgument>(
                                    new ParsedLongOption.WithParameter(
                                        new Located<Option>.Plain(
                                            @this,
                                            cursor.CurrentToken,
                                            cursor.SegmentToEnd
                                        ),
                                        new Located<string>.WholeToken(
                                            token.CurrentToken.Value.Content,
                                            token.CurrentToken
                                        )
                                    ),
                                    token.Next.Upcast()
                                )
                            );
                        }
                    );
        }
    }
}