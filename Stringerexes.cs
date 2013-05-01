using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RT.Generexes
{
    public static class Stringerexes
    {
        public static readonly Stringerex Digit = new Stringerex(char.IsDigit);
        public static readonly Stringerex Digit0to9 = new Stringerex(ch => ch >= '0' && ch <= '9');

        private static readonly Stringerex positiveInteger = Digit0to9.RepeatGreedy(1);
        private static readonly Stringerex optionalMinus = new Stringerex("-").OptionalGreedy();
        private static readonly Stringerex integer = optionalMinus.Then(positiveInteger);

        public static readonly Stringerex<long> PositiveInteger = positiveInteger.Process(m => Convert.ToInt64(m.Match));
        public static readonly Stringerex<long> Integer = integer.Process(m => Convert.ToInt64(m.Match));

        public static readonly Stringerex<double> Number = optionalMinus
            .Then(Digit0to9.RepeatGreedy())
            .Then(new Stringerex(".").Then(Digit0to9.RepeatGreedy()).OptionalGreedy())
            .Do(m => m.Length > 0)
            .Then(Stringerex.Ors("e", "E").Then(Stringerex.Ors("+", "-", "")).Then(Digit0to9.RepeatGreedy(1)).OptionalGreedy())
            .Process(m => Convert.ToDouble(m.Match));
    }
}
