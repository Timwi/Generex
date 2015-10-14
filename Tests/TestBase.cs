using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RT.Generexes.Tests
{
    class TestBase
    {
        protected static object[] _expectException = new object[] { new ExpectedException() };
        protected static object[] _expectInvOp = new object[] { new InvalidOperationException() };

        protected static int? _expectException2 = null;
        protected static int? _expectInvOp2 = -1;

        protected static Expectation True = Expectation.True;
        protected static Expectation False = Expectation.False;
        protected static Expectation Exception = Expectation.ExpectedException;
        protected static Expectation InvOp = Expectation.InvalidOperationException;

        public static void AssertMatches<TGenerex, TGenerexMatch>(GenerexNoResultBase<char, TGenerex, TGenerexMatch> generex, string input, Expectation isMatch, Expectation isMatchAt1, Expectation isMatchUpTo1, Expectation isMatchExact, Expectation isMatchReverse, object[] match, object[] matchExact, object[] matchReverse, int? matches, int? matchesReverse)
            where TGenerex : GenerexNoResultBase<char, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<char>
        {
            AssertMatches(generex, input.ToCharArray(), isMatch, isMatchAt1, isMatchUpTo1, isMatchExact, isMatchReverse, match, matchExact, matchReverse, matches, matchesReverse);
        }

        public static void AssertMatches<TGenerex, TResult, TGenerexMatch>(GenerexWithResultBase<char, TResult, TGenerex, TGenerexMatch> generex, string input, Expectation isMatch, Expectation isMatchAt1, Expectation isMatchUpTo1, Expectation isMatchExact, Expectation isMatchReverse, object[] match, object[] matchExact, object[] matchReverse, int? matches, int? matchesReverse)
            where TGenerex : GenerexWithResultBase<char, TResult, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<char, TResult>
        {
            AssertMatches(generex, input.ToCharArray(), isMatch, isMatchAt1, isMatchUpTo1, isMatchExact, isMatchReverse, match, matchExact, matchReverse, matches, matchesReverse);
        }

        public static void AssertMatches<T, TGenerex, TGenerexMatch>(GenerexNoResultBase<T, TGenerex, TGenerexMatch> generex, T[] input, Expectation isMatch, Expectation isMatchAt1, Expectation isMatchUpTo1, Expectation isMatchExact, Expectation isMatchReverse, object[] match, object[] matchExact, object[] matchReverse, int? matches, int? matchesReverse)
            where TGenerex : GenerexNoResultBase<T, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T>
        {
            assertMatchesBase(generex, input, isMatch, isMatchAt1, isMatchUpTo1, isMatchExact, isMatchReverse, match, matchExact, matchReverse, matches, matchesReverse);

            // Test 1: generex.Method(input)
            assertMatch<T, TGenerexMatch>(match, () => generex.Match(input));
            assertMatch<T, TGenerexMatch>(matchExact, () => generex.MatchExact(input));
            assertMatch<T, TGenerexMatch>(matchReverse, () => generex.MatchReverse(input));

            // Test 2: input.Method(generex)
            assertMatch<T, TGenerexMatch>(match, () => input.Match(generex));
            assertMatch<T, TGenerexMatch>(matchExact, () => input.MatchExact(generex));
            assertMatch<T, TGenerexMatch>(matchReverse, () => input.MatchReverse(generex));
        }

        public static void AssertMatches<T, TResult, TGenerex, TGenerexMatch>(GenerexWithResultBase<T, TResult, TGenerex, TGenerexMatch> generex, T[] input, Expectation isMatch, Expectation isMatchAt1, Expectation isMatchUpTo1, Expectation isMatchExact, Expectation isMatchReverse, object[] match, object[] matchExact, object[] matchReverse, int? matches, int? matchesReverse)
            where TGenerex : GenerexWithResultBase<T, TResult, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T, TResult>
        {
            assertMatchesBase(generex, input, isMatch, isMatchAt1, isMatchUpTo1, isMatchExact, isMatchReverse, match, matchExact, matchReverse, matches, matchesReverse);

            assertMatch<T, TResult, TGenerexMatch>(match, () => generex.Match(input), () => generex.RawMatch(input));
            assertMatch<T, TResult, TGenerexMatch>(matchExact, () => generex.MatchExact(input), () => generex.RawMatchExact(input));
            assertMatch<T, TResult, TGenerexMatch>(matchReverse, () => generex.MatchReverse(input), () => generex.RawMatchReverse(input));
        }

        static void assertMatchesBase<T, TMatch, TGenerex, TGenerexMatch>(GenerexBase<T, TMatch, TGenerex, TGenerexMatch> generex, T[] input, Expectation isMatch, Expectation isMatchAt1, Expectation isMatchUpTo1, Expectation isMatchExact, Expectation isMatchReverse, object[] match, object[] matchExact, object[] matchReverse, int? matches, int? matchesReverse)
            where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T>
        {
            // Test 1: generex.Method(input)
            assertExpectation(isMatch, () => generex.IsMatch(input));
            assertExpectation(isMatchAt1, () => generex.IsMatchAt(input, 1));
            assertExpectation(isMatchUpTo1, () => generex.IsMatchUpTo(input, input.Length - 1));
            assertExpectation(isMatchExact, () => generex.IsMatchExact(input));
            assertExpectation(isMatchReverse, () => generex.IsMatchReverse(input));
            assertIntOrThrow(matches, () => generex.Matches(input).Count());
            assertIntOrThrow(matchesReverse, () => generex.MatchesReverse(input).Count());

            // Test 2: input.Method(generex)
            assertExpectation(isMatch, () => input.IsMatch(generex));
            assertExpectation(isMatchAt1, () => input.IsMatchAt(generex, 1));
            assertExpectation(isMatchUpTo1, () => input.IsMatchUpTo(generex, input.Length - 1));
            assertExpectation(isMatchExact, () => input.IsMatchExact(generex));
            assertExpectation(isMatchReverse, () => input.IsMatchReverse(generex));
            assertIntOrThrow(matches, () => input.Matches(generex).Count());
            assertIntOrThrow(matchesReverse, () => input.MatchesReverse(generex).Count());
        }

        static void assertIntOrThrow(int? expected, Func<int> getActual)
        {
            if (expected == null)
                Assert.Throws<ExpectedException>(() => { getActual(); });
            else if (expected == -1)
                Assert.Throws<InvalidOperationException>(() => { getActual(); });
            else
                Assert.AreEqual(expected.Value, getActual());
        }

        static void assertMatch<T, TResult, TGenerexMatch>(object[] expected, Func<TGenerexMatch> getActual, Func<TResult> getActualRaw)
            where TGenerexMatch : GenerexMatch<T, TResult>
        {
            Assert.IsTrue(expected == null || expected.Length == 1 || expected.Length == 3);
            assertMatchBase<T, TGenerexMatch>(expected, getActual);

            if (expected != null && expected.Length > 1)
            {
                var result = getActual().Result;
                if (expected[2] is IEnumerable)
                {
                    Assert.IsTrue(result is IEnumerable);
                    Assert.IsTrue(((IEnumerable) expected[2]).Cast<object>().SequenceEqual(((IEnumerable) result).Cast<object>()));
                    Assert.IsTrue(((IEnumerable) expected[2]).Cast<object>().SequenceEqual(((IEnumerable) getActualRaw()).Cast<object>()));
                }
                else
                {
                    Assert.IsFalse(result is IEnumerable);
                    Assert.AreEqual(expected[2], result);
                    Assert.AreEqual(expected[2], getActualRaw());
                }
            }
            else if (expected == null)
            {
                Assert.AreEqual(default(TResult), getActualRaw());
            }
        }

        static void assertMatch<T, TGenerexMatch>(object[] expected, Func<TGenerexMatch> getActual)
            where TGenerexMatch : GenerexMatch<T>
        {
            Assert.IsTrue(expected == null || expected.Length == 1 || expected.Length == 2);
            assertMatchBase<T, TGenerexMatch>(expected, getActual);
        }

        static void assertMatchBase<T, TGenerexMatch>(object[] expected, Func<TGenerexMatch> getActual)
            where TGenerexMatch : GenerexMatch<T>
        {
            if (expected != null && expected.Length == 1)
            {
                // Expect exception
                Assert.Throws(expected[0].GetType(), () => { getActual(); });
                return;
            }

            var actual = getActual();
            Assert.AreEqual(expected == null, actual == null);
            if (expected != null)
            {
                Assert.AreEqual(expected[0], actual.Index);
                Assert.AreEqual(expected[1], actual.Length);
            }
        }

        static void assertExpectation(Expectation expectation, Func<bool> action)
        {
            switch (expectation)
            {
                case Expectation.True: Assert.IsTrue(action()); break;
                case Expectation.False: Assert.IsFalse(action()); break;
                case Expectation.ExpectedException: Assert.Throws<ExpectedException>(() => { action(); }); break;
                case Expectation.InvalidOperationException: Assert.Throws<InvalidOperationException>(() => { action(); }); break;
            }
        }
    }
}
