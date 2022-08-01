using GraceLine.Cursors;
using GraceLine.Opt.Parsed;
using GraceLine.Text;
using Optional;

namespace GraceLine.Opt
{
    public sealed class ShortParameterizedOption : Option
    {
        private readonly char key;

        public ShortParameterizedOption(char key)
        {
            this.key = key;
        }

        public override Option<Cursor.Item<ParsedArgument>> Match(Cursor cursor)
        {
            return cursor.MatchShort()
                .Filter(this, static (@this, option) => option.Content[0] == @this.key)
                .Map(
                    this,
                    static (@this, cursor) =>
                    {
                        if (cursor.Content.Length > 1)
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedParametrizedOption(
                                    @this,
                                    new string(cursor.Content[1..])
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
                                new ParsedParametrizedOption(
                                    @this,
                                    token.CurrentToken.Value.Content
                                ),
                                token.Next.Upcast()
                            )
                        );
                    }
                );
        }
    }
}