using System.Collections.Generic;
using System.Linq;
using GraceLine.Text;
using NUnit.Framework;

namespace GraceLine.Tests
{
    public sealed class ShellTokens_Should
    {
        [Test]
        [TestCase("", TestName = "{m}.Empty")]
        [TestCase(" \t", TestName = "{m}.White_Space")]
        public void Give_No_Tokens_When(string content)
        {
            var sut = new ShellTokens(new WholeInput(content));

            CollectionAssert.IsEmpty(((IEnumerable<Token>)sut).ToList());
        }

        [Test]
        public void Give_Tokens()
        {
            var sut = new ShellTokens(new WholeInput("cat dog"));

            CollectionAssert.AreEqual(new[] { ("cat", 0, 3), ("dog", 4, 7) }, Tuples(sut));
        }

        [Test]
        public void Give_Trimmed_Tokens()
        {
            var sut = new ShellTokens(new WholeInput(" cat dog\t"));

            CollectionAssert.AreEqual(new[] {"cat", "dog"}, Values(sut));
        }

        [Test]
        public void Give_Quoted_Token()
        {
            var sut = new ShellTokens(new WholeInput(" 'cat' "));

            CollectionAssert.AreEqual(new[] { "cat" }, Values(sut));
        }

        [Test]
        public void Give_Quoted_Tokens()
        {
            var sut = new ShellTokens(new WholeInput(" 'cat' \"dog\""));

            CollectionAssert.AreEqual(new[] { "cat", "dog" }, Values(sut));
        }

        [Test]
        public void Give_Quoted_Token_With_Spaces()
        {
            var sut = new ShellTokens(new WholeInput("' cat '"));

            CollectionAssert.AreEqual(new[] { " cat " }, Values(sut));
        }

        [Test]
        public void Give_Quoted_And_Adjacent_Unquoted_Input_As_Token()
        {
            // ReSharper disable StringLiteralTypo
            var sut = new ShellTokens(new WholeInput("'green'blue"));

            CollectionAssert.AreEqual(new[] { "greenblue" }, Values(sut));
            // ReSharper restore StringLiteralTypo
        }

        [Test]
        public void Give_Single_Token_When_Space_Escaped()
        {
            var sut = new ShellTokens(new WholeInput(@"cat\ dog"));

            CollectionAssert.AreEqual(new[] { "cat dog" }, Values(sut));

        }

        private static IEnumerable<string> Values(IEnumerable<Token> tokens) => tokens.Select(token => token.Value.Content);

        private static IEnumerable<(string Value, int From, int To)> Tuples(IEnumerable<Token> tokens) =>
            tokens.Select(
                token =>
                {
                    var segment = token.WholeSegment;
                    return (token.Value.Content, segment.FromInclusive, segment.ToExclusive);
                }
            );
    }
}