using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <summary>Returns <paramref name="match"/>.</summary>
        protected sealed override int getLength(int match) { return match; }
        /// <summary>Returns the sum of <paramref name="match"/> and <paramref name="extra"/>.</summary>
        protected sealed override int add(int match, int extra) { return match + extra; }
        /// <summary>Returns zero.</summary>
        protected sealed override int setZero(int match) { return 0; }
        internal sealed override TGenerexMatch createMatch(T[] input, int index, int match) { return createNoResultMatch(input, index, match); }
        internal sealed override TGenerexMatch createBackwardsMatch(T[] input, int index, int match) { return createNoResultMatch(input, index + match, -match); }

        /// <summary>Instantiates a <see cref="GenerexMatch{T}"/> object from an index and length.</summary>
        /// <param name="input">Original input array that was matched against.</param>
        /// <param name="index">Start index of the match.</param>
        /// <param name="matchLength">Length of the match.</param>
        protected abstract TGenerexMatch createNoResultMatch(T[] input, int index, int matchLength);

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        protected GenerexNoResultBase(IEqualityComparer<T> comparer, T[] elements)
            : base(
                elementsMatcher(elements, comparer, backward: false),
                elementsMatcher(elements, comparer, backward: true)) { }

        /// <summary>
        /// Instantiates a regular expression that matches a single element that satisfies the given predicate (cf. <c>[...]</c> in traditional regular expression syntax).
        /// </summary>
        /// <param name="predicate">The predicate that identifies matching elements.</param>
        protected GenerexNoResultBase(Predicate<T> predicate)
            : base(
                forwardPredicateMatcher(predicate),
                backwardPredicateMatcher(predicate)) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive regular expressions.
        /// </summary>
        protected GenerexNoResultBase(GenerexNoResultBase<T, TGenerex, TGenerexMatch>[] generexSequence)
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

        internal static matcher forwardPredicateMatcher(Predicate<T> predicate)
        {
            return (input, startIndex) => startIndex >= input.Length || !predicate(input[startIndex]) ? Generex.NoMatch : Generex.OneElementMatch;
        }

        internal static matcher backwardPredicateMatcher(Predicate<T> predicate)
        {
            return (input, startIndex) => startIndex <= 0 || !predicate(input[startIndex - 1]) ? Generex.NoMatch : Generex.NegativeOneElementMatch;
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

        /// <summary>
        /// Returns a regular expression that matches a single element, no matter what it is (cf. <c>.</c> in traditional regular expression syntax).
        /// </summary>
        /// <seealso cref="Generex.CreateAnyGenerex"/>
        public static TGenerex Any
        {
            get
            {
                if (_anyCache == null)
                    _anyCache = Constructor(
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
        /// <seealso cref="Generex.CreateEmptyGenerex"/>
        public static TGenerex Empty
        {
            get
            {
                if (_emptyCache == null)
                {
                    matcher zeroWidthMatch = (input, startIndex) => Generex.ZeroWidthMatch;
                    _emptyCache = Constructor(zeroWidthMatch, zeroWidthMatch);
                }
                return _emptyCache;
            }
        }
        private static TGenerex _emptyCache;

        /// <summary>
        /// Returns a regular expression that matches the beginning of the input collection (cf. <c>^</c> in traditional regular expression syntax). Successful matches are always zero length.
        /// </summary>
        /// <seealso cref="Generex.CreateStartGenerex"/>
        public static TGenerex Start
        {
            get
            {
                if (_startCache == null)
                {
                    matcher matcher = (input, startIndex) => startIndex != 0 ? Generex.NoMatch : Generex.ZeroWidthMatch;
                    _startCache = Constructor(matcher, matcher);
                }
                return _startCache;
            }
        }
        private static TGenerex _startCache;

        /// <summary>
        /// Returns a regular expression that matches the end of the input collection (cf. <c>$</c> in traditional regular expression syntax). Successful matches are always zero length.
        /// </summary>
        /// <seealso cref="Generex.CreateEndGenerex"/>
        public static TGenerex End
        {
            get
            {
                if (_endCache == null)
                {
                    matcher matcher = (input, startIndex) => startIndex != input.Length ? Generex.NoMatch : Generex.ZeroWidthMatch;
                    _endCache = Constructor(matcher, matcher);
                }
                return _endCache;
            }
        }
        private static TGenerex _endCache;

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified other,
        /// and retains the match object generated by each match of the other regular expression.
        /// </summary>
        protected TGenerexWithResult then<TGenerexWithResult, TGenerexWithResultMatch, TResult>(TGenerexWithResult other)
            where TGenerexWithResult : GenerexWithResultBase<T, TResult, TGenerexWithResult, TGenerexWithResultMatch>
            where TGenerexWithResultMatch : GenerexMatch<T, TResult>
        {
            return GenerexWithResultBase<T, TResult, TGenerexWithResult, TGenerexWithResultMatch>.Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m => other._forwardMatcher(input, startIndex + m).Select(m2 => m2.Add(m))),
                (input, startIndex) => other._backwardMatcher(input, startIndex).SelectMany(m2 => _backwardMatcher(input, startIndex + m2.Length).Select(m => m2.Add(m)))
            );
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified sequence of elements (cf. <c>|</c> or <c>[...]</c> in traditional regular expression syntax).
        /// </summary>
        /// <example>
        /// <para>The following code:</para>
        /// <code>var regex = new Generex&lt;char&gt;('a', 'b').Or('c', 'd');</code>
        /// <para>is equivalent to the regular expression <c>ab|cd</c>, NOT <c>ab|c|d</c>. For the latter, use <see cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.Ors(TGenerex[])"/>.</para>
        /// </example>
        /// <seealso cref="Or(IEqualityComparer{T},T[])"/>
        public TGenerex Or(params T[] elements) { return Or(EqualityComparer<T>.Default, elements); }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified sequence of elements using the specified equality comparer (cf. <c>|</c> or <c>[...]</c> in traditional regular expression syntax).
        /// </summary>
        /// <seealso cref="Or(T[])"/>
        public TGenerex Or(IEqualityComparer<T> comparer, params T[] elements)
        {
            return Or(Constructor(
                elementsMatcher(elements, comparer, backward: false),
                elementsMatcher(elements, comparer, backward: true)
            ));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified sequence of elements (cf. <c>|</c> or <c>[...]</c> in traditional regular expression syntax).
        /// </summary>
        public TGenerex Or(IEnumerable<T> elements) { return Or(EqualityComparer<T>.Default, elements.ToArray()); }
        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified sequence of elements using the specified equality comparer (cf. <c>|</c> or <c>[...]</c> in traditional regular expression syntax).
        /// </summary>
        public TGenerex Or(IEqualityComparer<T> comparer, IEnumerable<T> elements) { return Or(comparer, elements.ToArray()); }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or a single element that satisfies the specified predicate (cf. <c>|</c> in traditional regular expression syntax).
        /// </summary>
        public TGenerex Or(Predicate<T> predicate)
        {
            return Or(Constructor(forwardPredicateMatcher(predicate), backwardPredicateMatcher(predicate)));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified sequence of regular expressions (cf. <c>|</c> in traditional regular expression syntax).
        /// </summary>
        /// <example>
        /// <para>The following code:</para>
        /// <code>var regex = regex1.Or(regex2, regex3);</code>
        /// <para>generates a regular expression equivalent to <c>1|23</c>, NOT <c>1|2|3</c>. For the latter, use <see cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.Ors(TGenerex[])"/>.</para>
        /// </example>
        public TGenerex Or(params TGenerex[] other)
        {
            if (other.Length == 0)
                return (TGenerex) this;
            if (other.Length == 1)
                return Or(other[0]);
            return Or(Constructor(sequenceMatcher(other, backward: false), sequenceMatcher(other, backward: true)));
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified other regular expression (cf. <c>|</c> in traditional regular expression syntax).
        /// </summary>
        /// <remarks>
        /// <para>This overload is here even though an equivalent method is inherited from <see cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}"/> because without it, the following code:</para>
        /// <code>myGenerex.Or(myOtherGenerex);</code>
        /// <para>would call the <see cref="Or(TGenerex[])"/> overload instead because method overload resolution prefers direct members over inherited ones.</para>
        /// </remarks>
        public new TGenerex Or(TGenerex other) { return base.Or(other); }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf. <c>?</c> in traditional regular expression syntax).
        /// </summary>
        public TGenerex OptionalGreedy() { return repeatBetween(0, 1, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Zero times is prioritised (cf. <c>??</c> in traditional regular expression syntax).
        /// </summary>
        public TGenerex Optional() { return repeatBetween(0, 1, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. More times are prioritised (cf. <c>*</c> in traditional regular expression syntax).
        /// </summary>
        public TGenerex RepeatGreedy() { return Constructor(repeatInfinite(_forwardMatcher, true), repeatInfinite(_backwardMatcher, true)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. Fewer times are prioritised (cf. <c>*?</c> in traditional regular expression syntax).
        /// </summary>
        public TGenerex Repeat() { return Constructor(repeatInfinite(_forwardMatcher, false), repeatInfinite(_backwardMatcher, false)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. More times are prioritised (cf. <c>{min,}</c> in traditional regular expression syntax).
        /// </summary>
        public TGenerex RepeatGreedy(int min) { return repeatMin(min, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. Fewer times are prioritised (cf. <c>{min,}?</c> in traditional regular expression syntax).
        /// </summary>
        public TGenerex Repeat(int min) { return repeatMin(min, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. More times are prioritised (cf. <c>{min,max}</c> in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public TGenerex RepeatGreedy(int min, int max) { return repeatBetween(min, max, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. Fewer times are prioritised (cf. <c>{min,max}?</c> in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public TGenerex Repeat(int min, int max) { return repeatBetween(min, max, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times (cf. <c>{times}</c> in traditional regular expression syntax).
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
            return Constructor(repeatInfinite(_forwardMatcher, greedy), repeatInfinite(_backwardMatcher, greedy));
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
            return Constructor(rm.ForwardMatcher, rm.BackwardMatcher);
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

        /// <summary>Turns the current regular expression into a zero-width negative look-ahead assertion (cf. <c>(?!...)</c> in traditional regular expression syntax).</summary>
        public TGenerex LookAheadNegative() { return lookNegative(behind: false, defaultMatch: Generex.ZeroWidthMatch); }
        /// <summary>Turns the current regular expression into a zero-width negative look-behind assertion (cf. <c>(?&lt;!...)</c> in traditional regular expression syntax).</summary>
        public TGenerex LookBehindNegative() { return lookNegative(behind: true, defaultMatch: Generex.ZeroWidthMatch); }

        /// <summary>Returns a successful zero-width match.</summary>
        protected static IEnumerable<int> emptyMatch(T[] input, int startIndex) { return Generex.ZeroWidthMatch; }

        /// <summary>Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TGenerexWithResult">Generex type to return (for example, <see cref="Generex{T,TResult}"/>).</typeparam>
        /// <typeparam name="TGenerexWithResultMatch">Generex match type that corresponds to <typeparamref name="TGenerexWithResult"/></typeparam>
        /// <typeparam name="TResult">Type of the object returned by <paramref name="selector"/>, which is a result object associated with each match of the regular expression.</typeparam>
        /// <param name="selector">Function to process a regular expression match.</param>
        protected TGenerexWithResult process<TGenerexWithResult, TGenerexWithResultMatch, TResult>(Func<TGenerexMatch, TResult> selector)
            where TGenerexWithResult : GenerexWithResultBase<T, TResult, TGenerexWithResult, TGenerexWithResultMatch>
            where TGenerexWithResultMatch : GenerexMatch<T, TResult>
        {
            return GenerexWithResultBase<T, TResult, TGenerexWithResult, TGenerexWithResultMatch>.Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => new LengthAndResult<TResult>(selector(createMatch(input, startIndex, m)), m)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => new LengthAndResult<TResult>(selector(createBackwardsMatch(input, startIndex, m)), m))
            );
        }

        /// <summary>Returns a regular expression that matches a single element which is not equal to the specified element.</summary>
        /// <param name="element">List of elements excluded from matching.</param>
        public static TGenerex Not(T element) { return Not(EqualityComparer<T>.Default, element); }

        /// <summary>Returns a regular expression that matches a single element which is not equal to the specified element.</summary>
        /// <param name="element">List of elements excluded from matching.</param>
        /// <param name="comparer">Equality comparer to use.</param>
        public static TGenerex Not(IEqualityComparer<T> comparer, T element)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            return Constructor(
                (input, startIndex) => startIndex >= input.Length || comparer.Equals(input[startIndex], element) ? Generex.NoMatch : Generex.OneElementMatch,
                (input, startIndex) => startIndex <= 0 || comparer.Equals(input[startIndex - 1], element) ? Generex.NoMatch : Generex.NegativeOneElementMatch
            );
        }

        /// <summary>Returns a regular expression that matches a single element which is none of the specified elements.</summary>
        /// <param name="elements">List of elements excluded from matching.</param>
        public static TGenerex Not(params T[] elements) { return Not(EqualityComparer<T>.Default, elements); }

        /// <summary>Returns a regular expression that matches a single element which is none of the specified elements.</summary>
        /// <param name="elements">List of elements excluded from matching.</param>
        /// <param name="comparer">Equality comparer to use.</param>
        public static TGenerex Not(IEqualityComparer<T> comparer, params T[] elements)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");
            if (elements == null)
                throw new ArgumentNullException("elements");

            if (elements.Length == 0)
                return Any;

            if (elements.Length == 1)
                return Not(comparer, elements[0]);

            return Constructor(
                (input, startIndex) => startIndex >= input.Length || elements.Any(e => comparer.Equals(input[startIndex], e)) ? Generex.NoMatch : Generex.OneElementMatch,
                (input, startIndex) => startIndex <= 0 || elements.Any(e => comparer.Equals(input[startIndex - 1], e)) ? Generex.NoMatch : Generex.NegativeOneElementMatch
            );
        }

        /// <summary>
        /// Returns a regular expression that only matches if the subarray matched by this regular expression also contains a match for the specified other regular expression,
        /// and if so, associates each match of this regular expression with the result object returned by the other regular expression’s first match.
        /// </summary>
        /// <typeparam name="TOtherResult">Type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">The type of the match object for the other regular expression. (This is either <see cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <param name="other">A regular expression which must match the subarray matched by this regular expression.</param>
        public TOtherGenerex And<TOtherResult, TOtherGenerex, TOtherGenerexMatch>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch>(subarray => other.Match(subarray));
        }

        /// <summary>
        /// Returns a regular expression that only matches if the subarray matched by this regular expression also fully matches the specified other regular expression,
        /// and if so, associates each match of this regular expression with the result object returned by the other regular expression’s match.
        /// </summary>
        /// <typeparam name="TOtherResult">Type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">The type of the match object for the other regular expression. (This is either <see cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <param name="other">A regular expression which must match the subarray matched by this regular expression.</param>
        public TOtherGenerex AndExact<TOtherResult, TOtherGenerex, TOtherGenerexMatch>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch>(subarray => other.MatchExact(subarray));
        }

        /// <summary>
        /// Returns a regular expression that only matches if the subarray matched by this regular expression also contains a match for the specified other regular expression,
        /// and if so, associates each match of this regular expression with the result object returned by the other regular expression’s first match found when matching backwards
        /// (starting at the end of the matched subarray).
        /// </summary>
        /// <typeparam name="TOtherResult">Type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">The type of the match object for the other regular expression. (This is either <see cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <param name="other">A regular expression which must match the subarray matched by this regular expression.</param>
        public TOtherGenerex AndReverse<TOtherResult, TOtherGenerex, TOtherGenerexMatch>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch>(subarray => other.MatchReverse(subarray));
        }

        private TOtherGenerex and<TResult, TOtherGenerex, TOtherGenerexMatch>(Func<T[], TOtherGenerexMatch> innerMatch)
            where TOtherGenerex : GenerexWithResultBase<T, TResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TResult>
        {
            return GenerexWithResultBase<T, TResult, TOtherGenerex, TOtherGenerexMatch>.Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m =>
                {
                    var subarray = input.Subarray(startIndex, getLength(m));
                    var submatch = innerMatch(subarray);
                    return submatch == null ? Enumerable.Empty<LengthAndResult<TResult>>() : new[] { new LengthAndResult<TResult>(submatch.Result, m) };
                }),
                (input, startIndex) => _backwardMatcher(input, startIndex).SelectMany(m =>
                {
                    var length = getLength(m);
                    var subarray = input.Subarray(startIndex + length, -length);
                    var submatch = innerMatch(subarray);
                    return submatch == null ? Enumerable.Empty<LengthAndResult<TResult>>() : new[] { new LengthAndResult<TResult>(submatch.Result, m) };
                })
            );
        }
    }
}
