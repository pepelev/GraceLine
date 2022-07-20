using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CommandLine.Tests
{
    public sealed class ShellTokens_Should
    {
        [Test]
        [TestCase("", TestName = "{m}.Empty")]
        [TestCase(" \t", TestName = "{m}.White_Space")]
        public void Give_No_Tokens_When(string content)
        {
            var sut = new ShellTokens(new WholeInput(content));

            CollectionAssert.IsEmpty(sut);
        }

        [Test]
        public void Give_Tokens()
        {
            var sut = new ShellTokens(new WholeInput("cat dog"));

            CollectionAssert.AreEqual(new[] {"cat", "dog"}, Values(sut));
        }

        [Test]
        public void Give_Trimmed_Tokens()
        {
            var sut = new ShellTokens(new WholeInput(" cat dog\t"));

            CollectionAssert.AreEqual(new[] {"cat", "dog"}, Values(sut));
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

        private static IEnumerable<string> Values(IEnumerable<Token> tokens) => tokens.Select(token => token.Value);
    }
}