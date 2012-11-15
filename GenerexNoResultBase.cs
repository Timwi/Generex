﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;
using System.Reflection;

namespace RT.Generexes
{
    /// <summary>Abstract base class for <see cref="Generex{T}"/> and siblings.</summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    /// <typeparam name="TGenerex">The derived type. (Pass the type itself recursively.)</typeparam>
    /// <typeparam name="TGenerexMatch">Type describing a match of a regular expression.</typeparam>
    public abstract class GenerexNoResultBase<T, TGenerex, TGenerexMatch> : GenerexBase<T, int, TGenerex, TGenerexMatch>
        where TGenerex : GenerexNoResultBase<T, TGenerex, TGenerexMatch>
        where TGenerexMatch : GenerexMatch<T>
    {
        internal sealed override int getLength(int match) { return match; }
        internal sealed override int add(int match, int extra) { return match + extra; }
        internal sealed override int setZero(int match) { return 0; }
        internal sealed override TGenerexMatch createMatch(T[] input, int index, int match) { return createNoResultMatch(input, index, match); }
        internal sealed override TGenerexMatch createBackwardsMatch(T[] input, int index, int match) { return createNoResultMatch(input, index + match, -match); }
        internal abstract TGenerexMatch createNoResultMatch(T[] input, int index, int matchLength);

        /// <summary>
        /// Instantiates an empty regular expression (always matches).
        /// </summary>
        internal GenerexNoResultBase() : base(emptyMatch, emptyMatch) { }

        internal GenerexNoResultBase(IEqualityComparer<T> comparer, T[] elements)
            : base(
                elementsMatcher(elements, comparer, backward: false),
                elementsMatcher(elements, comparer, backward: true)) { }

        internal GenerexNoResultBase(Predicate<T> predicate)
            : base(
                predicateMatcher(predicate, backward: false),
                predicateMatcher(predicate, backward: true)) { }

        internal GenerexNoResultBase(GenerexNoResultBase<T, TGenerex, TGenerexMatch>[] generexSequence)
            : base(
                sequenceMatcher(generexSequence, backward: false),
                sequenceMatcher(generexSequence, backward: true)) { }

        internal GenerexNoResultBase(matcher forward, matcher backward) : base(forward, backward) { }

        /// <summary>
        /// Generates a matcher that matches a sequence of specific elements either fully or not at all.
        /// </summary>
        private static matcher elementsMatcher(T[] elements, IEqualityComparer<T> comparer, bool backward)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

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

        internal static matcher predicateMatcher(Predicate<T> predicate, bool backward)
        {
            if (backward)
                return (input, startIndex) => startIndex <= 0 || !predicate(input[startIndex - 1]) ? Generex.NoMatch : Generex.NegativeOneElementMatch;
            else
                return (input, startIndex) => startIndex >= input.Length || !predicate(input[startIndex]) ? Generex.NoMatch : Generex.OneElementMatch;
        }

        /// <summary>
        /// Generates a matcher that matches the <paramref name="first"/> regular expression followed by the <paramref name="second"/> regular expression.
        /// </summary>
        internal static matcher then(matcher first, matcher second)
        {
            return (input, startIndex) => first(input, startIndex).SelectMany(m => second(input, startIndex + m).Select(m2 => m + m2));
        }

        internal static matcher sequenceMatcher(GenerexNoResultBase<T, TGenerex, TGenerexMatch>[] generexSequence, bool backward)
        {
            return generexSequence.Length == 0 ? emptyMatch : backward
                ? generexSequence.Reverse().Select(p => p._backwardMatcher).Aggregate(then)
                : generexSequence.Select(p => p._forwardMatcher).Aggregate(then);
        }

        private static ConstructorInfo _constructor;
        internal static TGenerex newGenerex(matcher forward, matcher backward)
        {
            if (_constructor == null)
            {
                var matcherType = typeof(matcher);
                _constructor = typeof(TGenerex).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { matcherType, matcherType }, null);
                if (_constructor == null)
                    throw new InvalidOperationException("The derived type does not declare a constructor with two parameters, each of type matcher.");
            }
            return (TGenerex) _constructor.Invoke(new object[] { forward, backward });
        }

        /// <summary>
        /// Returns a regular expression that matches a single element, no matter what it is (cf. "." in traditional regular expression syntax).
        /// </summary>
        public static TGenerex Any
        {
            get
            {
                if (_anyCache == null)
                    _anyCache = newGenerex(
                        (input, startIndex) => startIndex >= input.Length ? Generex.NoMatch : Generex.OneElementMatch,
                        (input, startIndex) => startIndex <= 0 ? Generex.NoMatch : Generex.NegativeOneElementMatch
                    );
                return _anyCache;
            }
        }
        private static TGenerex _anyCache;

        /// <summary>
        /// Returns a regular expression that always matches and returns a zero-width match.
        /// </summary>
        public static TGenerex Empty
        {
            get
            {
                if (_emptyCache == null)
                {
                    matcher zeroWidthMatch = (input, startIndex) => Generex.ZeroWidthMatch;
                    _emptyCache = newGenerex(zeroWidthMatch, zeroWidthMatch);
                }
                return _emptyCache;
            }
        }
        private static TGenerex _emptyCache;

        /// <summary>
        /// Returns a regular expression that matches the beginning of the input collection (cf. "^" in traditional regular expression syntax). Successful matches are always zero length.
        /// </summary>
        public static TGenerex Start
        {
            get
            {
                if (_startCache == null)
                {
                    matcher matcher = (input, startIndex) => startIndex != 0 ? Generex.NoMatch : Generex.ZeroWidthMatch;
                    _startCache = newGenerex(matcher, matcher);
                }
                return _startCache;
            }
        }
        private static TGenerex _startCache;

        /// <summary>
        /// Returns a regular expression that matches the end of the input collection (cf. "$" in traditional regular expression syntax). Successful matches are always zero length.
        /// </summary>
        public static TGenerex End
        {
            get
            {
                if (_endCache == null)
                {
                    matcher matcher = (input, startIndex) => startIndex != input.Length ? Generex.NoMatch : Generex.ZeroWidthMatch;
                    _endCache = newGenerex(matcher, matcher);
                }
                return _endCache;
            }
        }
        private static TGenerex _endCache;

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified other,
        /// and retains the match object generated by each match of the other regular expression.
        /// </summary>
        public Generex<T, TResult> Then<TResult>(Generex<T, TResult> other)
        {
#warning TODO: This method should return a TGenerexWithResult, not a Generex<T, TResult>
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
        /// <seealso cref="Or(IEqualityComparer{T},T[])"/>
        public TGenerex Or(params T[] elements) { return Or(EqualityComparer<T>.Default, elements); }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or any of the specified elements using the specified equality comparer (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        /// <seealso cref="Or(T[])"/>
        public TGenerex Or(IEqualityComparer<T> comparer, params T[] elements)
        {
            return Or(newGenerex(
                elementsMatcher(elements, comparer, backward: false),
                elementsMatcher(elements, comparer, backward: true)
            ));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified sequence of elements (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        public TGenerex Or(IEnumerable<T> elements) { return Or(EqualityComparer<T>.Default, elements.ToArray()); }
        /// <summary>
        /// Returns a regular expression that matches either this regular expression or any of the specified elements using the specified equality comparer (cf. "|" or "[...]" in traditional regular expression syntax).
        /// </summary>
        public TGenerex Or(IEqualityComparer<T> comparer, IEnumerable<T> elements) { return Or(comparer, elements.ToArray()); }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or a single element that satisfies the specified predicate (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public TGenerex Or(Predicate<T> predicate)
        {
            return Or(newGenerex(predicateMatcher(predicate, backward: false), predicateMatcher(predicate, backward: true)));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified sequence of regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        /// <example>
        /// <para>The following code:</para>
        /// <code>var regex = regex1.Or(regex2, regex3);</code>
        /// <para>generates a regular expression equivalent to <c>1|23</c>, NOT <c>1|2|3</c>.</para>
        /// </example>
        public TGenerex Or(params TGenerex[] other)
        {
            if (other.Length == 0)
                return (TGenerex) this;
            if (other.Length == 1)
                return Or(other[0]);
            return Or(newGenerex(sequenceMatcher(other, backward: false), sequenceMatcher(other, backward: true)));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified other regular expression (cf. "|" in traditional regular expression syntax).
        /// </summary>
        /// <remarks>
        /// <para>This overload is here even though an equivalent method is inherited from <see cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}"/> because without it, the following code:</para>
        /// <code>myGenerex.Or(myOtherGenerex);</code>
        /// <para>would call the <see cref="Or(Generex{T}[])"/> overload instead because method overload resolution prefers direct members over inherited ones.</para>
        /// </remarks>
        public new TGenerex Or(TGenerex other) { return base.Or(other); }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf. "?" in traditional regular expression syntax).
        /// </summary>
        public TGenerex OptionalGreedy() { return repeatBetween(0, 1, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Zero times is prioritised (cf. "??" in traditional regular expression syntax).
        /// </summary>
        public TGenerex Optional() { return repeatBetween(0, 1, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. More times are prioritised (cf. "*" in traditional regular expression syntax).
        /// </summary>
        public TGenerex RepeatGreedy() { return newGenerex(repeatInfinite(_forwardMatcher, true), repeatInfinite(_backwardMatcher, true)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. Fewer times are prioritised (cf. "*?" in traditional regular expression syntax).
        /// </summary>
        public TGenerex Repeat() { return newGenerex(repeatInfinite(_forwardMatcher, false), repeatInfinite(_backwardMatcher, false)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. More times are prioritised (cf. "{min,}" in traditional regular expression syntax).
        /// </summary>
        public TGenerex RepeatGreedy(int min) { return repeatMin(min, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. Fewer times are prioritised (cf. "{min,}?" in traditional regular expression syntax).
        /// </summary>
        public TGenerex Repeat(int min) { return repeatMin(min, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. More times are prioritised (cf. "{min,max}" in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public TGenerex RepeatGreedy(int min, int max) { return repeatBetween(min, max, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. Fewer times are prioritised (cf. "{min,max}?" in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public TGenerex Repeat(int min, int max) { return repeatBetween(min, max, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times (cf. "{times}" in traditional regular expression syntax).
        /// </summary>
        public TGenerex Times(int times)
        {
            if (times < 0) throw new ArgumentException("'times' cannot be negative.", "times");
            return repeatBetween(times, times, true);
        }
        /// <summary>
        /// Returns a regular expression that matches this regular expression one or more times, interspersed with a separator. Fewer times are prioritised.
        /// </summary>
        public TGenerex RepeatWithSeparator(TGenerex separator) { return Then(separator.Then(this).Repeat()); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression one or more times, interspersed with a separator. More times are prioritised.
        /// </summary>
        public TGenerex RepeatWithSeparatorGreedy(TGenerex separator) { return Then(separator.Then(this).RepeatGreedy()); }


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
        private TGenerex repeatInfinite(bool greedy)
        {
            return newGenerex(repeatInfinite(_forwardMatcher, greedy), repeatInfinite(_backwardMatcher, greedy));
        }

        /// <summary>
        /// Generates a matcher that matches this regular expression at least a minimum number of times.
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private TGenerex repeatMin(int min, bool greedy)
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
        private TGenerex repeatBetween(int min, int max, bool greedy)
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
            return newGenerex(rm.ForwardMatcher, rm.BackwardMatcher);
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

        /// <summary>Turns the current regular expression into a zero-width negative look-ahead assertion.</summary>
        public TGenerex LookAheadNegative() { return lookNegative(behind: false, defaultMatch: Generex.ZeroWidthMatch); }
        /// <summary>Turns the current regular expression into a zero-width negative look-ahead assertion.</summary>
        public TGenerex LookBehindNegative() { return lookNegative(behind: true, defaultMatch: Generex.ZeroWidthMatch); }

        /// <summary>Returns a successful zero-width match.</summary>
        internal static IEnumerable<int> emptyMatch(T[] input, int startIndex) { return Generex.ZeroWidthMatch; }

        /// <summary>Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TResult">Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">Function to process a regular expression match.</param>
        public Generex<T, TResult> Process<TResult>(Func<TGenerexMatch, TResult> selector)
        {
            return new Generex<T, TResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => new GenerexMatchInfo<TResult>(selector(createMatch(input, startIndex, m)), m)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => new GenerexMatchInfo<TResult>(selector(createBackwardsMatch(input, startIndex, m)), m))
            );
        }

        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static TGenerex Recursive(Func<TGenerex, TGenerex> generator)
        {
            if (generator == null)
                throw new ArgumentNullException("generator");

            matcher recursiveForward = null, recursiveBackward = null;

            // Note the following *must* be lambdas so that they capture the above *variables* (which are modified afterwards), not their current value (which would be null)
            var carrier = newGenerex(
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
