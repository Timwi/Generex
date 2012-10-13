using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;

namespace RT.KitchenSink.PowerfulRegex
{
    /// <summary>
    /// Provides regular-expression functionality for collections of arbitrary objects.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    public sealed class PRegex<T>
    {
        private delegate IEnumerable<int> matcher(T[] input, int startIndex);
        private matcher _forwardMatcher, _backwardMatcher;

        /// <summary>
        /// Instantiates an empty regular expression (always matches).
        /// </summary>
        public PRegex()
        {
            _forwardMatcher = emptyMatch;
            _backwardMatcher = emptyMatch;
        }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public PRegex(params T[] elements)
        {
            _forwardMatcher = elementsMatcher(elements, EqualityComparer<T>.Default, backward: false);
            _backwardMatcher = elementsMatcher(elements, EqualityComparer<T>.Default, backward: true);
        }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        public PRegex(IEqualityComparer<T> comparer, params T[] elements)
        {
            _forwardMatcher = elementsMatcher(elements, comparer, backward: false);
            _backwardMatcher = elementsMatcher(elements, comparer, backward: true);
        }

        /// <summary>
        /// Instantiates a regular expression that matches a single element that satisfies the given predicate (cf. "[...]" in traditional regular expression syntax).
        /// </summary>
        public PRegex(Predicate<T> predicate)
        {
            _forwardMatcher = (input, startIndex) => startIndex >= input.Length || !predicate(input[startIndex]) ? PRegex.NoMatch : PRegex.OneElementMatch;
            _backwardMatcher = (input, startIndex) => startIndex <= 0 || !predicate(input[startIndex - 1]) ? PRegex.NoMatch : PRegex.NegativeOneElementMatch;
        }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive regular expressions.
        /// </summary>
        public PRegex(params PRegex<T>[] pregexes)
        {
            if (pregexes.Length == 0)
            {
                _forwardMatcher = emptyMatch;
                _backwardMatcher = emptyMatch;
            }
            else if (pregexes.Length == 1)
            {
                _forwardMatcher = pregexes[0]._forwardMatcher;
                _backwardMatcher = pregexes[0]._backwardMatcher;
            }
            else
            {
                _forwardMatcher = pregexes.Select(p => p._forwardMatcher).Aggregate(then);
                _backwardMatcher = pregexes.Reverse().Select(p => p._backwardMatcher).Aggregate(then);
            }
        }

        private PRegex(matcher forward, matcher backward)
        {
            _forwardMatcher = forward;
            _backwardMatcher = backward;
        }

        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression, optionally starting the search at a specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        public bool IsMatch(T[] input, int startAt = 0) { return Enumerable.Range(startAt, input.Length - startAt + 1).SelectMany(si => _forwardMatcher(input, si)).Any(); }

        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression that ends before the specified maximum index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        public bool IsMatchReverse(T[] input, int? endAt = null) { return rangeReverse(endAt ?? input.Length, (endAt ?? input.Length) + 1).SelectMany(si => _backwardMatcher(input, si)).Any(); }

        private static IEnumerable<int> rangeReverse(int start, int count)
        {
            for (int i = 0; i < count; i++)
                yield return start - i;
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>A <see cref="PRegexMatch{T}"/> object describing a regular expression match in case of success; null if no match.</returns>
        public PRegexMatch<T> Match(T[] input, int startAt = 0)
        {
            return Matches(input, startAt).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match
        /// found by matching the regular expression backwards (starting from the end of the input sequence).
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>A <see cref="PRegexMatch{T}"/> object describing a regular expression match in case of success; null if no match.</returns>
        public PRegexMatch<T> MatchReverse(T[] input, int? endAt = null)
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
        public IEnumerable<PRegexMatch<T>> Matches(T[] input, int startAt = 0)
        {
            int i = 0;
            while (i <= input.Length)
            {
                int matchLength = _forwardMatcher(input, i).FirstOrDefault(-1);
                if (matchLength == -1)
                {
                    i++;
                    continue;
                }
                yield return new PRegexMatch<T>(input, i, matchLength);
                i += matchLength == 0 ? 1 : matchLength;
            }
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the specified
        /// input sequence), optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        /// <remarks>The behaviour is analogous to <see cref="Regex.Matches(string,string)"/>.
        /// The documentation for that method claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public IEnumerable<PRegexMatch<T>> MatchesReverse(T[] input, int? endAt = null)
        {
            int i = endAt ?? input.Length;
            while (i >= 0)
            {
                int matchLength = _backwardMatcher(input, i).FirstOrDefault(1);
                if (matchLength == 1)
                {
                    i--;
                    continue;
                }
                yield return new PRegexMatch<T>(input, i + matchLength, -matchLength);
                i += matchLength == 0 ? -1 : matchLength;
            }
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
                    return (input, startIndex) => startIndex > 0 && comparer.Equals(input[startIndex - 1], element) ? PRegex.NegativeOneElementMatch : PRegex.NoMatch;
                else
                    return (input, startIndex) => startIndex < input.Length && comparer.Equals(input[startIndex], element) ? PRegex.OneElementMatch : PRegex.NoMatch;
            }

            if (backward)
                return (input, startIndex) => startIndex >= elements.Length && input.SubarrayEquals(startIndex - elements.Length, elements, comparer) ? new int[] { -elements.Length } : PRegex.NoMatch;
            else
                return (input, startIndex) => startIndex <= input.Length - elements.Length && input.SubarrayEquals(startIndex, elements, comparer) ? new int[] { elements.Length } : PRegex.NoMatch;
        }

        /// <summary>
        /// Generates a matcher that matches the <paramref name="first"/> regular expression followed by the <paramref name="second"/> regular expression.
        /// </summary>
        private static matcher then(matcher first, matcher second)
        {
            return (input, startIndex) => first(input, startIndex).SelectMany(m => second(input, startIndex + m).Select(m2 => m + m2));
        }

        /// <summary>
        /// Returns a regular expression that matches a single element, no matter what it is (cf. "." in traditional regular expression syntax).
        /// </summary>
        public static PRegex<T> Any
        {
            get
            {
                if (_anyCache == null)
                    _anyCache = new PRegex<T>(
                        (input, startIndex) => startIndex >= input.Length ? PRegex.NoMatch : PRegex.OneElementMatch,
                        (input, startIndex) => startIndex <= 0 ? PRegex.NoMatch : PRegex.NegativeOneElementMatch
                    );
                return _anyCache;
            }
        }
        private static PRegex<T> _anyCache;

        /// <summary>
        /// Returns a regular expression that matches a consecutive sequence of regular expressions, beginning with this one, followed by the specified ones.
        /// </summary>
        public PRegex<T> Then(params PRegex<T>[] other)
        {
            return new PRegex<T>(
                _forwardMatcher.Concat(other.Select(o => o._forwardMatcher)).Aggregate(then),
                other.Reverse().Select(o => o._backwardMatcher).Concat(_backwardMatcher).Aggregate(then)
            );
        }

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

        private static matcher or(matcher one, matcher two)
        {
            return one == null ? two : (input, startIndex) => one(input, startIndex).Concat(two(input, startIndex));
        }

        private PRegex<T> or(PRegex<T> other)
        {
            return new PRegex<T>(or(_forwardMatcher, other._forwardMatcher), or(other._backwardMatcher, _backwardMatcher));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified sequence of regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        /// <example>
        /// <para>The following code:</para>
        /// <code>var regex = regex1.Or(regex2, regex3);</code>
        /// <para>generates a regular expression equivalent to <c>1|23</c>, NOT <c>1|2|3</c>.</para>
        /// </example>
        public PRegex<T> Or(params PRegex<T>[] other)
        {
            if (other.Length == 0)
                return this;
            if (other.Length == 1)
                return or(other[0]);
            return or(new PRegex<T>(other));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified sequence of elements (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        /// <example>
        /// <para>The following code:</para>
        /// <code>var regex = new PRegex&lt;char&gt;('a', 'b').Or('c', 'd');</code>
        /// <para>is equivalent to the regular expression <c>ab|cd</c>, NOT <c>ab|c|d</c>.</para>
        /// </example>
        /// <seealso cref="Or(IEqualityComparer{T},T[])"/>
        public PRegex<T> Or(params T[] elements)
        {
            if (elements.Length == 0)
                return this;
            return or(new PRegex<T>(elements));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or any of the specified elements using the specified equality comparer (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        /// <seealso cref="Or(T[])"/>
        public PRegex<T> Or(IEqualityComparer<T> comparer, params T[] elements)
        {
            if (elements.Length == 0)
                return this;
            return or(new PRegex<T>(comparer, elements));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or a single element that satisfies the specified predicate (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public PRegex<T> Or(Predicate<T> predicate)
        {
            return Or(new PRegex<T>(predicate));
        }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors(params PRegex<T>[] other)
        {
            return new PRegex<T>(
                other.Select(p => p._forwardMatcher).Aggregate(or),
                other.Reverse().Select(p => p._backwardMatcher).Aggregate(or)
            );
        }

        /// <summary>
        /// Returns a regular expression that matches any one of the specified elements (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors(params T[] elements) { return Ors(EqualityComparer<T>.Default, elements); }

        /// <summary>
        /// Returns a regular expression that matches any one of the specified elements using the specified equality comparer (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors(IEqualityComparer<T> comparer, params T[] elements)
        {
            return new PRegex<T>(
                (input, startIndex) => startIndex >= input.Length ? PRegex.NoMatch : elements.Any(el => comparer.Equals(el, input[startIndex])) ? PRegex.OneElementMatch : PRegex.NoMatch,
                (input, startIndex) => startIndex <= 0 ? PRegex.NoMatch : elements.Any(el => comparer.Equals(el, input[startIndex])) ? PRegex.OneElementMatch : PRegex.NoMatch
            );
        }

        /// <summary>
        /// Returns a regular expression that matches the beginning of the input collection (cf. "^" in traditional regular expression syntax). Successful matches are always zero length.
        /// </summary>
        public static PRegex<T> Start
        {
            get
            {
                if (_startCache == null)
                {
                    matcher matcher = (input, startIndex) => startIndex != 0 ? PRegex.NoMatch : PRegex.ZeroWidthMatch;
                    _startCache = new PRegex<T>(matcher, matcher);
                }
                return _startCache;
            }
        }
        public static PRegex<T> _startCache;

        /// <summary>
        /// Returns a regular expression that matches the end of the input collection (cf. "$" in traditional regular expression syntax). Successful matches are always zero length.
        /// </summary>
        public static PRegex<T> End
        {
            get
            {
                if (_endCache == null)
                {
                    matcher matcher = (input, startIndex) => startIndex != input.Length ? PRegex.NoMatch : PRegex.ZeroWidthMatch;
                    _endCache = new PRegex<T>(matcher, matcher);
                }
                return _endCache;
            }
        }
        public static PRegex<T> _endCache;

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf. "?" in traditional regular expression syntax).
        /// </summary>
        public PRegex<T> OptionalGreedy() { return repeatBetween(0, 1, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Zero times is prioritised (cf. "??" in traditional regular expression syntax).
        /// </summary>
        public PRegex<T> Optional() { return repeatBetween(0, 1, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. More times are prioritised (cf. "*" in traditional regular expression syntax).
        /// </summary>
        public PRegex<T> RepeatGreedy() { return new PRegex<T>(repeatInfinite(_forwardMatcher, true), repeatInfinite(_backwardMatcher, true)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. Fewer times are prioritised (cf. "*?" in traditional regular expression syntax).
        /// </summary>
        public PRegex<T> Repeat() { return new PRegex<T>(repeatInfinite(_forwardMatcher, false), repeatInfinite(_backwardMatcher, false)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. More times are prioritised (cf. "{min,}" in traditional regular expression syntax).
        /// </summary>
        public PRegex<T> RepeatGreedy(int min) { return repeatMin(min, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. Fewer times are prioritised (cf. "{min,}?" in traditional regular expression syntax).
        /// </summary>
        public PRegex<T> Repeat(int min) { return repeatMin(min, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. More times are prioritised (cf. "{min,max}" in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public PRegex<T> RepeatGreedy(int min, int max) { return repeatBetween(min, max, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. Fewer times are prioritised (cf. "{min,max}?" in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public PRegex<T> Repeat(int min, int max) { return repeatBetween(min, max, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times (cf. "{times}" in traditional regular expression syntax).
        /// </summary>
        public PRegex<T> Times(int times)
        {
            if (times < 0) throw new ArgumentException("'times' cannot be negative.", "times");
            return repeatBetween(times, times, true);
        }

        /// <summary>
        /// Generates a matcher that matches this regular expression zero or more times.
        /// </summary>
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
        private PRegex<T> repeatInfinite(bool greedy)
        {
            return new PRegex<T>(repeatInfinite(_forwardMatcher, greedy), repeatInfinite(_backwardMatcher, greedy));
        }

        /// <summary>
        /// Generates a matcher that matches this regular expression at least a minimum number of times.
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private PRegex<T> repeatMin(int min, bool greedy)
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
        private PRegex<T> repeatBetween(int min, int max, bool greedy)
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
            return new PRegex<T>(rm.ForwardMatcher, rm.BackwardMatcher);
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
        public PRegex<T> Do(Action code)
        {
            return new PRegex<T>(
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
        /// PRegex&lt;char&gt; myRe = someRe.Then(someOtherRe.Do(m => { captured = new string(m.Match.ToArray()); })).Then(yetAnotherRe);
        /// foreach (var m in myRe.Matches(input))
        ///     Console.WriteLine("Captured text: {0}", captured);
        /// </code>
        /// </example>
        public PRegex<T> Do(Action<PRegexMatch<T>> code)
        {
            return new PRegex<T>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => { code(new PRegexMatch<T>(input, startIndex, m)); return m; }),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => { code(new PRegexMatch<T>(input, startIndex + m, -m)); return m; })
            );
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. The return value of the specified code determines whether the expression matches successfully (all matches are zero-length).
        /// </summary>
        public PRegex<T> Do(Func<bool> code)
        {
            return new PRegex<T>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => code()),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => code())
            );
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. The return value of the specified code determines whether the expression matches successfully (all matches are zero-length).
        /// </summary>
        public PRegex<T> Do(Func<PRegexMatch<T>, bool> code)
        {
            return new PRegex<T>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => code(new PRegexMatch<T>(input, startIndex, m))),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => code(new PRegexMatch<T>(input, startIndex + m, -m)))
            );
        }

        /// <summary>
        /// Turns the current regular expression into a zero-width positive look-ahead assertion.
        /// </summary>
        public PRegex<T> LookAhead()
        {
            // Note even the backwards matcher uses _forwardMatcher because it’s a look-*ahead* assertion even if we’re going backwards overall.
            matcher matcher = (input, startIndex) => _forwardMatcher(input, startIndex).Any() ? PRegex.ZeroWidthMatch : PRegex.NoMatch;
            return new PRegex<T>(matcher, matcher);
        }

        /// <summary>
        /// Turns the current regular expression into a zero-width negative look-ahead assertion.
        /// </summary>
        public PRegex<T> LookAheadNegative()
        {
            // Note even the backwards matcher uses _forwardMatcher because it’s a look-*ahead* assertion even if we’re going backwards overall.
            matcher matcher = (input, startIndex) => _forwardMatcher(input, startIndex).Any() ? PRegex.NoMatch : PRegex.ZeroWidthMatch;
            return new PRegex<T>(matcher, matcher);
        }

        /// <summary>
        /// Turns the current regular expression into a zero-width positive look-behind assertion.
        /// </summary>
        public PRegex<T> LookBehind()
        {
            // Note even the forwards matcher uses _backwardMatcher because it’s a look-*behind* assertion even if we’re going forwards overall.
            matcher matcher = (input, startIndex) => _backwardMatcher(input, startIndex).Any() ? PRegex.ZeroWidthMatch : PRegex.NoMatch;
            return new PRegex<T>(matcher, matcher);
        }

        /// <summary>
        /// Turns the current regular expression into a zero-width negative look-ahead assertion.
        /// </summary>
        public PRegex<T> LookBehindNegative()
        {
            // Note even the forwards matcher uses _backwardMatcher because it’s a look-*behind* assertion even if we’re going forwards overall.
            matcher matcher = (input, startIndex) => _backwardMatcher(input, startIndex).Any() ? PRegex.NoMatch : PRegex.ZeroWidthMatch;
            return new PRegex<T>(matcher, matcher);
        }

        /// <summary>Returns a successful zero-width match.</summary>
        private static IEnumerable<int> emptyMatch(T[] input, int startIndex) { return PRegex.ZeroWidthMatch; }

        /// <summary>Implicitly converts an element into a regular expression that matches just that element.</summary>
        public static implicit operator PRegex<T>(T element) { return new PRegex<T>(element); }
        /// <summary>Implicitly converts a predicate into a regular expression that matches a single element satisfying the predicate.</summary>
        public static implicit operator PRegex<T>(Predicate<T> predicate) { return new PRegex<T>(predicate); }
    }

    /// <summary>
    /// Provides static factory methods to generate <see cref="PRegex{T}"/> objects.
    /// </summary>
    public static class PRegex
    {
        internal static IEnumerable<int> NoMatch = new int[0];
        internal static IEnumerable<int> ZeroWidthMatch = new int[] { 0 };
        internal static IEnumerable<int> OneElementMatch = new int[] { 1 };
        internal static IEnumerable<int> NegativeOneElementMatch = new int[] { -1 };

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public static PRegex<T> New<T>(params T[] elements) { return new PRegex<T>(elements); }
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        public static PRegex<T> New<T>(IEqualityComparer<T> comparer, params T[] elements) { return new PRegex<T>(comparer, elements); }
        /// <summary>
        /// Instantiates a regular expression that matches a single element that satisfies the given predicate (cf. "[...]" in traditional regular expression syntax).
        /// </summary>
        public static PRegex<T> New<T>(Predicate<T> predicate) { return new PRegex<T>(predicate); }
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive regular expressions.
        /// </summary>
        public static PRegex<T> New<T>(params PRegex<T>[] pregexes) { return new PRegex<T>(pregexes); }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors<T>(params PRegex<T>[] other) { return PRegex<T>.Ors(other); }
        /// <summary>
        /// Returns a regular expression that matches any one of the specified elements (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors<T>(params T[] elements) { return PRegex<T>.Ors(EqualityComparer<T>.Default, elements); }
        /// <summary>
        /// Returns a regular expression that matches any one of the specified elements using the specified equality comparer (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        public static PRegex<T> Ors<T>(IEqualityComparer<T> comparer, params T[] elements) { return PRegex<T>.Ors(comparer, elements); }
    }

    /// <summary>
    /// Represents the result of a regular expression match using <see cref="PRegex{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    public sealed class PRegexMatch<T>
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
