namespace GraceLine.Opt.Parsed
{
    public sealed class MissingParameter : ParsedArgument
    {
        public MissingParameter(Option option)
        {
            Option = option;
        }

        public Option Option { get; }

        public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
    }
}