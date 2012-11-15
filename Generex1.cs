using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;

namespace RT.Generexes
{
    /// <summary>
    /// Provides regular-expression functionality for collections of arbitrary objects.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    public sealed class Generex<T> : GenerexBase<T, int, Generex<T>, GenerexMatch<T>>
    {
        internal override Generex<T> create(matcher forward, matcher backward) { return new Generex<T>(forward, backward); }
        internal override int getLength(int match) { return match; }
        internal override int add(int match, int extra) { return match + extra; }
        internal override int setZero(int match) { return 0; }
        internal override GenerexMatch<T> createMatch(T[] input, int index, int match) { return new GenerexMatch<T>(input, index, match); }
        internal override GenerexMatch<T> createBackwardsMatch(T[] input, int index, int match) { return new GenerexMatch<T>(input, index + match, -match); }

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
        /// Returns a regular expression that matches this regular expression, followed by the specified other,
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
            return Or(new Generex<T>(elements));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or any of the specified elements using the specified equality comparer (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        /// <seealso cref="Or(T[])"/>
        public Generex<T> Or(IEqualityComparer<T> comparer, params T[] elements)
        {
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
                newMatcher = (input, startIndex) => inner(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m)
                    .Select(m2 => m + m2))
                    .Concat(0);
            else
                newMatcher = (input, startIndex) => 0
                    .Concat(inner(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m)
                    .Select(m2 => m + m2)));
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

        /// <summary>Turns the current regular expression into a zero-width negative look-ahead assertion.</summary>
        public Generex<T> LookAheadNegative() { return lookNegative(behind: false, defaultMatch: Generex.ZeroWidthMatch); }
        /// <summary>Turns the current regular expression into a zero-width negative look-ahead assertion.</summary>
        public Generex<T> LookBehindNegative() { return lookNegative(behind: true, defaultMatch: Generex.ZeroWidthMatch); }

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
}
