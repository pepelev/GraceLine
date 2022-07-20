using System.Collections.Generic;
using System.Linq;
using CommandLine.Opt;
using CommandLine.Opt.Parsed;
using NUnit.Framework;

namespace CommandLine
{
    public sealed class Options_Should
    {
        [Test]
        public void Parse_Empty_Arguments_When_Empty()
        {
            var options = new Options();

            var arguments = options.Parse();

            CollectionAssert.IsEmpty(arguments);
        }

        [Test]
        public void Parse_Empty_Arguments_When_Has_Options()
        {
            var options = new Options(
                new ShortOption('a'),
                new ShortOption('b'),
                new ShortOption('c')
            );

            var arguments = options.Parse();

            CollectionAssert.IsEmpty(arguments);
        }

        [Test]
        public void Parse_Short_Options()
        {
            var a = new ShortOption('a');
            var b = new ShortOption('b');
            var c = new ShortOption('c');
            var options = new Options(a, b, c);

            var arguments = options.Parse("-a", "-c", "-b");

            Assert(arguments, a, c, b);
        }

        [Test]
        public void Parse_Duplicating_Short_Options()
        {
            var a = new ShortOption('a');
            var b = new ShortOption('b');
            var c = new ShortOption('c');
            var options = new Options(a, b, c);

            var arguments = options.Parse("-a", "-c", "-a");

            Assert(arguments, a, c, a);
        }

        [Test]
        public void Parse_Multiple_Short_Options_In_One_Token()
        {
            var r = new ShortOption('r');
            var n = new ShortOption('n');
            var x = new ShortOption('x');
            var options = new Options(r, n, x);

            // ReSharper disable StringLiteralTypo
            var arguments = options.Parse("-xrnx");
            // ReSharper restore StringLiteralTypo

            Assert(arguments, x, r, n, x);
        }

        [Test]
        public void Parse_Unknown_Short_Option()
        {
            var r = new ShortOption('r');
            var n = new ShortOption('n');
            var x = new ShortOption('x');
            var options = new Options(r, n, x);

            // ReSharper disable StringLiteralTypo
            var arguments = options.Parse("-x", "-k");
            // ReSharper restore StringLiteralTypo

            Assert(arguments, x, Unrecognized("-k"));
        }

        [Test]
        public void Parse_Unknown_Composite_Short_Option()
        {
            var r = new ShortOption('r');
            var n = new ShortOption('n');
            var x = new ShortOption('x');
            var options = new Options(r, n, x);

            // ReSharper disable StringLiteralTypo
            var arguments = options.Parse("-xkn");
            // ReSharper restore StringLiteralTypo

            Assert(arguments, x, Unrecognized("k"), n);
        }

        [Test]
        public void Parse_Unknown_Short_Option_Which_Is_First_In_Composite()
        {
            var r = new ShortOption('r');
            var n = new ShortOption('n');
            var x = new ShortOption('x');
            var options = new Options(r, n, x);

            // ReSharper disable StringLiteralTypo
            var arguments = options.Parse("-knr");
            // ReSharper restore StringLiteralTypo

            Assert(arguments, Unrecognized("-k"), n, r);
        }

        [Test]
        public void Parse_Long_Options()
        {
            var help = new LongOption("help");
            var output = new LongOption("output");
            var binary = new LongOption("binary");
            var options = new Options(help, output, binary);

            var arguments = options.Parse("--binary", "--help", "--output");

            Assert(arguments, binary, help, output);
        }

        [Test]
        public void Parse_Long_Options_By_Prefix()
        {
            var help = new LongOption("help");
            var quick = new LongOption("quick");
            var quiet = new LongOption("quiet");
            var options = new Options(help, quick, quiet);

            // ReSharper disable StringLiteralTypo
            var arguments = options.Parse("--h", "--quie");
            // ReSharper restore StringLiteralTypo

            Assert(arguments, help, quiet);
        }

        [Test]
        public void Parse_Unknown_Long_Options([Values("--cell", "--sells")] string argument)
        {
            var sell = new LongOption("sell");
            var options = new Options(sell);

            var arguments = options.Parse(argument);

            Assert(arguments, Unrecognized(argument));
        }

        [Test] // todo ambiguous options with parameter and mixed (parameter and without parameter)
        public void Parse_Ambiguous_Prefix_Of_Long_Options()
        {
            var help = new LongOption("help");
            var quick = new LongOption("quick");
            var quiet = new LongOption("quiet");
            var options = new Options(help, quick, quiet);

            // ReSharper disable StringLiteralTypo
            var arguments = options.Parse("--help", "--qui");
            // ReSharper restore StringLiteralTypo

            Assert(arguments, help, Ambiguity(quick, quiet));
        }

        [Test]
        public void Parse_Long_Option_Which_Prefix_Of_Another_Option()
        {
            var help = new LongOption("help");
            var quick = new LongOption("quick");
            var quickly = new LongOption("quickly");
            var options = new Options(help, quick, quickly);

            // ReSharper disable StringLiteralTypo
            var arguments = options.Parse("--quic", "--quick").ToList();
            // ReSharper restore StringLiteralTypo

            Assert(arguments, Ambiguity(quick, quickly), quick);
        }

        [Test] // todo parse incomplete parameterized option "--level" and nothing more
        public void Parse_Long_Parametrized_Option()
        {
            var level = new LongParametrizedOption("level");
            var options = new Options(level);

            var arguments = options.Parse("--level=info");
            Assert(arguments, (level, "info"));
        }

        [Test]
        public void Parse_Long_Parametrized_Option_By_Prefix()
        {
            var level = new LongParametrizedOption("level");
            var options = new Options(level);

            var arguments = options.Parse("--l=error");
            Assert(arguments, (level, "error"));
        }
        [Test]
        public void Parse_Long_Parametrized_Option_By_Prefix_With_Short_Value()
        {
            var level = new LongParametrizedOption("level");
            var options = new Options(level);

            var arguments = options.Parse("--l=1");
            Assert(arguments, (level, "1"));
        }

        [Test]
        // ReSharper disable StringLiteralTypo
        public void Parse_Long_Parametrized_Option_Of_Two_Tokens(
                [Values("--l", "--leve", "--level")] string key,
                [Values("error", "info", "--level", "-a")] string value)
        // ReSharper restore StringLiteralTypo
        {
            var all = new ShortOption('a');
            var level = new LongParametrizedOption("level");
            var options = new Options(level, all);

            var arguments = options.Parse(key, value);
            Assert(arguments, (level, value));
        }

        [Test]
        public void Parse_Ambiguous_Long_Parametrized_Options()
        {
            var all = new ShortOption('a');
            var beep = new LongParametrizedOption("beep");
            var binary = new LongParametrizedOption("binary");
            var options = new Options(all, beep, binary);

            var arguments = options.Parse("-a", "--b=true");
            Assert(arguments, all, Ambiguity(beep, binary));
        }

        [Test]
        public void Parse_NonOption_Argument([Values("filename.txt", "-", "12", "FileName", " space")] string argument)
        {
            var a = new ShortOption('a');
            var b = new ShortOption('b');
            var c = new ShortOption('c');
            var options = new Options(a, b, c);

            var arguments = options.Parse(argument);

            Assert(arguments, argument);
        }

        [Test]
        public void Parse_Options_And_NonOptions()
        {
            var a = new ShortOption('a');
            var b = new ShortOption('b');
            var options = new Options(a, b);

            var arguments = options.Parse("-a", "filename.txt", "-b");

            Assert(arguments, a, "filename.txt", b);
        }

        [Test]
        public void Parse_Only_NonOptions_After_Option_Terminator()
        {
            var a = new ShortOption('a');
            var b = new ShortOption('b');
            var help = new LongOption("help");
            var options = new Options(a, b, help);

            var arguments = options.Parse("-a", "--", "filename.txt", "-b", "--help");

            Assert(arguments, a, OptionTerminator.Singleton, "filename.txt", "-b", "--help");
        }

        [Test]
        public void Parse_Short_Parametrized_Option_In_Single_Token()
        {
            var help = new LongOption("help");
            var count = new ShortParameterizedOption('c');
            var options = new Options(count, help);

            var arguments = options.Parse("-c100");

            Assert(arguments, (count, "100"));
        }

        [Test]
        public void Parse_Short_Parametrized_Option_In_Single_Composite_Token()
        {
            var help = new LongOption("help");
            var count = new ShortParameterizedOption('c');
            var all = new ShortOption('a');
            var options = new Options(all, count, help);

            var arguments = options.Parse("-ac100");

            Assert(arguments, all, (count, "100"));
        }

        [Test] // todo parse incomplete parameterized option "-c" and nothing more
        public void Parse_Short_Parametrized_Option_In_Two_Tokens()
        {
            var help = new LongOption("help");
            var count = new ShortParameterizedOption('c');
            var options = new Options(count, help);

            var arguments = options.Parse("-c", "100");

            Assert(arguments, (count, "100"));
        }

        [Test]
        public void Parse_Number()
        {
            var help = new LongOption("help");
            var count = NumberOption.Singleton;
            var options = new Options(count, help);

            var arguments = options.Parse("--help", "-42");

            Assert(arguments, help, 42);
        }

        [Test]
        public void Parse_Number_And_Short_Option_In_Single_Token()
        {
            var help = new LongOption("help");
            var count = NumberOption.Singleton;
            var quiet = new ShortOption('q');
            var options = new Options(count, help, quiet);

            var arguments = options.Parse("--help", "-42q");

            Assert(arguments, help, 42, quiet);
        }

        [Test]
        public void Parse_Number_As_Unrecognized_When_It_Follows_Short_Option_In_Single_Token()
        {
            var help = new LongOption("help");
            var count = NumberOption.Singleton;
            var quiet = new ShortOption('q');
            var options = new Options(count, help, quiet);

            var arguments = options.Parse("--help", "-q50");

            Assert(arguments, help, quiet, Unrecognized("5"), Unrecognized("0"));
        }

        private static void Assert(IEnumerable<ParsedArgument> arguments, params object[] expected)
        {
            CollectionAssert.AreEqual(
                expected,
                arguments.Select(argument => argument.Accept(AssertConversion.Singleton))
            );
        }

        private static T[] Ambiguity<T>(params T[] options) => options;
        private static string Unrecognized(string option) => $"UNRECOGNIZED: {option}";

        private sealed class AssertConversion : ParsedArgument.Visitor<object>
        {
            public static AssertConversion Singleton { get; } = new();
            public override object Visit(ParsedNonOptionArgument argument) => argument.Value;
            public override object Visit(ParsedShortOption argument) => argument.Option;
            public override object Visit(ParsedLongOption argument) => argument.Option;
            public override object Visit(ParsedNumberOption argument) => argument.Value;
            public override object Visit(ParsedParametrizedOption argument) => (argument.Option, argument.Parameter);
            public override object Visit(OptionTerminator argument) => argument;
            public override object Visit(UnrecognizedOption argument) => Unrecognized(argument.Content);
            public override object Visit(LongOptionAmbiguity argument) => argument.Options.ToArray();
        }
    }
}