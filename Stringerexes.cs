using System;
using System.Globalization;

namespace RT.Generexes
{
    /// <summary>Contains some pre-defined regular expressions for matching strings.</summary>
    public static class Stringerexes
    {
        /// <summary>Matches a single digit (any Unicode character designated as a “digit”; not just the ASCII digits).</summary>
        public static readonly Stringerex Digit = new Stringerex(char.IsDigit);
        /// <summary>Matches a single ASCII digit.</summary>
        public static readonly Stringerex Digit0to9 = new Stringerex(ch => ch >= '0' && ch <= '9');

        private static readonly Stringerex positiveInteger = Digit0to9.RepeatGreedy(1);
        private static readonly Stringerex optionalMinus = new Stringerex("-").OptionalGreedy();
        private static readonly Stringerex integer = optionalMinus.Then(positiveInteger);

        /// <summary>Parses a positive integer.</summary>
        public static readonly Stringerex<long> PositiveInteger = positiveInteger.Process(m => Convert.ToInt64(m.Match));
        /// <summary>Parses an integer that may have a leading minus sign.</summary>
        public static readonly Stringerex<long> Integer = integer.Process(m => Convert.ToInt64(m.Match));

        /// <summary>Parses a decimal number, allowing fractional parts and exponential notation.</summary>
        public static readonly Stringerex<double> Number = optionalMinus
            .Then(Digit0to9.RepeatGreedy()
                .Then(new Stringerex(".").Then(Digit0to9.RepeatGreedy()).OptionalGreedy())
                .Do(m => m.Length > 0))
            .Then(Stringerex.Ors("e", "E").Then(Stringerex.Ors("+", "-", "")).Then(Digit0to9.RepeatGreedy(1)).OptionalGreedy())
            .Process(m => Convert.ToDouble(m.Match));

        /// <summary>Parses a single letter character (Unicode category L).</summary>
        public static readonly Stringerex Letter = new Stringerex(char.IsLetter);
        /// <summary>Parses a single character that is a letter or digit (Unicode category L or Nd).</summary>
        public static readonly Stringerex LetterOrDigit = new Stringerex(char.IsLetterOrDigit);
        /// <summary>Parses a single character that belongs to Unicode category Pc. This includes the underscore.</summary>
        public static readonly Stringerex ConnectorPunctuation = new Stringerex(c => char.GetUnicodeCategory(c) == UnicodeCategory.ConnectorPunctuation);
        /// <summary>
        ///     Parses an identifier according to the rules of most programming languages (allowing the underscore but no
        ///     dollar sign).</summary>
        public static readonly Stringerex Identifier = Letter.Or(ConnectorPunctuation).Then(LetterOrDigit.Or(ConnectorPunctuation).RepeatGreedy());
        /// <summary>Parses an identifier without allowing the underscore.</summary>
        public static readonly Stringerex IdentifierNoPunctuation = Letter.Then(LetterOrDigit.RepeatGreedy());
        /// <summary>Parses a JavaScript identifier, which allows the dollar ($) character.</summary>
        public static readonly Stringerex JavaScriptIdentifier = Letter.Or(ConnectorPunctuation).Or('$').Then(LetterOrDigit.Or(ConnectorPunctuation).Or('$').RepeatGreedy());
    }
}
