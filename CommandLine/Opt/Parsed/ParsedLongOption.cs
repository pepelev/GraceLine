using CommandLine.Text;

namespace CommandLine.Opt.Parsed
{
    public sealed class ParsedLongOption : ParsedArgument
    {
        public ParsedLongOption(Option option, LongOptionMatch match, Token token)
        {
            Option = option;
            Match = match;
            Token = token;
        }

        public Option Option { get; }
        public LongOptionMatch Match { get; }
        public Token Token { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}