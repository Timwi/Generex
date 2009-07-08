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
    /// <remarks><para>Things this cannot do yet:</para>
    /// <list type="bullet">
    /// <item><description>Positive/negative zero-width look-ahead/look-behind assertions</description></item>
    /// <item><description>Code embedded in a regular expression, to be executed when something matches</description></item>
    /// <item><description>Capturing groups</description></item>
    /// </list></remarks>
    public class PRegex<T>
    {
        private delegate IEnumerable<PRegexMatch<T>> matcher(T[] input, int startIndex);
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
        public PRegex(Predicate<T> predicate) { _matcher = (input, startIndex) => startIndex >= input.Length || !predicate(input[startIndex]) ? new PRegexMatch<T>[0] : new PRegexMatch<T>[] { new PRegexMatch<T>(input, startIndex, 1, null) }; }

        private PRegex(matcher matcher) { _matcher = matcher; }

        private static matcher elementsMatcher(T[] elements, IEqualityComparer<T> comparer)
        {
            return (input, startIndex) =>
            {
                if (startIndex > input.Length - elements.Length)
                    return new PRegexMatch<T>[0];
                for (int i = 0; i < elements.Length; i++)
                    if (!comparer.Equals(input[i + startIndex], elements[i]))
                        return new PRegexMatch<T>[0];
                return new PRegexMatch<T>[] { new PRegexMatch<T>(input, startIndex, elements.Length, null) };
            };
        }

        private IEnumerable<PRegexMatch<T>> getMatches(T[] input) { return Enumerable.Range(0, input.Length + 1).SelectMany(si => _matcher(input, si)); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression.
        /// </summary>
        public bool IsMatch(T[] input) { return getMatches(input).Any(); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match.
        /// </summary>
        /// <returns>A <see cref="PRegexMatch&lt;T&gt;"/> object describing a regular expression match in case of success; null if no match.</returns>
        public PRegexMatch<T> Match(T[] input) { return getMatches(input).FirstOrDefault(); }

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
                var m = _matcher(input, i).FirstOrDefault();
                if (m == null)
                    i++;
                else
                {
                    yield return m;
                    i += m.Length > 0 ? m.Length : 1;
                }
            }
        }

        /// <summary>
        /// Returns a regular expression that matches a single element, no matter what it is (cf. "." in standard regular expression syntax).
        /// </summary>
        public static PRegex<T> Any
        {
            get { return new PRegex<T>((input, startIndex) => startIndex >= input.Length ? new PRegexMatch<T>[0] : new PRegexMatch<T>[] { new PRegexMatch<T>(input, startIndex, 1, null) }); }
        }

        private matcher thenMatcher(IEnumerable<PRegex<T>> other)
        {
            return (input, startIndex) => other.Aggregate(_matcher(input, startIndex), (acc, pre) => acc.SelectMany(m => pre._matcher(input, startIndex + m.Length).Select(m2 => new PRegexMatch<T>(input, m.Index, m.Length + m2.Length, m.Groups, m2.Groups))));
        }

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
        /// Returns a regular expression that matches either this regular expression or any one of the specified elements (cf. "|" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> Or(params T[] elements) { return Or(new PRegex<T>((input, startIndex) => elements.Where(el => EqualityComparer<T>.Default.Equals(el, input[startIndex])).Take(1).Select(el => new PRegexMatch<T>(input, startIndex, 1, null)))); }
        /// <summary>
        /// Returns a regular expression that matches either this regular expression or any of the specified elements using the specified equality comparer (cf. "|" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> Or(IEqualityComparer<T> comparer, params T[] elements) { return Or(new PRegex<T>((input, startIndex) => elements.Where(el => comparer.Equals(el, input[startIndex])).Take(1).Select(el => new PRegexMatch<T>(input, startIndex, 1, null)))); }
        /// <summary>
        /// Returns a regular expression that matches either this regular expression or a single element that satisfies the specified predicate (cf. "|" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> Or(Predicate<T> predicate) { return Or(new PRegex<T>(predicate)); }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in standard regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors(params PRegex<T>[] other) { return new PRegex<T>((input, startIndex) => other.Select(pre => pre._matcher).SelectMany(m => m(input, startIndex))); }
        /// <summary>
        /// Returns a regular expression that matches any one of the specified elements (cf. "|" in standard regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors(params T[] elements) { return new PRegex<T>((input, startIndex) => elements.Where(el => EqualityComparer<T>.Default.Equals(el, input[startIndex])).Take(1).Select(el => new PRegexMatch<T>(input, startIndex, 1, null))); }
        /// <summary>
        /// Returns a regular expression that matches any one of the specified elements using the specified equality comparer (cf. "|" in standard regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors(IEqualityComparer<T> comparer, params T[] elements) { return new PRegex<T>((input, startIndex) => elements.Where(el => comparer.Equals(el, input[startIndex])).Take(1).Select(el => new PRegexMatch<T>(input, startIndex, 1, null))); }

        /// <summary>
        /// Returns a regular expression that matches the beginning of the input collection (cf. "^" in standard regular expression syntax). Successful matches are always zero length.
        /// </summary>
        public static PRegex<T> Start { get { return new PRegex<T>((input, startIndex) => startIndex != 0 ? new PRegexMatch<T>[0] : new PRegexMatch<T>[] { new PRegexMatch<T>(input, 0, 0, null) }); } }
        /// <summary>
        /// Returns a regular expression that matches the end of the input collection (cf. "$" in standard regular expression syntax). Successful matches are always zero length.
        /// </summary>
        public static PRegex<T> End { get { return new PRegex<T>((input, startIndex) => startIndex != input.Length ? new PRegexMatch<T>[0] : new PRegexMatch<T>[] { new PRegexMatch<T>(input, startIndex, 0, null) }); } }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. More times are prioritised (cf. "*" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> GreedyStar() { return star(true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. Fewer times are prioritised (cf. "*?" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> NonGreedyStar() { return star(false); }

        private PRegex<T> star(bool greedy)
        {
            matcher newMatcher = null;
            newMatcher = new matcher((input, startIndex) => addOrPrependEmptyMatch(
                _matcher(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m.Length).Select(m2 => new PRegexMatch<T>(input, startIndex, m2.Index + m2.Length - startIndex, m.Groups, m2.Groups))), greedy, input, startIndex
            ));
            return new PRegex<T>(newMatcher);
        }

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
        private PRegex<T> star(int min, bool greedy)
        {
            if (min < 1)
                return star(greedy);
            if (min == 1)
                return Then(star(greedy));
            return Then(this.Repeat(min - 1).ToArray()).Then(star(greedy));
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
        /// Returns a regular expression that matches this regular expression the specified number of times (cf. "{times}" in standard regular expression syntax).
        /// </summary>
        public PRegex<T> Times(int times) { return q(times, times, true); }
        private PRegex<T> q(int min, int max, bool greedy)
        {
            if (min < 0) throw new ArgumentException("'min' cannot be negative.", "min");
            if (max < min) throw new ArgumentException("'max' cannot be smaller than 'min'.", "max");
            return new PRegex<T>(Enumerable.Range(min, max - min)
                .Aggregate((min == 0) ? emptyMatcher() : (min == 1) ? _matcher : thenMatcher(this.Repeat(min - 1)),
                            (prevMatcher, dummy) => (input, startIndex) => prevMatcher(input, startIndex)
                                .SelectMany(m2 => addOrPrependEmptyMatch(_matcher(input, m2.Index + m2.Length), greedy, input, startIndex))
                                .Select(m2 => new PRegexMatch<T>(input, startIndex, m2.Index + m2.Length - startIndex, m2.Groups))));
        }

        /// <summary>
        /// Returns a regular expression which captures the subsequence matched by this regular expression.
        /// </summary>
        /// <param name="name">Name of the capturing group. (Two groups with the same name within the same regular expression will overwrite each other.)</param>
        public PRegex<T> Capture(string name)
        {
            return new PRegex<T>((input, startIndex) => _matcher(input, startIndex).Select(m =>
            {
                IDictionary<string, T[]> groups = m.Groups;
                groups[name] = m.Match.ToArray();
                return new PRegexMatch<T>(input, m.Index, m.Length, groups);
            }));
        }

        private IEnumerable<PRegexMatch<T>> addOrPrependEmptyMatch(IEnumerable<PRegexMatch<T>> orig, bool add, T[] input, int startIndex)
        {
            return add
                ? orig.Add(new PRegexMatch<T>(input, startIndex, 0, null))
                : orig.Prepend(new PRegexMatch<T>(input, startIndex, 0, null));
        }

        private static matcher emptyMatcher() { return (input, startIndex) => new PRegexMatch<T>[] { new PRegexMatch<T>(input, startIndex, 0, null) }; }
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
        private IDictionary<string, T[]> _groups;

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
        /// <summary>
        /// Returns a dictionary containing the matches for each captured group (see <see cref="PRegex&lt;T&gt;.Capture(string)"/>).
        /// </summary>
        public IDictionary<string, T[]> Groups { get { return _groups ?? new Dictionary<string, T[]>(); } }

        internal PRegexMatch(T[] original, int index, int length, IDictionary<string, T[]> groups)
        {
            _index = index;
            _length = length;
            _match = original.Skip(index).Take(length);
            _groups = groups;
        }

        internal PRegexMatch(T[] original, int index, int length, IDictionary<string, T[]> groups1, IDictionary<string, T[]> groups2)
            : this(original, index, length, null)
        {
            _groups = groups1.CopyMerge(groups2);
        }
    }
}