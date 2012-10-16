using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace RT.Generexes
{
    /// <summary>Abstract base class for <see cref="Generex{T}"/> and <see cref="Generex{T,TResult}"/>.</summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    /// <typeparam name="TMatch">Either int or <see cref="GenerexMatchInfo{TResult}"/>.</typeparam>
    /// <typeparam name="TGenerex">The derived type. (Pass the type itself recursive.)</typeparam>
    /// <typeparam name="TGenerexMatch">Type describing a match of a regular expression.</typeparam>
    public abstract class GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
        where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
        where TGenerexMatch : GenerexMatch<T>
    {
        internal delegate IEnumerable<TMatch> matcher(T[] input, int startIndex);
        internal matcher _forwardMatcher, _backwardMatcher;

        internal GenerexBase(matcher forward, matcher backward)
        {
            _forwardMatcher = forward;
            _backwardMatcher = backward;
        }

        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression, optionally starting the search at a specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        public bool IsMatch(T[] input, int startAt = 0)
        {
            return Enumerable.Range(startAt, input.Length - startAt + 1).SelectMany(startIndex => _forwardMatcher(input, startIndex)).Any();
        }

        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression, optionally starting the search at a specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        public bool IsMatch(IEnumerable<T> input, int startAt = 0)
        {
            return IsMatch(input.ToArray(), startAt);
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression at a specific index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <returns>True if a match starting at the specified index exists (which need not run all the way to the end of the sequence); otherwise, false.</returns>
        public bool IsMatchAt(T[] input, int mustStartAt = 0)
        {
            return _forwardMatcher(input, mustStartAt).Any();
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression up to a specific index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>True if a match ending at the specified index exists (which need not begin at the start of the sequence); otherwise, false.</returns>
        public bool IsMatchUpTo(T[] input, int? mustEndAt = null)
        {
            return _backwardMatcher(input, mustEndAt ?? input.Length).Any();
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression exactly.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>True if a match starting and ending at the specified indexes exists; otherwise, false.</returns>
        public bool IsMatchExact(T[] input, int mustStartAt = 0, int? mustEndAt = null)
        {
            var mustEndAtReally = mustEndAt ?? input.Length;
            if (mustEndAtReally < input.Length)
            {
                input = input.Subarray(mustStartAt, mustEndAtReally - mustStartAt);
                mustEndAtReally -= mustStartAt;
                mustStartAt = 0;
            }
            var mustHaveLength = mustEndAtReally - mustStartAt;
            return _forwardMatcher(input, mustStartAt).Any(m => getLength(m) == mustHaveLength);
        }

        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression that ends before the specified maximum index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index before which a match must end. The search begins by matching from this index backwards, and then proceeds towards the start of the input sequence.</param>
        public bool IsMatchReverse(T[] input, int? endAt = null)
        {
            var endAtIndex = endAt ?? input.Length;
            return Enumerable.Range(0, endAtIndex + 1).SelectMany(offset => _backwardMatcher(input, endAtIndex - offset)).Any();
        }

        internal IEnumerable<TResult> matches<TResult>(T[] input, int startAt, Func<int, TMatch, TResult> selector, bool backward)
        {
            int i = startAt;
            int step = backward ? -1 : 1;
            while (backward ? (i >= 0) : (i <= input.Length))
            {
                TMatch firstMatch;
                using (var enumerator = (backward ? _backwardMatcher : _forwardMatcher)(input, i).GetEnumerator())
                {
                    if (!enumerator.MoveNext())
                    {
                        i += step;
                        continue;
                    }
                    firstMatch = enumerator.Current;
                }
                yield return selector(i, firstMatch);
                var matchLength = getLength(firstMatch);
                i += matchLength == 0 ? step : matchLength;
            }
        }

        internal TResult matchExact<TResult>(T[] input, int mustStartAt, int mustEndAt, Func<TMatch, TResult> selector)
        {
            if (mustEndAt < input.Length)
            {
                input = input.Subarray(mustStartAt, mustEndAt - mustStartAt);
                mustEndAt -= mustStartAt;
                mustStartAt = 0;
            }
            var mustHaveLength = mustEndAt - mustStartAt;
            return _forwardMatcher(input, mustStartAt).Where(m => getLength(m) == mustHaveLength).Select(selector).FirstOrDefault();
        }

        internal static matcher or(matcher one, matcher two)
        {
            return (input, startIndex) => one(input, startIndex).Concat(new[] { 0 }.SelectMany(x => two(input, startIndex)));
        }

        internal static matcher oneOf(matcher one, matcher two)
        {
            return (input, startIndex) => oneOfIterator(one, two, input, startIndex);
        }

        private static IEnumerable<TMatch> oneOfIterator(matcher one, matcher two, T[] input, int startIndex)
        {
            bool any = false;
            foreach (var match in one(input, startIndex))
            {
                yield return match;
                any = true;
            }
            if (!any)
                foreach (var match in two(input, startIndex))
                    yield return match;
        }

        /// <summary>
        /// Generates a matcher that matches the <paramref name="first"/> regular expression followed by the <paramref name="second"/> regular expression
        /// while retaining the result object of the first one.
        /// </summary>
        internal matcher then(matcher first, Generex<T>.matcher second)
        {
            return (input, startIndex) => first(input, startIndex).SelectMany(m => second(input, startIndex + getLength(m)).Select(m2 => add(m, m2)));
        }

        /// <summary>
        /// Generates a matcher that matches the <paramref name="first"/> regular expression followed by the <paramref name="second"/> regular expression
        /// while retaining the result object of the second one.
        /// </summary>
        internal matcher then(Generex<T>.matcher first, matcher second)
        {
            return (input, startIndex) => first(input, startIndex).SelectMany(m => second(input, startIndex + m).Select(m2 => add(m2, m)));
        }

        internal abstract int getLength(TMatch match);
        internal abstract TMatch add(TMatch match, int extra);
        internal abstract TGenerex create(matcher forward, matcher backward);

        /// <summary>Matches this regular expression atomically (without backtracking into it) (cf. "(?>...)" in traditional regular expression syntax).</summary>
        public TGenerex Atomic()
        {
            return create(
                (input, startIndex) => _forwardMatcher(input, startIndex).Take(1),
                (input, startIndex) => _backwardMatcher(input, startIndex).Take(1)
            );
        }
    }

    /// <summary>
    /// Provides regular-expression functionality for collections of arbitrary objects.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    public sealed class Generex<T> : GenerexBase<T, int, Generex<T>, GenerexMatch<T>>
    {
        internal override Generex<T> create(matcher forward, matcher backward) { return new Generex<T>(forward, backward); }
        internal override int getLength(int match) { return match; }
        internal override int add(int match, int extra) { return match + extra; }

        /// <summary>
        /// Instantiates an empty regular expression (always matches).
        /// </summary>
        public Generex() : base(emptyMatch, emptyMatch) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public Generex(params T[] elements)
            : this(EqualityComparer<T>.Default, elements) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public Generex(IEnumerable<T> elements)
            : this(EqualityComparer<T>.Default, elements.ToArray()) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        public Generex(IEqualityComparer<T> comparer, params T[] elements)
            : base(
                elementsMatcher(elements, comparer, backward: false),
                elementsMatcher(elements, comparer, backward: true)) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public Generex(IEqualityComparer<T> comparer, IEnumerable<T> elements)
            : this(comparer, elements.ToArray()) { }

        /// <summary>
        /// Instantiates a regular expression that matches a single element that satisfies the given predicate (cf. "[...]" in traditional regular expression syntax).
        /// </summary>
        public Generex(Predicate<T> predicate)
            : base(
                (input, startIndex) => startIndex >= input.Length || !predicate(input[startIndex]) ? Generex.NoMatch : Generex.OneElementMatch,
                (input, startIndex) => startIndex <= 0 || !predicate(input[startIndex - 1]) ? Generex.NoMatch : Generex.NegativeOneElementMatch) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive regular expressions.
        /// </summary>
        public Generex(params Generex<T>[] generexes)
            : base(
                generexes.Length == 0 ? emptyMatch : generexes.Select(p => p._forwardMatcher).Aggregate(thenSimple),
                generexes.Length == 0 ? emptyMatch : generexes.Reverse().Select(p => p._backwardMatcher).Aggregate(thenSimple)) { }

        private Generex(matcher forward, matcher backward) : base(forward, backward) { }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>A <see cref="GenerexMatch{T}"/> object describing a regular expression match in case of success; null if no match.</returns>
        public GenerexMatch<T> Match(T[] input, int startAt = 0)
        {
            return Matches(input, startAt).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression exactly, and if so, returns information about the match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>A <see cref="GenerexMatch{T}"/> object describing the regular expression match in case of success; null if no match.</returns>
        public GenerexMatch<T> MatchExact(T[] input, int mustStartAt = 0, int? mustEndAt = null)
        {
            return matchExact(input, mustStartAt, mustEndAt ?? input.Length, m => new GenerexMatch<T>(input, mustStartAt, m));
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match
        /// found by matching the regular expression backwards (starting from the end of the input sequence).
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>A <see cref="GenerexMatch{T}"/> object describing a regular expression match in case of success; null if no match.</returns>
        public GenerexMatch<T> MatchReverse(T[] input, int? endAt = null)
        {
            return MatchesReverse(input, endAt).FirstOrDefault();
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>The behaviour is analogous to <see cref="Regex.Matches(string,string)"/>.
        /// The documentation for that method claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public IEnumerable<GenerexMatch<T>> Matches(T[] input, int startAt = 0)
        {
            return matches(input, startAt, (index, length) => new GenerexMatch<T>(input, index, length), backward: false);
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the specified
        /// input sequence), optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        public IEnumerable<GenerexMatch<T>> MatchesReverse(T[] input, int? endAt = null)
        {
            return matches(input, endAt ?? input.Length, (index, negativeLength) => new GenerexMatch<T>(input, index + negativeLength, -negativeLength), backward: true);
        }

        /// <summary>
        /// Generates a matcher that matches a sequence of specific elements either fully or not at all.
        /// </summary>
        private static matcher elementsMatcher(T[] elements, IEqualityComparer<T> comparer, bool backward)
        {
            if (elements.Length == 0)
                return emptyMatch;

            if (elements.Length == 1)
            {
                var element = elements[0];
                if (backward)
                    return (input, startIndex) => startIndex > 0 && comparer.Equals(input[startIndex - 1], element) ? Generex.NegativeOneElementMatch : Generex.NoMatch;
                else
                    return (input, startIndex) => startIndex < input.Length && comparer.Equals(input[startIndex], element) ? Generex.OneElementMatch : Generex.NoMatch;
            }

            if (backward)
                return (input, startIndex) => startIndex >= elements.Length && input.SubarrayEquals(startIndex - elements.Length, elements, comparer) ? new int[] { -elements.Length } : Generex.NoMatch;
            else
                return (input, startIndex) => startIndex <= input.Length - elements.Length && input.SubarrayEquals(startIndex, elements, comparer) ? new int[] { elements.Length } : Generex.NoMatch;
        }

        /// <summary>
        /// Generates a matcher that matches the <paramref name="first"/> regular expression followed by the <paramref name="second"/> regular expression.
        /// </summary>
        private static matcher thenSimple(matcher first, matcher second)
        {
            return (input, startIndex) => first(input, startIndex).SelectMany(m => second(input, startIndex + m).Select(m2 => m + m2));
        }

        /// <summary>
        /// Returns a regular expression that matches a single element, no matter what it is (cf. "." in traditional regular expression syntax).
        /// </summary>
        public static Generex<T> Any
        {
            get
            {
                if (_anyCache == null)
                    _anyCache = new Generex<T>(
                        (input, startIndex) => startIndex >= input.Length ? Generex.NoMatch : Generex.OneElementMatch,
                        (input, startIndex) => startIndex <= 0 ? Generex.NoMatch : Generex.NegativeOneElementMatch
                    );
                return _anyCache;
            }
        }
        private static Generex<T> _anyCache;

        /// <summary>
        /// Returns a regular expression that always matches and returns a zero-width match.
        /// </summary>
        public static Generex<T> Empty
        {
            get
            {
                if (_emptyCache == null)
                {
                    matcher zeroWidthMatch = (input, startIndex) => Generex.ZeroWidthMatch;
                    _emptyCache = new Generex<T>(zeroWidthMatch, zeroWidthMatch);
                }
                return _emptyCache;
            }
        }
        private static Generex<T> _emptyCache;

        /// <summary>
        /// Returns a regular expression that matches a consecutive sequence of regular expressions, beginning with this one, followed by the specified ones.
        /// </summary>
        public Generex<T> Then(params Generex<T>[] other)
        {
            return new Generex<T>(
                _forwardMatcher.Concat(other.Select(o => o._forwardMatcher)).Aggregate(thenSimple),
                other.Reverse().Select(o => o._backwardMatcher).Concat(_backwardMatcher).Aggregate(thenSimple)
            );
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expressions, followed by the specified other,
        /// and retains the match object generated by each match of the other regular expression.
        /// </summary>
        public Generex<T, TResult> Then<TResult>(Generex<T, TResult> other)
        {
            return new Generex<T, TResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m => other._forwardMatcher(input, startIndex + m).Select(m2 => m2.Add(m))),
                (input, startIndex) => other._backwardMatcher(input, startIndex).SelectMany(m2 => _backwardMatcher(input, startIndex + m2.Length).Select(m => m2.Add(m)))
            );
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements.
        /// </summary>
        public Generex<T> Then(params T[] elements) { return Then(new Generex<T>(elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements.
        /// </summary>
        public Generex<T> Then(IEnumerable<T> elements) { return Then(new Generex<T>(elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements, using the specified equality comparer.
        /// </summary>
        public Generex<T> Then(IEqualityComparer<T> comparer, params T[] elements) { return Then(new Generex<T>(comparer, elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements, using the specified equality comparer.
        /// </summary>
        public Generex<T> Then(IEqualityComparer<T> comparer, IEnumerable<T> elements) { return Then(new Generex<T>(comparer, elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by a single element that satisfies the specified predicate.
        /// </summary>
        public Generex<T> Then(Predicate<T> predicate) { return Then(new Generex<T>(predicate)); }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified sequence of regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        /// <example>
        /// <para>The following code:</para>
        /// <code>var regex = regex1.Or(regex2, regex3);</code>
        /// <para>generates a regular expression equivalent to <c>1|23</c>, NOT <c>1|2|3</c>.</para>
        /// </example>
        public Generex<T> Or(params Generex<T>[] other)
        {
            if (other.Length == 0)
                return this;
            if (other.Length == 1)
                return Or(other[0]);
            return Or(new Generex<T>(other));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified other regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public Generex<T> Or(Generex<T> other)
        {
            return new Generex<T>(or(_forwardMatcher, other._forwardMatcher), or(_backwardMatcher, other._backwardMatcher));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified sequence of elements (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        /// <example>
        /// <para>The following code:</para>
        /// <code>var regex = new Generex&lt;char&gt;('a', 'b').Or('c', 'd');</code>
        /// <para>is equivalent to the regular expression <c>ab|cd</c>, NOT <c>ab|c|d</c>.</para>
        /// </example>
        /// <seealso cref="Or(IEqualityComparer{Generex},Generex[])"/>
        public Generex<T> Or(params T[] elements)
        {
            if (elements.Length == 0)
                return this;
            return Or(new Generex<T>(elements));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or any of the specified elements using the specified equality comparer (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        /// <seealso cref="Or(T[])"/>
        public Generex<T> Or(IEqualityComparer<T> comparer, params T[] elements)
        {
            if (elements.Length == 0)
                return this;
            return Or(new Generex<T>(comparer, elements));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified sequence of elements (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        public Generex<T> Or(IEnumerable<T> elements) { return Or(elements.ToArray()); }
        /// <summary>
        /// Returns a regular expression that matches either this regular expression or any of the specified elements using the specified equality comparer (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        public Generex<T> Or(IEqualityComparer<T> comparer, IEnumerable<T> elements) { return Or(comparer, elements.ToArray()); }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or a single element that satisfies the specified predicate (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public Generex<T> Or(Predicate<T> predicate)
        {
            return Or(new Generex<T>(predicate));
        }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static Generex<T> Ors(params Generex<T>[] other)
        {
            return new Generex<T>(
                other.Select(p => p._forwardMatcher).Aggregate(or),
                other.Select(p => p._backwardMatcher).Aggregate(or)
            );
        }

        /// <summary>
        /// Returns a regular expression that matches exactly one of the specified regular expressions.
        /// (This means that as soon as one matches, then the remaining ones are not matched in backtracking.)
        /// </summary>
        public static Generex<T> OneOf(params Generex<T>[] other)
        {
            return new Generex<T>(
                other.Select(p => p._forwardMatcher).Aggregate(oneOf),
                other.Select(p => p._backwardMatcher).Aggregate(oneOf)
            );
        }

        /// <summary>
        /// Returns a regular expression that matches the beginning of the input collection (cf. "^" in traditional regular expression syntax). Successful matches are always zero length.
        /// </summary>
        public static Generex<T> Start
        {
            get
            {
                if (_startCache == null)
                {
                    matcher matcher = (input, startIndex) => startIndex != 0 ? Generex.NoMatch : Generex.ZeroWidthMatch;
                    _startCache = new Generex<T>(matcher, matcher);
                }
                return _startCache;
            }
        }
        private static Generex<T> _startCache;

        /// <summary>
        /// Returns a regular expression that matches the end of the input collection (cf. "$" in traditional regular expression syntax). Successful matches are always zero length.
        /// </summary>
        public static Generex<T> End
        {
            get
            {
                if (_endCache == null)
                {
                    matcher matcher = (input, startIndex) => startIndex != input.Length ? Generex.NoMatch : Generex.ZeroWidthMatch;
                    _endCache = new Generex<T>(matcher, matcher);
                }
                return _endCache;
            }
        }
        private static Generex<T> _endCache;

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf. "?" in traditional regular expression syntax).
        /// </summary>
        public Generex<T> OptionalGreedy() { return repeatBetween(0, 1, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Zero times is prioritised (cf. "??" in traditional regular expression syntax).
        /// </summary>
        public Generex<T> Optional() { return repeatBetween(0, 1, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. More times are prioritised (cf. "*" in traditional regular expression syntax).
        /// </summary>
        public Generex<T> RepeatGreedy() { return new Generex<T>(repeatInfinite(_forwardMatcher, true), repeatInfinite(_backwardMatcher, true)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. Fewer times are prioritised (cf. "*?" in traditional regular expression syntax).
        /// </summary>
        public Generex<T> Repeat() { return new Generex<T>(repeatInfinite(_forwardMatcher, false), repeatInfinite(_backwardMatcher, false)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. More times are prioritised (cf. "{min,}" in traditional regular expression syntax).
        /// </summary>
        public Generex<T> RepeatGreedy(int min) { return repeatMin(min, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. Fewer times are prioritised (cf. "{min,}?" in traditional regular expression syntax).
        /// </summary>
        public Generex<T> Repeat(int min) { return repeatMin(min, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. More times are prioritised (cf. "{min,max}" in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public Generex<T> RepeatGreedy(int min, int max) { return repeatBetween(min, max, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. Fewer times are prioritised (cf. "{min,max}?" in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public Generex<T> Repeat(int min, int max) { return repeatBetween(min, max, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times (cf. "{times}" in traditional regular expression syntax).
        /// </summary>
        public Generex<T> Times(int times)
        {
            if (times < 0) throw new ArgumentException("'times' cannot be negative.", "times");
            return repeatBetween(times, times, true);
        }
        /// <summary>
        /// Returns a regular expression that matches this regular expression one or more times, interspersed with a separator. Fewer times are prioritised.
        /// </summary>
        public Generex<T> RepeatWithSeparator(Generex<T> separator) { return Then(separator.Then(this).Repeat()); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression one or more times, interspersed with a separator. More times are prioritised.
        /// </summary>
        public Generex<T> RepeatWithSeparatorGreedy(Generex<T> separator) { return Then(separator.Then(this).RepeatGreedy()); }


        /// <summary>
        /// Generates a matcher that matches the specified matcher zero or more times.
        /// </summary>
        /// <param name="inner">Inner matcher.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private static matcher repeatInfinite(matcher inner, bool greedy)
        {
            matcher newMatcher = null;
            if (greedy)
                newMatcher = (input, startIndex) => inner(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m).Select(m2 => m + m2)).Concat(0);
            else
                newMatcher = (input, startIndex) => 0.Concat(inner(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m).Select(m2 => m + m2)));
            return newMatcher;
        }

        /// <summary>
        /// Generates a regular expression that matches this regular expression zero or more times.
        /// </summary>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private Generex<T> repeatInfinite(bool greedy)
        {
            return new Generex<T>(repeatInfinite(_forwardMatcher, greedy), repeatInfinite(_backwardMatcher, greedy));
        }

        /// <summary>
        /// Generates a matcher that matches this regular expression at least a minimum number of times.
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private Generex<T> repeatMin(int min, bool greedy)
        {
            if (min < 0) throw new ArgumentException("'min' cannot be negative.", "min");
            return repeatBetween(min, min, true).Then(repeatInfinite(greedy));
        }

        /// <summary>
        /// Generates a matcher that matches this regular expression at least a minimum number of times and at most a maximum number of times.
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private Generex<T> repeatBetween(int min, int max, bool greedy)
        {
            if (min < 0) throw new ArgumentException("'min' cannot be negative.", "min");
            if (max < min) throw new ArgumentException("'max' cannot be smaller than 'min'.", "max");
            var rm = new repeatMatcher
            {
                Greedy = greedy,
                MinTimes = min,
                MaxTimes = max,
                InnerForwardMatcher = _forwardMatcher,
                InnerBackwardMatcher = _backwardMatcher
            };
            return new Generex<T>(rm.ForwardMatcher, rm.BackwardMatcher);
        }

        private sealed class repeatMatcher
        {
            public int MinTimes;
            public int MaxTimes;
            public bool Greedy;
            public matcher InnerForwardMatcher;
            public matcher InnerBackwardMatcher;
            public IEnumerable<int> ForwardMatcher(T[] input, int startIndex) { return matcher(input, startIndex, 0, InnerForwardMatcher); }
            public IEnumerable<int> BackwardMatcher(T[] input, int startIndex) { return matcher(input, startIndex, 0, InnerBackwardMatcher); }
            private IEnumerable<int> matcher(T[] input, int startIndex, int iteration, matcher inner)
            {
                if (!Greedy && iteration >= MinTimes)
                    yield return 0;
                if (iteration < MaxTimes)
                {
                    foreach (var m in inner(input, startIndex))
                        foreach (var m2 in matcher(input, startIndex + m, iteration + 1, inner))
                            yield return m + m2;
                }
                if (Greedy && iteration >= MinTimes)
                    yield return 0;
            }
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. (This always matches successfully and all matches are zero-length.)
        /// </summary>
        public Generex<T> Do(Action code)
        {
            return new Generex<T>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => { code(); return m; }),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => { code(); return m; })
            );
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. (This always matches successfully and all matches are zero-length.)
        /// </summary>
        /// <example>
        /// <para>You can use this to capture the match from a subexpression:</para>
        /// <code>
        /// string captured = null;
        /// Generex&lt;char&gt; myRe = someRe.Then(someOtherRe.Do(m => { captured = new string(m.Match.ToArray()); })).Then(yetAnotherRe);
        /// foreach (var m in myRe.Matches(input))
        ///     Console.WriteLine("Captured text: {0}", captured);
        /// </code>
        /// </example>
        public Generex<T> Do(Action<GenerexMatch<T>> code)
        {
            return new Generex<T>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => { code(new GenerexMatch<T>(input, startIndex, m)); return m; }),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => { code(new GenerexMatch<T>(input, startIndex + m, -m)); return m; })
            );
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. The return value of the specified code determines whether the expression matches successfully (all matches are zero-length).
        /// </summary>
        public Generex<T> Do(Func<bool> code)
        {
            return new Generex<T>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => code()),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => code())
            );
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. The return value of the specified code determines whether the expression matches successfully (all matches are zero-length).
        /// </summary>
        public Generex<T> Do(Func<GenerexMatch<T>, bool> code)
        {
            return new Generex<T>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => code(new GenerexMatch<T>(input, startIndex, m))),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => code(new GenerexMatch<T>(input, startIndex + m, -m)))
            );
        }

        /// <summary>Turns the current regular expression into a zero-width positive look-ahead assertion.</summary>
        public Generex<T> LookAhead() { return look(behind: false, negative: false); }
        /// <summary>Turns the current regular expression into a zero-width negative look-ahead assertion.</summary>
        public Generex<T> LookAheadNegative() { return look(behind: false, negative: true); }
        /// <summary>Turns the current regular expression into a zero-width positive look-behind assertion.</summary>
        public Generex<T> LookBehind() { return look(behind: true, negative: false); }
        /// <summary>Turns the current regular expression into a zero-width negative look-ahead assertion.</summary>
        public Generex<T> LookBehindNegative() { return look(behind: true, negative: true); }

        private Generex<T> look(bool behind, bool negative)
        {
            // In a look-*behind* assertion, both matchers use the _backwardMatcher. Similarly, look-*ahead* assertions always use _forwardMatcher.
            matcher innerMatcher = behind ? _backwardMatcher : _forwardMatcher;
            matcher newMatcher = (input, startIndex) => (innerMatcher(input, startIndex).Any() ^ negative) ? Generex.ZeroWidthMatch : Generex.NoMatch;
            return new Generex<T>(newMatcher, newMatcher);
        }

        /// <summary>Returns a successful zero-width match.</summary>
        private static IEnumerable<int> emptyMatch(T[] input, int startIndex) { return Generex.ZeroWidthMatch; }

        /// <summary>Implicitly converts an element into a regular expression that matches just that element.</summary>
        public static implicit operator Generex<T>(T element) { return new Generex<T>(element); }
        /// <summary>Implicitly converts a predicate into a regular expression that matches a single element satisfying the predicate.</summary>
        public static implicit operator Generex<T>(Predicate<T> predicate) { return new Generex<T>(predicate); }

        /// <summary>Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TResult">Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">Function to process a regular expression match.</param>
        public Generex<T, TResult> Process<TResult>(Func<GenerexMatch<T>, TResult> selector)
        {
            return new Generex<T, TResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => new GenerexMatchInfo<TResult>(selector(new GenerexMatch<T>(input, startIndex, m)), m)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => new GenerexMatchInfo<TResult>(selector(new GenerexMatch<T>(input, startIndex + m, -m)), m))
            );
        }

        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Generex<T> Recursive(Func<Generex<T>, Generex<T>> generator)
        {
            if (generator == null)
                throw new ArgumentNullException("generator");

            matcher recursiveForward = null, recursiveBackward = null;

            // Note the following *must* be lambdas so that they capture the above *variables* (which are modified afterwards), not their current value (which would be null)
            var carrier = new Generex<T>(
                (input, startIndex) => recursiveForward(input, startIndex),
                (input, startIndex) => recursiveBackward(input, startIndex)
            );

            var generated = generator(carrier);
            if (generated == null)
                throw new InvalidOperationException("Generator function returned null.");
            recursiveForward = generated._forwardMatcher;
            recursiveBackward = generated._backwardMatcher;
            return generated;
        }
    }

    /// <summary>Encapsulates preliminary information about matches generated by <see cref="Generex{T,TResult}"/>.</summary>
    /// <typeparam name="TResult">Type of objects generated from each match of the regular expression.</typeparam>
    /// <remarks>This type is an implementation detail.</remarks>
    public struct GenerexMatchInfo<TResult>
    {
        internal TResult Result { get; private set; }
        internal int Length { get; private set; }
        internal GenerexMatchInfo(TResult result, int length) : this() { Result = result; Length = length; }
        internal GenerexMatchInfo<TResult> Add(int extraLength)
        {
            return new GenerexMatchInfo<TResult>(Result, Length + extraLength);
        }
    }

    /// <summary>
    /// Provides regular-expression functionality for collections of arbitrary objects.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    /// <typeparam name="TResult">Type of objects generated from each match of the regular expression.</typeparam>
    public sealed class Generex<T, TResult> : GenerexBase<T, GenerexMatchInfo<TResult>, Generex<T, TResult>, GenerexMatch<T, TResult>>
    {
        internal Generex(matcher forward, matcher backward) : base(forward, backward) { }
        internal override Generex<T, TResult> create(matcher forward, matcher backward) { return new Generex<T, TResult>(forward, backward); }
        internal override int getLength(GenerexMatchInfo<TResult> match) { return match.Length; }
        internal override GenerexMatchInfo<TResult> add(GenerexMatchInfo<TResult> match, int extra) { return match.Add(extra); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns the result of the first match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>The result of the first match in case of success; default(TResult) if no match.</returns>
        public TResult Match(T[] input, int startAt = 0)
        {
            return Matches(input, startAt).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>The result of the first match in case of success; null if no match.</returns>
        public GenerexMatch<T, TResult> MatchInfo(T[] input, int startAt = 0)
        {
            return MatchInfos(input, startAt).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns the result of the first match
        /// found by matching the regular expression backwards (starting from the end of the input sequence).
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>The result of the match in case of success; default(TResult) if no match.</returns>
        public TResult MatchReverse(T[] input, int? endAt = null)
        {
            return MatchesReverse(input, endAt).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match
        /// found by matching the regular expression backwards (starting from the end of the input sequence).
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>The result of the match in case of success; null if no match.</returns>
        public GenerexMatch<T, TResult> MatchInfoReverse(T[] input, int? endAt = null)
        {
            return MatchInfosReverse(input, endAt).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression exactly, and if so, returns information about the match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>A <see cref="GenerexMatch{T,TResult}"/> object describing the regular expression match in case of success; null if no match.</returns>
        public GenerexMatch<T, TResult> MatchInfoExact(T[] input, int mustStartAt = 0, int? mustEndAt = null)
        {
            return matchExact(input, mustStartAt, mustEndAt ?? input.Length, m => new GenerexMatch<T, TResult>(m.Result, input, mustStartAt, m.Length));
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression exactly, and if so, returns the match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>The result of the match in case of success; default(TResult) if no match.</returns>
        public TResult MatchExact(T[] input, int mustStartAt = 0, int? mustEndAt = null)
        {
            return matchExact(input, mustStartAt, mustEndAt ?? input.Length, m => m.Result);
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>The behaviour is analogous to <see cref="Regex.Matches(string,string)"/>.
        /// The documentation for that method claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public IEnumerable<TResult> Matches(T[] input, int startAt = 0)
        {
            return matches(input, startAt, (index, resultInfo) => resultInfo.Result, backward: false);
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>The behaviour is analogous to <see cref="Regex.Matches(string,string)"/>.
        /// The documentation for that method claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public IEnumerable<GenerexMatch<T, TResult>> MatchInfos(T[] input, int startAt = 0)
        {
            return matches(input, startAt, (index, resultInfo) => new GenerexMatch<T, TResult>(resultInfo.Result, input, index, resultInfo.Length), backward: false);
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the specified
        /// input sequence), optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        public IEnumerable<TResult> MatchesReverse(T[] input, int? endAt = null)
        {
            return matches(input, endAt ?? input.Length, (index, resultInfo) => resultInfo.Result, backward: true);
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the specified
        /// input sequence), optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        public IEnumerable<GenerexMatch<T, TResult>> MatchInfosReverse(T[] input, int? endAt = null)
        {
            return matches(input, endAt ?? input.Length, (index, resultInfo) => new GenerexMatch<T, TResult>(resultInfo.Result, input, index + resultInfo.Length, -resultInfo.Length), backward: true);
        }

        /// <summary>
        /// Returns a regular expression that matches a consecutive sequence of regular expressions, beginning with this one, followed
        /// by the specified ones. The object returned for each match of the current regular expression remains the same.
        /// </summary>
        public Generex<T, TResult> Then(params Generex<T>[] other)
        {
            var newRegex = new Generex<T>(other);
            var newForward = newRegex._forwardMatcher;
            var newBackward = newRegex._backwardMatcher;
            return new Generex<T, TResult>(then(_forwardMatcher, newForward), then(newBackward, _backwardMatcher));
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified ones,
        /// and generates a match object that combines the original two matches.
        /// </summary>
        public Generex<T, TCombined> Then<TOtherResult, TCombined>(Generex<T, TOtherResult> other, Func<TResult, TOtherResult, TCombined> selector)
        {
            return new Generex<T, TCombined>(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m => other._forwardMatcher(input, startIndex + m.Length)
                    .Select(m2 => new GenerexMatchInfo<TCombined>(selector(m.Result, m2.Result), m.Length + m2.Length))),
                (input, startIndex) => other._backwardMatcher(input, startIndex).SelectMany(m2 => _backwardMatcher(input, startIndex + m2.Length)
                    .Select(m => new GenerexMatchInfo<TCombined>(selector(m.Result, m2.Result), m.Length + m2.Length)))
            );
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified ones,
        /// and generates a match object that combines the original two matches.
        /// </summary>
        public Generex<T, TCombined> ThenMatch<TOtherResult, TCombined>(Generex<T, TOtherResult> other, Func<TResult, GenerexMatch<T, TOtherResult>, TCombined> selector)
        {
            return new Generex<T, TCombined>(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m => other._forwardMatcher(input, startIndex + m.Length)
                    .Select(m2 => new GenerexMatchInfo<TCombined>(selector(m.Result, new GenerexMatch<T, TOtherResult>(m2.Result, input, startIndex + m.Length, m2.Length)), m.Length + m2.Length))),
                (input, startIndex) => other._backwardMatcher(input, startIndex).Select(m2 => new { Match = new GenerexMatch<T, TOtherResult>(m2.Result, input, startIndex + m2.Length, -m2.Length), Length = m2.Length })
                    .SelectMany(inf => _backwardMatcher(input, startIndex + inf.Length).Select(m => new GenerexMatchInfo<TCombined>(selector(m.Result, inf.Match), m.Length + inf.Length)))
            );
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified ones,
        /// and generates a match object that combines the original two matches.
        /// </summary>
        public Generex<T, TCombined> Then<TCombined>(Generex<T> other, Func<TResult, GenerexMatch<T>, TCombined> selector)
        {
            return new Generex<T, TCombined>(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m => other._forwardMatcher(input, startIndex + m.Length)
                    .Select(m2 => new GenerexMatchInfo<TCombined>(selector(m.Result, new GenerexMatch<T>(input, startIndex + m.Length, m2)), m.Length + m2))),
                (input, startIndex) => other._backwardMatcher(input, startIndex).Select(m2 => new { Match = new GenerexMatch<T>(input, startIndex + m2, -m2), Length = m2 })
                    .SelectMany(inf => _backwardMatcher(input, startIndex + inf.Length).Select(m => new GenerexMatchInfo<TCombined>(selector(m.Result, inf.Match), m.Length + inf.Length)))
            );
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements.
        /// </summary>
        public Generex<T, TResult> Then(params T[] elements) { return Then(new Generex<T>(elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements, using the specified equality comparer.
        /// </summary>
        public Generex<T, TResult> Then(IEqualityComparer<T> comparer, params T[] elements) { return Then(new Generex<T>(comparer, elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements.
        /// </summary>
        public Generex<T, TResult> Then(IEnumerable<T> elements) { return Then(new Generex<T>(elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements, using the specified equality comparer.
        /// </summary>
        public Generex<T, TResult> Then(IEqualityComparer<T> comparer, IEnumerable<T> elements) { return Then(new Generex<T>(comparer, elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by a single element that satisfies the specified predicate.
        /// </summary>
        public Generex<T, TResult> Then(Predicate<T> predicate) { return Then(new Generex<T>(predicate)); }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified other regular expression (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, TResult> Or(Generex<T, TResult> other)
        {
            return new Generex<T, TResult>(or(_forwardMatcher, other._forwardMatcher), or(_backwardMatcher, other._backwardMatcher));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or a single element that satisfies the specified predicate (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, TResult> Or(Predicate<T> predicate, Func<GenerexMatch<T>, TResult> selector)
        {
            return Or(new Generex<T>(predicate).Process(selector));
        }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static Generex<T, TResult> Ors(params Generex<T, TResult>[] other)
        {
            return new Generex<T, TResult>(
                other.Select(p => p._forwardMatcher).Aggregate(or),
                other.Select(p => p._backwardMatcher).Aggregate(or)
            );
        }

        /// <summary>
        /// Returns a regular expression that matches exactly one of the specified regular expressions.
        /// (This means that as soon as one matches, then the remaining ones are not matched in backtracking.)
        /// </summary>
        public static Generex<T, TResult> OneOf(params Generex<T, TResult>[] other)
        {
            return new Generex<T, TResult>(
                other.Select(p => p._forwardMatcher).Aggregate(oneOf),
                other.Select(p => p._backwardMatcher).Aggregate(oneOf)
            );
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf. "?" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> OptionalGreedy() { return repeatBetween(0, 1, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Zero times is prioritised (cf. "??" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> Optional() { return repeatBetween(0, 1, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. More times are prioritised (cf. "*" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> RepeatGreedy() { return new Generex<T, IEnumerable<TResult>>(repeatInfinite(_forwardMatcher, true), repeatInfinite(_backwardMatcher, true)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. Fewer times are prioritised (cf. "*?" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> Repeat() { return new Generex<T, IEnumerable<TResult>>(repeatInfinite(_forwardMatcher, false), repeatInfinite(_backwardMatcher, false)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. More times are prioritised (cf. "{min,}" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> RepeatGreedy(int min) { return repeatMin(min, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. Fewer times are prioritised (cf. "{min,}?" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> Repeat(int min) { return repeatMin(min, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. More times are prioritised (cf. "{min,max}" in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public Generex<T, IEnumerable<TResult>> RepeatGreedy(int min, int max) { return repeatBetween(min, max, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. Fewer times are prioritised (cf. "{min,max}?" in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public Generex<T, IEnumerable<TResult>> Repeat(int min, int max) { return repeatBetween(min, max, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times (cf. "{times}" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> Times(int times)
        {
            if (times < 0) throw new ArgumentException("'times' cannot be negative.", "times");
            return repeatBetween(times, times, true);
        }
        /// <summary>
        /// Returns a regular expression that matches this regular expression one or more times, interspersed with a separator. Fewer times are prioritised.
        /// </summary>
        public Generex<T, IEnumerable<TResult>> RepeatWithSeparator(Generex<T> separator) { return Then(separator.Then(this).Repeat(), (first, rest) => first.Concat(rest)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression one or more times, interspersed with a separator. More times are prioritised.
        /// </summary>
        public Generex<T, IEnumerable<TResult>> RepeatWithSeparatorGreedy(Generex<T> separator) { return Then(separator.Then(this).RepeatGreedy(), (first, rest) => first.Concat(rest)); }

        /// <summary>
        /// Generates a matcher that matches this regular expression zero or more times.
        /// </summary>
        /// <param name="inner">Inner matcher.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private static Generex<T, IEnumerable<TResult>>.matcher repeatInfinite(matcher inner, bool greedy)
        {
            Generex<T, IEnumerable<TResult>>.matcher newMatcher = null;
            if (greedy)
                newMatcher = (input, startIndex) => inner(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m.Length)
                    .Select(m2 => new GenerexMatchInfo<IEnumerable<TResult>>(m.Result.Concat(m2.Result), m.Length + m2.Length)))
                    .Concat(new GenerexMatchInfo<IEnumerable<TResult>>(Enumerable.Empty<TResult>(), 0));
            else
                newMatcher = (input, startIndex) => new GenerexMatchInfo<IEnumerable<TResult>>(Enumerable.Empty<TResult>(), 0)
                    .Concat(inner(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m.Length)
                    .Select(m2 => new GenerexMatchInfo<IEnumerable<TResult>>(m.Result.Concat(m2.Result), m.Length + m2.Length))));
            return newMatcher;
        }

        /// <summary>
        /// Generates a regular expression that matches this regular expression zero or more times.
        /// </summary>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private Generex<T, IEnumerable<TResult>> repeatInfinite(bool greedy)
        {
            return new Generex<T, IEnumerable<TResult>>(repeatInfinite(_forwardMatcher, greedy), repeatInfinite(_backwardMatcher, greedy));
        }

        /// <summary>
        /// Generates a matcher that matches this regular expression at least a minimum number of times.
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private Generex<T, IEnumerable<TResult>> repeatMin(int min, bool greedy)
        {
            if (min < 0) throw new ArgumentException("'min' cannot be negative.", "min");
            return repeatBetween(min, min, true).Then(repeatInfinite(greedy), Enumerable.Concat);
        }

        /// <summary>
        /// Generates a matcher that matches this regular expression at least a minimum number of times and at most a maximum number of times.
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private Generex<T, IEnumerable<TResult>> repeatBetween(int min, int max, bool greedy)
        {
            if (min < 0) throw new ArgumentException("'min' cannot be negative.", "min");
            if (max < min) throw new ArgumentException("'max' cannot be smaller than 'min'.", "max");
            var rm = new repeatMatcher
            {
                Greedy = greedy,
                MinTimes = min,
                MaxTimes = max,
                InnerForwardMatcher = _forwardMatcher,
                InnerBackwardMatcher = _backwardMatcher
            };
            return new Generex<T, IEnumerable<TResult>>(rm.ForwardMatcher, rm.BackwardMatcher);
        }

        private sealed class repeatMatcher
        {
            public int MinTimes;
            public int MaxTimes;
            public bool Greedy;
            public matcher InnerForwardMatcher;
            public matcher InnerBackwardMatcher;
            public IEnumerable<GenerexMatchInfo<IEnumerable<TResult>>> ForwardMatcher(T[] input, int startIndex) { return matcher(input, startIndex, 0, backward: false); }
            public IEnumerable<GenerexMatchInfo<IEnumerable<TResult>>> BackwardMatcher(T[] input, int startIndex) { return matcher(input, startIndex, 0, backward: true); }
            private IEnumerable<GenerexMatchInfo<IEnumerable<TResult>>> matcher(T[] input, int startIndex, int iteration, bool backward)
            {
                if (!Greedy && iteration >= MinTimes)
                    yield return new GenerexMatchInfo<IEnumerable<TResult>>(Enumerable.Empty<TResult>(), 0);
                if (iteration < MaxTimes)
                {
                    foreach (var m in (backward ? InnerBackwardMatcher : InnerForwardMatcher)(input, startIndex))
                        foreach (var m2 in matcher(input, startIndex + m.Length, iteration + 1, backward))
                            yield return new GenerexMatchInfo<IEnumerable<TResult>>(
                                backward ? m2.Result.Concat(m.Result) : m.Result.Concat(m2.Result),
                                m.Length + m2.Length
                            );
                }
                if (Greedy && iteration >= MinTimes)
                    yield return new GenerexMatchInfo<IEnumerable<TResult>>(Enumerable.Empty<TResult>(), 0);
            }
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. (This always matches successfully and all matches are zero-length.)
        /// </summary>
        public Generex<T, TResult> Do(Action code)
        {
            return new Generex<T, TResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => { code(); return m; }),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => { code(); return m; })
            );
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. (This always matches successfully and all matches are zero-length.)
        /// </summary>
        /// <example>
        /// <para>You can use this to capture the match from a subexpression:</para>
        /// <code>
        /// string captured = null;
        /// Generex&lt;char&gt; myRe = someRe.Then(someOtherRe.Do(m => { captured = new string(m.Match.ToArray()); })).Then(yetAnotherRe);
        /// foreach (var m in myRe.Matches(input))
        ///     Console.WriteLine("Captured text: {0}", captured);
        /// </code>
        /// </example>
        public Generex<T, TResult> Do(Action<TResult> code)
        {
            return new Generex<T, TResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => { code(m.Result); return m; }),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => { code(m.Result); return m; })
            );
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. The return value of the specified code determines whether the expression matches successfully (all matches are zero-length).
        /// </summary>
        public Generex<T, TResult> Do(Func<bool> code)
        {
            return new Generex<T, TResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => code()),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => code())
            );
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. The return value of the specified code determines whether the expression matches successfully (all matches are zero-length).
        /// </summary>
        public Generex<T, TResult> Do(Func<TResult, bool> code)
        {
            return new Generex<T, TResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => code(m.Result)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => code(m.Result))
            );
        }

        /// <summary>Turns the current regular expression into a zero-width positive look-ahead assertion.</summary>
        public Generex<T, TResult> LookAhead() { return look(behind: false); }
        /// <summary>Turns the current regular expression into a zero-width positive look-behind assertion.</summary>
        public Generex<T, TResult> LookBehind() { return look(behind: true); }

        private Generex<T, TResult> look(bool behind)
        {
            // In a look-*behind* assertion, both matchers use the _backwardMatcher. Similarly, look-*ahead* assertions always use _forwardMatcher.
            matcher innerMatcher = behind ? _backwardMatcher : _forwardMatcher;
            matcher newMatcher = (input, startIndex) => innerMatcher(input, startIndex).Take(1).Select(m => new GenerexMatchInfo<TResult>(m.Result, 0));
            return new Generex<T, TResult>(newMatcher, newMatcher);
        }

        /// <summary>Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TOtherResult">Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">Function to process a regular expression match.</param>
        public Generex<T, TOtherResult> Process<TOtherResult>(Func<TResult, TOtherResult> selector)
        {
            return new Generex<T, TOtherResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => new GenerexMatchInfo<TOtherResult>(selector(m.Result), m.Length)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => new GenerexMatchInfo<TOtherResult>(selector(m.Result), m.Length))
            );
        }

        /// <summary>Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TOtherResult">Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">Function to process a regular expression match.</param>
        public Generex<T, TOtherResult> ProcessMatch<TOtherResult>(Func<GenerexMatch<T, TResult>, TOtherResult> selector)
        {
            return new Generex<T, TOtherResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => new GenerexMatchInfo<TOtherResult>(selector(new GenerexMatch<T, TResult>(m.Result, input, startIndex, m.Length)), m.Length)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => new GenerexMatchInfo<TOtherResult>(selector(new GenerexMatch<T, TResult>(m.Result, input, startIndex + m.Length, -m.Length)), m.Length))
            );
        }

        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Generex<T, TResult> Recursive(Func<Generex<T, TResult>, Generex<T, TResult>> generator)
        {
            if (generator == null)
                throw new ArgumentNullException("generator");

            matcher recursiveForward = null, recursiveBackward = null;

            // Note the following *must* be lambdas so that they capture the above *variables* (which are modified afterwards), not their current value (which would be null)
            var carrier = new Generex<T, TResult>(
                (input, startIndex) => recursiveForward(input, startIndex),
                (input, startIndex) => recursiveBackward(input, startIndex)
            );

            var generated = generator(carrier);
            if (generated == null)
                throw new InvalidOperationException("Generator function returned null.");
            recursiveForward = generated._forwardMatcher;
            recursiveBackward = generated._backwardMatcher;
            return generated;
        }
    }

    /// <summary>
    /// Provides static factory methods to generate <see cref="Generex{T}"/> objects.
    /// </summary>
    public static class Generex
    {
        internal static IEnumerable<int> NoMatch = Enumerable.Empty<int>();
        internal static IEnumerable<int> ZeroWidthMatch = new int[] { 0 };
        internal static IEnumerable<int> OneElementMatch = new int[] { 1 };
        internal static IEnumerable<int> NegativeOneElementMatch = new int[] { -1 };

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public static Generex<T> New<T>(params T[] elements) { return new Generex<T>(elements); }
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        public static Generex<T> New<T>(IEqualityComparer<T> comparer, params T[] elements) { return new Generex<T>(comparer, elements); }
        /// <summary>
        /// Instantiates a regular expression that matches a single element that satisfies the given predicate (cf. "[...]" in traditional regular expression syntax).
        /// </summary>
        public static Generex<T> New<T>(Predicate<T> predicate) { return new Generex<T>(predicate); }
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive regular expressions.
        /// </summary>
        public static Generex<T> New<T>(params Generex<T>[] generexes) { return new Generex<T>(generexes); }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static Generex<T> Ors<T>(params Generex<T>[] other) { return Generex<T>.Ors(other); }
        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static Generex<T> OneOf<T>(params Generex<T>[] other) { return Generex<T>.OneOf(other); }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static Generex<T, TResult> Ors<T, TResult>(params Generex<T, TResult>[] other) { return Generex<T, TResult>.Ors(other); }
        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static Generex<T, TResult> OneOf<T, TResult>(params Generex<T, TResult>[] other) { return Generex<T, TResult>.OneOf(other); }

        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Generex<T> Recursive<T>(Func<Generex<T>, Generex<T>> generator) { return Generex<T>.Recursive(generator); }
        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Generex<T, TResult> Recursive<T, TResult>(Func<Generex<T, TResult>, Generex<T, TResult>> generator) { return Generex<T, TResult>.Recursive(generator); }
    }

    /// <summary>
    /// Represents the result of a regular expression match using <see cref="Generex{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    public class GenerexMatch<T>
    {
        /// <summary>
        /// Gets the index in the original collection at which the match occurred.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the length of the match.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the entire original sequence against which the regular expression was matched.
        /// </summary>
        public T[] OriginalSource { get; private set; }

        /// <summary>
        /// Returns a slice of the original collection which the regular expression matched.
        /// </summary>
        public T[] Match { get { return _match ?? (_match = OriginalSource.Subarray(Index, Length)); } }
        private T[] _match;

        internal GenerexMatch(T[] original, int index, int length)
        {
            OriginalSource = original;
            Index = index;
            Length = length;
        }
    }

    /// <summary>
    /// Represents the result of a regular expression match using <see cref="Generex{T,TResult}"/>.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    /// <typeparam name="TResult">Type of objects generated from each match of the regular expression.</typeparam>
    public sealed class GenerexMatch<T, TResult> : GenerexMatch<T>
    {
        /// <summary>Contains the object generated from this match of the regular expression.</summary>
        public TResult Result { get; private set; }

        internal GenerexMatch(TResult result, T[] original, int index, int length)
            : base(original, index, length)
        {
            Result = result;
        }
    }
}
