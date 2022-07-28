using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public sealed class ParsedNonOptionArgument : ParsedArgument
    {
        public ParsedNonOptionArgument(Token token)
        {
            Token = token;
        }

        public Token Token { get; }
        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}