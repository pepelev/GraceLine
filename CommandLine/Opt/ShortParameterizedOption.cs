using CommandLine.Opt.Parsed;
using Optional;

namespace CommandLine.Opt
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
                .Filter(option => option.Content[0] == key)
                .Map(option =>
                    {
                        if (option.Content.Length > 1)
                        {
                            return new Cursor.Item<ParsedArgument>(
                                new ParsedParametrizedOption(
                                    this,
                                    new string(option.Content[1..])
                                ),
                                option.Skip(option.Content.Length) // todo skip all token
                            );
                        }

                        return option.Skip(1).FlatMap(
                            token => token.MatchWholeToken()
                        ).Match(
                            none: () => new Cursor.Item<ParsedArgument>(
                                new MissingParameter(this),
                                Optional.Option.None<Cursor>()
                            ),
                            some: token => new Cursor.Item<ParsedArgument>(
                                new ParsedParametrizedOption(
                                    this,
                                    token.CurrentToken.Value
                                ),
                                token.Next.Upcast()
                            )
                        );
                    }
                );
        }
    }
}