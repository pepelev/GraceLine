using GraceLine.Cursors;
using GraceLine.Opt.Parsed;
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
                    static (@this, option) =>
                    {
                        if (option.Content.Length > 1)
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedParametrizedOption(
                                    @this,
                                    new string(option.Content[1..])
                                ),
                                option.FeedToNextToken().Upcast()
                            );
                        }

                        return option.FeedToNextToken().FlatMap(
                            static token => token.MatchWholeToken()
                        ).Match(
                            none: () => new Cursor.Item<ParsedArgument>(
                                new MissingParameter(@this, option.Segment(1)),
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