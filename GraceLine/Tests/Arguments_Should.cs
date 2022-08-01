using System.Collections.Generic;
using System.Linq;
using GraceLine.Opt;
using GraceLine.Opt.Parsed;
using NUnit.Framework;

namespace GraceLine.Tests
{
    public sealed class Arguments_Should
    {
        private sealed class Terminator
        {
            public static Terminator Singleton { get; } = new();
            public override string ToString() => "--";
        }

        [Test]
        public void Parse_Empty_Arguments_When_Empty()
        {
            var arguments = new Arguments();

            var parsedArguments = arguments.Parse();

            CollectionAssert.IsEmpty(parsedArguments);
        }

        [Test]
        public void Parse_Empty_Arguments_When_Has_Options()
        {
            var arguments = new Arguments(
                new ShortOption('a'),
                new ShortOption('b'),
                new ShortOption('c')
            );

            var parsedArguments = arguments.Parse();

            CollectionAssert.IsEmpty(parsedArguments);
        }

        [Test]
        public void Parse_Short_Options()
        {
            var a = new ShortOption('a');
            var b = new ShortOption('b');
            var c = new ShortOption('c');
            var arguments = new Arguments(a, b, c);

            var parsedArguments = arguments.Parse("-a", "-c", "-b");

            Assert(parsedArguments, a, c, b);
        }

        [Test]
        public void Parse_Duplicating_Short_Options()
        {
            var a = new ShortOption('a');
            var b = new ShortOption('b');
            var c = new ShortOption('c');
            var arguments = new Arguments(a, b, c);

            var parsedArguments = arguments.Parse("-a", "-c", "-a");

            Assert(parsedArguments, a, c, a);
        }

        [Test]
        public void Parse_Multiple_Short_Options_In_One_Token()
        {
            var r = new ShortOption('r');
            var n = new ShortOption('n');
            var x = new ShortOption('x');
            var arguments = new Arguments(r, n, x);

            // ReSharper disable StringLiteralTypo
            var parsedArguments = arguments.Parse("-xrnx");
            // ReSharper restore StringLiteralTypo

            Assert(parsedArguments, x, r, n, x);
        }

        [Test]
        public void Parse_Unknown_Short_Option()
        {
            var r = new ShortOption('r');
            var n = new ShortOption('n');
            var x = new ShortOption('x');
            var arguments = new Arguments(r, n, x);

            // ReSharper disable StringLiteralTypo
            var parsedArguments = arguments.Parse("-x", "-k");
            // ReSharper restore StringLiteralTypo

            Assert(parsedArguments, x, Unrecognized("k"));
        }

        [Test]
        public void Parse_Unknown_Composite_Short_Option()
        {
            var r = new ShortOption('r');
            var n = new ShortOption('n');
            var x = new ShortOption('x');
            var arguments = new Arguments(r, n, x);

            // ReSharper disable StringLiteralTypo
            var parsedArguments = arguments.Parse("-xkn");
            // ReSharper restore StringLiteralTypo

            Assert(parsedArguments, x, Unrecognized("k"), n);
        }

        [Test]
        public void Parse_Unknown_Short_Option_Which_Is_First_In_Composite()
        {
            var r = new ShortOption('r');
            var n = new ShortOption('n');
            var x = new ShortOption('x');
            var arguments = new Arguments(r, n, x);

            // ReSharper disable StringLiteralTypo
            var parsedArguments = arguments.Parse("-knr");
            // ReSharper restore StringLiteralTypo

            Assert(parsedArguments, Unrecognized("k"), n, r);
        }

        [Test]
        public void Parse_Long_Options()
        {
            var help = new LongOption("help");
            var output = new LongOption("output");
            var binary = new LongOption("binary");
            var arguments = new Arguments(help, output, binary);

            var parsedArguments = arguments.Parse("--binary", "--help", "--output");

            Assert(parsedArguments, binary, help, output);
        }

        [Test]
        public void Parse_Long_Options_By_Prefix()
        {
            var help = new LongOption("help");
            var quick = new LongOption("quick");
            var quiet = new LongOption("quiet");
            var arguments = new Arguments(help, quick, quiet);

            // ReSharper disable StringLiteralTypo
            var parsedArguments = arguments.Parse("--h", "--quie");
            // ReSharper restore StringLiteralTypo

            Assert(parsedArguments, help, quiet);
        }

        [Test]
        public void Parse_Unknown_Long_Options([Values("--cell", "--sells")] string argument)
        {
            var sell = new LongOption("sell");
            var arguments = new Arguments(sell);

            var parsedArguments = arguments.Parse(argument);

            Assert(parsedArguments, Unrecognized(argument));
        }

        [Test] // todo ambiguous options with parameter and mixed (parameter and without parameter)
        public void Parse_Ambiguous_Prefix_Of_Long_Options()
        {
            var help = new LongOption("help");
            var quick = new LongOption("quick");
            var quiet = new LongOption("quiet");
            var arguments = new Arguments(help, quick, quiet);

            // ReSharper disable StringLiteralTypo
            var parsedArguments = arguments.Parse("--help", "--qui");
            // ReSharper restore StringLiteralTypo

            Assert(parsedArguments, help, Ambiguity(quick, quiet));
        }

        [Test]
        public void Parse_Long_Option_Which_Prefix_Of_Another_Option()
        {
            var help = new LongOption("help");
            var quick = new LongOption("quick");
            var quickly = new LongOption("quickly");
            var arguments = new Arguments(help, quick, quickly);

            // ReSharper disable StringLiteralTypo
            var parsedArguments = arguments.Parse("--quick").ToList();
            // ReSharper restore StringLiteralTypo

            Assert(parsedArguments, quick);
        }

        [Test]
        public void Parse_Long_Parametrized_Option()
        {
            var level = new LongParametrizedOption("level");
            var arguments = new Arguments(level);

            var parsedArguments = arguments.Parse("--level=info");
            Assert(parsedArguments, (level, "info"));
        }

        [Test]
        public void Parse_Long_Parametrized_Option_By_Prefix()
        {
            var level = new LongParametrizedOption("level");
            var arguments = new Arguments(level);

            var parsedArguments = arguments.Parse("--l=error");
            Assert(parsedArguments, (level, "error"));
        }

        [Test]
        public void Parse_Long_Parametrized_Option_By_Prefix_With_Short_Value()
        {
            var level = new LongParametrizedOption("level");
            var arguments = new Arguments(level);

            var parsedArguments = arguments.Parse("--l=1");
            Assert(parsedArguments, (level, "1"));
        }

        [Test]
        // ReSharper disable StringLiteralTypo
        public void Parse_Long_Parametrized_Option_Of_Two_Tokens(
                [Values("--l", "--leve", "--level")] string key,
                [Values("error", "info", "--level", "-a")]
                string value)
            // ReSharper restore StringLiteralTypo
        {
            var all = new ShortOption('a');
            var level = new LongParametrizedOption("level");
            var arguments = new Arguments(level, all);

            var parsedArguments = arguments.Parse(key, value);
            Assert(parsedArguments, (level, value));
        }

        [Test]
        public void Parse_Long_Parametrized_Option_With_Missing_Parameter()
        {
            var level = new LongParametrizedOption("level");
            var arguments = new Arguments(level);

            var parsedArguments = arguments.Parse("--level");
            Assert(parsedArguments, (level, "MISSING"));
        }

        [Test]
        public void Parse_Ambiguous_Long_Parametrized_Options()
        {
            var all = new ShortOption('a');
            var beep = new LongParametrizedOption("beep");
            var binary = new LongParametrizedOption("binary");
            var arguments = new Arguments(all, beep, binary);

            var parsedArguments = arguments.Parse("-a", "--b=true");
            Assert(parsedArguments, all, Ambiguity(beep, binary));
        }

        [Test]
        public void Parse_Long_Option_With_Optional_Parameter()
        {
            var beep = new LongOptionallyParametrizedOption("beep");
            var arguments = new Arguments(beep);

            var parsedArguments = arguments.Parse("--beep=loud");
            Assert(parsedArguments, (beep, "loud"));
        }

        [Test]
        public void Parse_Long_Option_With_Optional_Parameter_By_Prefix()
        {
            var beep = new LongOptionallyParametrizedOption("beep");
            var arguments = new Arguments(beep);

            var parsedArguments = arguments.Parse("--b=loud");
            Assert(parsedArguments, (beep, "loud"));
        }

        [Test]
        public void Parse_Long_Option_With_Empty_Optional_Parameter()
        {
            var beep = new LongOptionallyParametrizedOption("beep");
            var arguments = new Arguments(beep);

            var parsedArguments = arguments.Parse("--beep=");
            Assert(parsedArguments, (beep, ""));
        }

        [Test]
        public void Parse_Long_Option_Without_Optional_Parameter()
        {
            var beep = new LongOptionallyParametrizedOption("beep");
            var arguments = new Arguments(beep);

            var parsedArguments = arguments.Parse("--beep");
            Assert(parsedArguments, (beep, "NO_PARAMETER"));
        }

        [Test]
        public void Parse_Long_Option_Without_Optional_Parameter2()
        {
            var beep = new LongOptionallyParametrizedOption("beep");
            var arguments = new Arguments(beep);

            var parsedArguments = arguments.Parse("--beep", "argument");
            Assert(parsedArguments, (beep, "NO_PARAMETER"), "argument");
        }

        [Test]
        public void Parse_Long_Option_Mimic()
        {
            var beep = new LongOptionallyParametrizedOption("beep");
            var arguments = new Arguments(beep);

            var parsedArguments = arguments.Parse("--=", "argument");
            Assert(parsedArguments, Unrecognized("--="), "argument");
        }

        [Test]
        public void Parse_NonOption_Argument([Values("filename.txt", "-", "12", "FileName", " space")] string argument)
        {
            var a = new ShortOption('a');
            var b = new ShortOption('b');
            var c = new ShortOption('c');
            var arguments = new Arguments(a, b, c);

            var parsedArguments = arguments.Parse(argument);

            Assert(parsedArguments, argument);
        }

        [Test]
        public void Parse_Options_And_NonOptions()
        {
            var a = new ShortOption('a');
            var b = new ShortOption('b');
            var arguments = new Arguments(a, b);

            var parsedArguments = arguments.Parse("-a", "filename.txt", "-b");

            Assert(parsedArguments, a, "filename.txt", b);
        }

        [Test]
        public void Parse_Only_NonOptions_After_Option_Terminator()
        {
            var a = new ShortOption('a');
            var b = new ShortOption('b');
            var help = new LongOption("help");
            var count = NumberOption.Singleton;
            var arguments = new Arguments(a, b, help, count);

            var parsedArguments = arguments.Parse("-a", "--", "filename.txt", "-b", "--help", "-5");

            Assert(parsedArguments, a, Terminator.Singleton, "filename.txt", "-b", "--help", "-5");
        }

        [Test]
        public void Parse_Short_Parametrized_Option_In_Single_Token()
        {
            var help = new LongOption("help");
            var count = new ShortOption.WithParameter('c');
            var arguments = new Arguments(count, help);

            var parsedArguments = arguments.Parse("-c100");

            Assert(parsedArguments, (count, "100"));
        }

        [Test]
        public void Parse_Short_Parametrized_Option_In_Single_Composite_Token()
        {
            var help = new LongOption("help");
            var count = new ShortOption.WithParameter('c');
            var all = new ShortOption('a');
            var arguments = new Arguments(all, count, help);

            var parsedArguments = arguments.Parse("-ac100");

            Assert(parsedArguments, all, (count, "100"));
        }

        [Test]
        public void Parse_Short_Parametrized_Option_In_Two_Tokens()
        {
            var help = new LongOption("help");
            var count = new ShortOption.WithParameter('c');
            var arguments = new Arguments(count, help);

            var parsedArguments = arguments.Parse("-c", "100");

            Assert(parsedArguments, (count, "100"));
        }

        [Test]
        public void Parse_Short_Parametrized_Option_With_Missing_Parameter()
        {
            var all = new ShortOption('a');
            var help = new LongOption("help");
            var count = new ShortOption.WithParameter('c');
            var arguments = new Arguments(count, help, all);

            var parsedArguments = arguments.Parse("-ac");

            Assert(parsedArguments, all, (count, "MISSING"));
        }

        [Test]
        public void Parse_Number()
        {
            var help = new LongOption("help");
            var count = NumberOption.Singleton;
            var arguments = new Arguments(count, help);

            var parsedArguments = arguments.Parse("--help", "-42");

            Assert(parsedArguments, help, 42);
        }

        [Test]
        public void Parse_Number_And_Short_Option_In_Single_Token()
        {
            var help = new LongOption("help");
            var count = NumberOption.Singleton;
            var quiet = new ShortOption('q');
            var arguments = new Arguments(count, help, quiet);

            var parsedArguments = arguments.Parse("--help", "-42q");

            Assert(parsedArguments, help, 42, quiet);
        }

        [Test]
        public void Parse_Number_As_Unrecognized_When_It_Follows_Short_Option_In_Single_Token()
        {
            var help = new LongOption("help");
            var count = NumberOption.Singleton;
            var quiet = new ShortOption('q');
            var arguments = new Arguments(count, help, quiet);

            var parsedArguments = arguments.Parse("--help", "-q50");

            Assert(parsedArguments, help, quiet, Unrecognized("5"), Unrecognized("0"));
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
            public override object Visit(ParsedNonOptionArgument argument) => argument.Token.Value.Content;
            public override object Visit(ParsedShortOption argument) => argument.Option.Value;
            public override object Visit(ParsedLongOption argument) => argument.Option.Value;
            public override object Visit(ParsedNumber argument) => argument.Value;
            public override object Visit(ParsedParametrizedOption argument) => (argument.Option, argument.Argument ?? "NO_PARAMETER");
            public override object Visit(OptionTerminator argument) => Terminator.Singleton;
            public override object Visit(UnrecognizedOption argument) => Unrecognized(argument.Content.Value);
            public override object Visit(LongOptionAmbiguity argument) => argument.Options.ToArray();
            public override object Visit(MissingParameter argument) => (argument.Option.Value, "MISSING");
        }
    }
}