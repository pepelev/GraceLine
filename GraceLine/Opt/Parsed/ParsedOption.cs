using GraceLine.Text;

namespace GraceLine.Opt.Parsed
{
    public abstract class ParsedOption : ParsedArgument
    {
        public abstract Located<Option> Option { get; }
    }
}