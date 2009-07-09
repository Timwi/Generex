using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util.ExtensionMethods;
using System.Text.RegularExpressions;

namespace RT.Util.PowerfulRegex
{
    /// <summary>
    /// Provides regular-expression functionality for collections of arbitrary objects rather than just strings.
    /// Regular expression trees are generated using successive method calls instead of a (cryptic) string-based syntax.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    /// <remarks><para>To do:</para>
    /// <list type="bullet">
    /// <item><description>Positive/negative zero-width look-ahead/look-behind assertions</description></item>
    /// <item><description>Match backwards (reverse)</description></item>
    /// </list></remarks>
    public class PRegex<T>
    {
        private delegate IEnumerable<int> matcher(T[] input, int startIndex);
        private matcher _matcher;

        /// <summary>
        /// Instantiates an empty regular expression (always matches).
        /// </summary>
        public PRegex() { _matcher = emptyMatcher(); }
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public PRegex(params T[] elements) { _matcher = elementsMatcher(elements, EqualityComparer<T>.Default); }
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        public PRegex(IEqualityComparer<T> comparer, params T[] elements) { _matcher = elementsMatcher(elements, comparer); }
        /// <summary>
        /// Instantiates a regular expression that matches a single element that satisfies the given predicate (cf. "[...]" in standard regular expression syntax).
        /// </summary>
        public PRegex(Predicate<T> predicate) { _matcher = (input, startIndex) => startIndex >= input.Length || !predicate(input[startIndex]) ? new int[0] : new int[] { 1 }; }

        private PRegex(matcher matcher) { _matcher = matcher; }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression.
        /// </summary>
        public bool IsMatch(T[] input) { return Enumerable.Range(0, input.Length + 1).SelectMany(si => _matcher(input, si)).Any(); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match.
        /// </summary>
        /// <returns>A <see cref="PRegexMatch&lt;T&gt;"/> object describing a regular expression match in case of success; null if no match.</returns>
        public PRegexMatch<T> Match(T[] input)
        {
            for (int i = 0; i <= input.Length; i++)
            {
                try
                {
                    int matchLength = _matcher(input, i).First();
                    return new PRegexMatch<T>(input, i, matchLength);
                }
                catch (InvalidOperationException) { }
            }
            return null;
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches.
        /// </summary>
        /// <remarks>The behaviour is the same as <see cref="Regex.Matches(string,string)"/>.
        /// The documentation for that method lies when it claims that it returns "all occurrences of the regular expression".</remarks>
        public IEnumerable<PRegexMatch<T>> Matches(T[] input)
        {
            int i = 0;
            while (i <= input.Length)
            {
                int m;
                try { m = _matcher(input, i).First(); }
                catch (InvalidOperationException) { i++; continue; }
                yield return new PRegexMatch<T>(input, i, m);
                i += m > 0 ? m : 1;
            }
        }

        /// <summary>
        /// Generates a matcher that matches a sequence of specific elements either fully or not at all.
        /// </summary>
        private static matcher elementsMatcher(T[] elements, IEqualityComparer<T> comparer)
        {
            return (input, startIndex) =>
            {
                if (startIndex > input.Length - elements.Length)
                    return new int[0];
                for (int i = 0; i < elements.Length; i++)
                    if (!comparer.Equals(input[i + startIndex], elements[i]))
                        return new int[0];
                return new int[] { elements.Length };
            };
        }

        /// <summary>
        /// Generates a matcher that matches this regular expression followed by the specified sequence of other regular expressions.
        /// </summary>
        private matcher thenMatcher(IEnumerable<PRegex<T>> other)
        {
            return (input, startIndex) => other.Aggregate(_matcher(input, startIndex), (acc, otherPRegex) => acc.SelectMany(m => otherPRegex._matcher(input, startIndex + m).Select(m2 => m + m2)));
        }

        /// <summary>
        /// Returns a regular expression that matches a single element, no matter what it is (cf. "." in standard regular expression syntax).
        /// </summary>
        public static PRegex<T> Any { get { return new PRegex<T>((input, startIndex) => startIndex >= input.Length ? new int[0] : new int[] { 1 }); } }

        /// <summary>
        /// Returns a regular expression that matches a consecutive sequence of regular expressions, beginning with this one, followed by the specified ones.
        /// </summary>
        public PRegex<T> Then(params PRegex<T>[] other) { return new PRegex<T>(thenMatcher(other)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements.
        /// </summary>
        public PRegex<T> Then(params T[] elements) { return Then(new PRegex<T>(elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements, using the specified equality comparer.
        /// </summary>
        public PRegex<T> Then(IEqualityComparer<T> comparer, params T[] elements) { return Then(new PRegex<T>(comparer, elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by a single element that satisfies the specified predicate.
        /// </summary>
        public PRegex<T> Then(Predicate<T> predicate) { return Then(new PRegex<T>(predicate)); }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or any of the specified ones (cf. "|" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> Or(params PRegex<T>[] other) { return new PRegex<T>((input, startIndex) => _matcher(input, startIndex).Concat(other.SelectMany(pre => pre._matcher(input, startIndex)))); }
        /// <summary>
        /// Returns a regular expression that matches either this regular expression or any one of the specified elements (cf. "|" or "[...]" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> Or(params T[] elements) { return Or(new PRegex<T>((input, startIndex) => startIndex >= input.Length ? new int[0] : elements.Where(el => EqualityComparer<T>.Default.Equals(el, input[startIndex])).Take(1).Select(el => 1))); }
        /// <summary>
        /// Returns a regular expression that matches either this regular expression or any of the specified elements using the specified equality comparer (cf. "|" or "[...]" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> Or(IEqualityComparer<T> comparer, params T[] elements) { return Or(new PRegex<T>((input, startIndex) => startIndex >= input.Length ? new int[0] : elements.Where(el => comparer.Equals(el, input[startIndex])).Take(1).Select(el => 1))); }
        /// <summary>
        /// Returns a regular expression that matches either this regular expression or a single element that satisfies the specified predicate (cf. "|" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> Or(Predicate<T> predicate) { return Or(new PRegex<T>(predicate)); }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in standard regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors(params PRegex<T>[] other) { return new PRegex<T>((input, startIndex) => other.SelectMany(pre => pre._matcher(input, startIndex))); }
        /// <summary>
        /// Returns a regular expression that matches any one of the specified elements (cf. "|" or "[...]" in standard regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors(params T[] elements) { return Ors(EqualityComparer<T>.Default, elements); }
        /// <summary>
        /// Returns a regular expression that matches any one of the specified elements using the specified equality comparer (cf. "|" or "[...]" in standard regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors(IEqualityComparer<T> comparer, params T[] elements) { return new PRegex<T>((input, startIndex) => startIndex >= input.Length ? new int[0] : elements.Where(el => comparer.Equals(el, input[startIndex])).Take(1).Select(el => 1)); }

        /// <summary>
        /// Returns a regular expression that matches the beginning of the input collection (cf. "^" in standard regular expression syntax). Successful matches are always zero length.
        /// </summary>
        public static PRegex<T> Start { get { return new PRegex<T>((input, startIndex) => startIndex != 0 ? new int[0] : new int[] { 0 }); } }
        /// <summary>
        /// Returns a regular expression that matches the end of the input collection (cf. "$" in standard regular expression syntax). Successful matches are always zero length.
        /// </summary>
        public static PRegex<T> End { get { return new PRegex<T>((input, startIndex) => startIndex != input.Length ? new int[0] : new int[] { 0 }); } }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. More times are prioritised (cf. "*" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> GreedyStar() { return star(true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. Fewer times are prioritised (cf. "*?" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> NonGreedyStar() { return star(false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. More times are prioritised (cf. "{min,}" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> GreedyStar(int min) { return star(min, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. Fewer times are prioritised (cf. "{min,}?" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> NonGreedyStar(int min) { return star(min, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression one or more times. More times are prioritised (cf. "+" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> GreedyPlus() { return Then(star(true)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression one or more times. Fewer times are prioritised (cf. "+?" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> NonGreedyPlus() { return Then(star(false)); }

        /// <summary>
        /// Generates a matcher that matches this regular expression zero or more times.
        /// </summary>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private PRegex<T> star(bool greedy)
        {
            matcher newMatcher = null;
            if (greedy)
                newMatcher = (input, startIndex) => _matcher(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m).Select(m2 => m + m2)).Add(0);
            else
                newMatcher = (input, startIndex) => _matcher(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m).Select(m2 => m + m2)).Prepend(0);
            return new PRegex<T>(newMatcher);
        }
        /// <summary>
        /// Generates a matcher that matches this regular expression at least a minimum number of times.
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private PRegex<T> star(int min, bool greedy)
        {
            if (min < 0) throw new ArgumentException("'min' cannot be negative.", "min");
            return q(min, min, true).Then(star(greedy));
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times (cf. "{times}" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> Times(int times)
        {
            if (times < 0) throw new ArgumentException("'times' cannot be negative.", "times");
            return q(times, times, true);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf. "?" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> GreedyQ() { return q(0, 1, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Zero times is prioritised (cf. "??" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> NonGreedyQ() { return q(0, 1, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. More times are prioritised (cf. "{min,max}" in standard regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public PRegex<T> GreedyQ(int min, int max) { return q(min, max, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. Fewer times are prioritised (cf. "{min,max}?" in standard regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public PRegex<T> NonGreedyQ(int min, int max) { return q(min, max, false); }

        /// <summary>
        /// Generates a matcher that matches this regular expression at least a minimum number of times and at most a maximum number of times.
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private PRegex<T> q(int min, int max, bool greedy)
        {
            if (min < 0) throw new ArgumentException("'min' cannot be negative.", "min");
            if (max < min) throw new ArgumentException("'max' cannot be smaller than 'min'.", "max");
            qMatcher qm = new qMatcher { Greedy = greedy, MinTimes = min, MaxTimes = max, OrigMatcher = _matcher };
            return new PRegex<T>(qm.Matcher);
        }

        private class qMatcher
        {
            public int MinTimes;
            public int MaxTimes;
            public bool Greedy;
            public matcher OrigMatcher;
            public IEnumerable<int> Matcher(T[] input, int startIndex)
            {
                return matcher(input, startIndex, 0);
            }
            private IEnumerable<int> matcher(T[] input, int startIndex, int iteration)
            {
                if (!Greedy && iteration >= MinTimes)
                    yield return 0;
                if (iteration < MaxTimes)
                {
                    foreach (var m in OrigMatcher(input, startIndex))
                        foreach (var m2 in matcher(input, startIndex + m, iteration + 1))
                            yield return m + m2;
                }
                if (Greedy && iteration >= MinTimes)
                    yield return 0;
            }
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. (This always matches successfully and all matches are zero-length.)
        /// </summary>
        public PRegex<T> Do(Action code) { return new PRegex<T>((input, startIndex) => _matcher(input, startIndex).Select(m => { code(); return m; })); }
        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. (This always matches successfully and all matches are zero-length.)
        /// </summary>
        /// <remarks>You can use this to capture the match from a subexpression. See the example section.</remarks>
        /// <example>
        /// <code>
        /// string captured = null;
        /// PRegex&lt;char&gt; myRe = someRe.Then(someOtherRe.Do(m => captured = new string(m.Match.ToArray()))).Then(yetAnotherRe);
        /// foreach (var m in myRe.Matches(input))
        ///     Console.WriteLine("Captured text: {0}", captured);
        /// </code>
        /// </example>
        public PRegex<T> Do(Action<PRegexMatch<T>> code) { return new PRegex<T>((input, startIndex) => _matcher(input, startIndex).Select(m => { code(new PRegexMatch<T>(input, startIndex, m)); return m; })); }
        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. The return value of the specified code determines whether the expression matches successfully (all matches are zero-length).
        /// </summary>
        public PRegex<T> Do(Func<bool> code) { return new PRegex<T>((input, startIndex) => _matcher(input, startIndex).Where(m => code())); }
        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. The return value of the specified code determines whether the expression matches successfully (all matches are zero-length).
        /// </summary>
        public PRegex<T> Do(Func<PRegexMatch<T>, bool> code) { return new PRegex<T>((input, startIndex) => _matcher(input, startIndex).Where(m => code(new PRegexMatch<T>(input, startIndex, m)))); }

        /// <summary>Generates an always-successful zero-width matcher.</summary>
        private static matcher emptyMatcher() { return (input, startIndex) => new int[] { 0 }; }
    }

    /// <summary>
    /// Provides static factory methods to generate <see cref="PRegex&lt;T&gt;"/> objects.
    /// </summary>
    public static class PRegex
    {
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public static PRegex<T> New<T>(params T[] elements) { return new PRegex<T>(elements); }
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        public static PRegex<T> New<T>(IEqualityComparer<T> comparer, params T[] elements) { return new PRegex<T>(comparer, elements); }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in standard regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors<T>(params PRegex<T>[] other) { return PRegex<T>.Ors(other); }
        /// <summary>
        /// Returns a regular expression that matches any one of the specified elements (cf. "|" or "[...]" in standard regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors<T>(params T[] elements) { return PRegex<T>.Ors(EqualityComparer<T>.Default, elements); }
        /// <summary>
        /// Returns a regular expression that matches any one of the specified elements using the specified equality comparer (cf. "|" or "[...]" in standard regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors<T>(IEqualityComparer<T> comparer, params T[] elements) { return PRegex<T>.Ors(comparer, elements); }
    }

    /// <summary>
    /// Represents the result of a regular expression match using <see cref="PRegex&lt;T&gt;"/>.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    public class PRegexMatch<T>
    {
        private int _index;
        private int _length;
        private IEnumerable<T> _match;

        /// <summary>
        /// Gets the index in the original collection at which the match occurred.
        /// </summary>
        public int Index { get { return _index; } }
        /// <summary>
        /// Gets the length of the match.
        /// </summary>
        public int Length { get { return _length; } }
        /// <summary>
        /// Returns a slice of the original collection which the regular expression matched.
        /// </summary>
        public IEnumerable<T> Match { get { return _match; } }

        internal PRegexMatch(T[] original, int index, int length)
        {
            _index = index;
            _length = length;
            _match = original.Skip(index).Take(length);
        }
    }
}