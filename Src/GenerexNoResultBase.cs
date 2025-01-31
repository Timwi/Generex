﻿namespace RT.Generexes
{
    /// <summary>
    ///     Abstract base class for <see cref="Generex{T}"/> and <see cref="Stringerex"/>.</summary>
    /// <typeparam name="T">
    ///     Type of the objects in the collection.</typeparam>
    /// <typeparam name="TGenerex">
    ///     The derived type. (Pass the type itself recursively.)</typeparam>
    /// <typeparam name="TGenerexMatch">
    ///     Type describing a match of a regular expression.</typeparam>
    public abstract class GenerexNoResultBase<T, TGenerex, TGenerexMatch> : GenerexBase<T, int, TGenerex, TGenerexMatch>
        where TGenerex : GenerexNoResultBase<T, TGenerex, TGenerexMatch>
        where TGenerexMatch : GenerexMatch<T>
    {
        /// <summary>Returns <paramref name="match"/>.</summary>
        internal sealed override int getLength(int match) { return match; }
        /// <summary>Returns the sum of <paramref name="match"/> and <paramref name="extra"/>.</summary>
        protected sealed override int add(int match, int extra) { return match + extra; }
        /// <summary>Returns zero.</summary>
        protected sealed override int setZero(int match) { return 0; }
        internal sealed override TGenerexMatch createMatch(T[] input, int index, int match) { return createNoResultMatch(input, index, match); }
        internal sealed override TGenerexMatch createBackwardsMatch(T[] input, int index, int match) { return createNoResultMatch(input, index + match, -match); }

        /// <summary>
        ///     Instantiates a <see cref="GenerexMatch{T}"/> object from an index and length.</summary>
        /// <param name="input">
        ///     Original input array that was matched against.</param>
        /// <param name="index">
        ///     Start index of the match.</param>
        /// <param name="matchLength">
        ///     Length of the match.</param>
        protected abstract TGenerexMatch createNoResultMatch(T[] input, int index, int matchLength);

        /// <summary>
        ///     Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality
        ///     comparer.</summary>
        protected GenerexNoResultBase(T[] elements, IEqualityComparer<T> comparer)
            : base(
                elementsMatcher(elements, comparer, backward: false),
                elementsMatcher(elements, comparer, backward: true)) { }

        /// <summary>
        ///     Instantiates a regular expression that matches a single element that satisfies the given predicate (cf.
        ///     <c>[...]</c> in traditional regular expression syntax).</summary>
        /// <param name="predicate">
        ///     The predicate that identifies matching elements.</param>
        protected GenerexNoResultBase(Predicate<T> predicate)
            : base(
                forwardPredicateMatcher(predicate),
                backwardPredicateMatcher(predicate)) { }

        /// <summary>Instantiates a regular expression that matches a sequence of consecutive regular expressions.</summary>
        protected GenerexNoResultBase(GenerexNoResultBase<T, TGenerex, TGenerexMatch>[] generexSequence)
            : base(
                sequenceMatcher(generexSequence, backward: false),
                sequenceMatcher(generexSequence, backward: true)) { }

        internal GenerexNoResultBase(matcher forward, matcher backward) : base(forward, backward) { }

        /// <summary>Generates a matcher that matches a sequence of specific elements either fully or not at all.</summary>
        private static matcher elementsMatcher(T[] elements, IEqualityComparer<T> comparer, bool backward)
        {
            if (elements == null)
                throw new ArgumentNullException("elements");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            if (elements.Length == 0)
                return emptyMatch;

            if (elements.Length == 1)
                return elementMatcher(elements[0], comparer, backward);

            if (backward)
                return (input, startIndex) => startIndex >= elements.Length && input.SubarrayEquals(startIndex - elements.Length, elements, comparer) ? new int[] { -elements.Length } : Generex.NoMatch;
            else
                return (input, startIndex) => startIndex <= input.Length - elements.Length && input.SubarrayEquals(startIndex, elements, comparer) ? new int[] { elements.Length } : Generex.NoMatch;
        }

        private static matcher elementMatcher(T element, IEqualityComparer<T> comparer, bool backward)
        {
            if (backward)
                return (input, startIndex) => startIndex > 0 && comparer.Equals(input[startIndex - 1], element) ? Generex.NegativeOneElementMatch : Generex.NoMatch;
            else
                return (input, startIndex) => startIndex < input.Length && comparer.Equals(input[startIndex], element) ? Generex.OneElementMatch : Generex.NoMatch;
        }

        internal static matcher forwardPredicateMatcher(Predicate<T> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            return (input, startIndex) => startIndex >= input.Length || !predicate(input[startIndex]) ? Generex.NoMatch : Generex.OneElementMatch;
        }

        internal static matcher backwardPredicateMatcher(Predicate<T> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            return (input, startIndex) => startIndex <= 0 || !predicate(input[startIndex - 1]) ? Generex.NoMatch : Generex.NegativeOneElementMatch;
        }

        /// <summary>
        ///     Generates a matcher that matches the <paramref name="first"/> regular expression followed by the <paramref
        ///     name="second"/> regular expression.</summary>
        internal static matcher then(matcher first, matcher second)
        {
            return (input, startIndex) => first(input, startIndex).SelectMany(m => second(input, startIndex + m).Select(m2 => m + m2));
        }

        internal static matcher sequenceMatcher(GenerexNoResultBase<T, TGenerex, TGenerexMatch>[] generexSequence, bool backward)
        {
            if (generexSequence == null)
                throw new ArgumentNullException("generexSequence");
            return generexSequence.Length == 0 ? emptyMatch : backward
                ? generexSequence.Reverse().Select(p => p._backwardMatcher).Aggregate(then)
                : generexSequence.Select(p => p._forwardMatcher).Aggregate(then);
        }

        /// <summary>
        ///     Returns a regular expression that matches a single element, no matter what it is (cf. <c>.</c> in traditional
        ///     regular expression syntax).</summary>
        /// <seealso cref="Generex.CreateAnyGenerex"/>
        public static TGenerex Any
        {
            get
            {
                return _anyCache ?? (_anyCache = Constructor(
                    (input, startIndex) => startIndex >= input.Length ? Generex.NoMatch : Generex.OneElementMatch,
                    (input, startIndex) => startIndex <= 0 ? Generex.NoMatch : Generex.NegativeOneElementMatch
                ));
            }
        }
        private static TGenerex _anyCache;

        /// <summary>
        ///     Returns a regular expression that matches any number of elements, no matter what they are; fewer are
        ///     prioritized (cf. <c>.*?</c> in traditional regular expression syntax).</summary>
        /// <seealso cref="Generex.CreateAnythingGenerex"/>
        public static TGenerex Anything { get { return _anythingCache ?? (_anythingCache = Any.Repeat()); } }
        private static TGenerex _anythingCache;

        /// <summary>
        ///     Returns a regular expression that matches any number of elements, no matter what they are; more are
        ///     prioritized (cf. <c>.*</c> in traditional regular expression syntax).</summary>
        /// <seealso cref="Generex.CreateAnythingGreedyGenerex"/>
        public static TGenerex AnythingGreedy { get { return _anythingGreedyCache ?? (_anythingGreedyCache = Any.RepeatGreedy()); } }
        private static TGenerex _anythingGreedyCache;

        /// <summary>
        ///     Returns a regular expression that always matches and returns a zero-width match.</summary>
        /// <seealso cref="Generex.CreateEmptyGenerex"/>
        public static TGenerex Empty
        {
            get
            {
                if (_emptyCache != null)
                    return _emptyCache;
                matcher zeroWidthMatch = (input, startIndex) => Generex.ZeroWidthMatch;
                return (_emptyCache = Constructor(zeroWidthMatch, zeroWidthMatch));
            }
        }
        private static TGenerex _emptyCache;

        /// <summary>
        ///     Returns a regular expression that matches the beginning of the input collection (cf. <c>^</c> in traditional
        ///     regular expression syntax). Successful matches are always zero length.</summary>
        /// <seealso cref="Generex.CreateStartGenerex"/>
        public static TGenerex Start
        {
            get
            {
                if (_startCache != null)
                    return _startCache;
                matcher matcher = (input, startIndex) => startIndex != 0 ? Generex.NoMatch : Generex.ZeroWidthMatch;
                return (_startCache = Constructor(matcher, matcher));
            }
        }
        private static TGenerex _startCache;

        /// <summary>
        ///     Returns a regular expression that matches the end of the input collection (cf. <c>$</c> in traditional regular
        ///     expression syntax). Successful matches are always zero length.</summary>
        /// <seealso cref="Generex.CreateEndGenerex"/>
        public static TGenerex End
        {
            get
            {
                if (_endCache != null)
                    return _endCache;
                matcher matcher = (input, startIndex) => startIndex != input.Length ? Generex.NoMatch : Generex.ZeroWidthMatch;
                return (_endCache = Constructor(matcher, matcher));
            }
        }
        private static TGenerex _endCache;

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, followed by the specified other, and
        ///     retains the result object generated by each match of the other regular expression.</summary>
        public TOtherGenerex Then<TOtherGenerex, TOtherGenerexMatch, TOtherResult>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            if (other == null)
                throw new ArgumentNullException("other");
            return GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>.Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m => other._forwardMatcher(input, startIndex + m).Select(m2 => m2.Add(m))),
                (input, startIndex) => other._backwardMatcher(input, startIndex).SelectMany(m2 => _backwardMatcher(input, startIndex + m2.Length).Select(m => m2.Add(m)))
            );
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then uses a specified <paramref
        ///     name="selector"/> to create a new regular expression from the match; then attempts to match the new regular
        ///     expression and throws an exception if that regular expression fails to match. The new regular expression’s
        ///     result object replaces the current one’s.</summary>
        /// <param name="selector">
        ///     The selector that generates a new regular expression, which is expected to match after the current one.</param>
        /// <param name="exceptionGenerator">
        ///     A selector which, in case of no match, generates the exception object to be thrown.</param>
        /// <returns>
        ///     The resulting regular expression.</returns>
        /// <remarks>
        ///     Regular expressions created by this method cannot match backwards. The full set of affected methods is listed
        ///     at <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then{TOtherGenerex, TOtherMatch,
        ///     TOtherGenerexMatch}(Func{TGenerexMatch, GenerexBase{T, TOtherMatch, TOtherGenerex, TOtherGenerexMatch}})"/>.</remarks>
        public TOtherGenerex ThenExpect<TOtherGenerex, TOtherResult, TOtherGenerexMatch>(Func<TGenerexMatch, GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>> selector, Func<TGenerexMatch, Exception> exceptionGenerator)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            if (selector == null)
                throw new ArgumentNullException("selector");
            return Then(m => selector(m).expect(() => exceptionGenerator(m)));
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then attempts to match the specified other
        ///     regular expression and throws an exception if the second regular expression fails to match.</summary>
        /// <typeparam name="TOtherGenerex">
        ///     The type of <paramref name="expectation"/>. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the match object <paramref name="expectation"/>. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="expectation"/>.</typeparam>
        /// <param name="expectation">
        ///     The regular expression that is expected to match after the current one.</param>
        /// <param name="exceptionGenerator">
        ///     A selector which, in case of no match, generates the exception object to be thrown.</param>
        /// <returns>
        ///     The resulting regular expression.</returns>
        /// <remarks>
        ///     Regular expressions created by this method cannot match backwards. The full set of affected methods is listed
        ///     at <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then{TOtherGenerex, TOtherMatch,
        ///     TOtherGenerexMatch}(Func{TGenerexMatch, GenerexBase{T, TOtherMatch, TOtherGenerex, TOtherGenerexMatch}})"/>.</remarks>
        public TOtherGenerex ThenExpect<TOtherGenerex, TOtherGenerexMatch, TOtherResult>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> expectation, Func<TGenerexMatch, Exception> exceptionGenerator)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            if (expectation == null)
                throw new ArgumentNullException("expectation");
            return Then(m => expectation.expect(() => exceptionGenerator(m)));
        }

        /// <summary>
        ///     Returns a regular expression that matches either this regular expression or the specified single element (cf.
        ///     <c>|</c> in traditional regular expression syntax).</summary>
        /// <seealso cref="Or(T,IEqualityComparer{T})"/>
        /// <seealso cref="Or(Predicate{T})"/>
        public TGenerex Or(T element)
        {
            return Or(element, EqualityComparer<T>.Default);
        }

        /// <summary>
        ///     Returns a regular expression that matches either this regular expression or the specified single element using
        ///     the specified equality comparer (cf. <c>|</c> in traditional regular expression syntax).</summary>
        /// <seealso cref="Or(T)"/>
        /// <seealso cref="Or(IEqualityComparer{T},IEnumerable{T})"/>
        /// <seealso cref="Or(Predicate{T})"/>
        public TGenerex Or(T element, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");
            return Or(Constructor(
                elementMatcher(element, comparer, backward: false),
                elementMatcher(element, comparer, backward: true)
            ));
        }

        /// <summary>
        ///     Returns a regular expression that matches either this regular expression or the specified sequence of elements
        ///     (cf. <c>|</c> or <c>[...]</c> in traditional regular expression syntax).</summary>
        /// <seealso cref="Or(T,IEqualityComparer{T})"/>
        /// <seealso cref="Or(Predicate{T})"/>
        public TGenerex Or(IEnumerable<T> elements)
        {
            if (elements == null)
                throw new ArgumentNullException("elements");
            return Or((elements as T[]) ?? elements.ToArray());
        }

        /// <summary>
        ///     Returns a regular expression that matches either this regular expression or the specified sequence of elements
        ///     using the specified equality comparer (cf. <c>|</c> or <c>[...]</c> in traditional regular expression syntax).</summary>
        /// <seealso cref="Or(T,IEqualityComparer{T})"/>
        /// <seealso cref="Or(Predicate{T})"/>
        public TGenerex Or(IEqualityComparer<T> comparer, IEnumerable<T> elements)
        {
            if (elements == null)
                throw new ArgumentNullException("elements");
            return Or(comparer, (elements as T[]) ?? elements.ToArray());
        }

        /// <summary>
        ///     Returns a regular expression that matches either this regular expression or the specified sequence of elements
        ///     (cf. <c>|</c> or <c>[...]</c> in traditional regular expression syntax).</summary>
        /// <seealso cref="Or(T,IEqualityComparer{T})"/>
        /// <seealso cref="Or(Predicate{T})"/>
        public TGenerex Or(params T[] elements)
        {
            if (elements == null)
                throw new ArgumentNullException("elements");
            return Or(EqualityComparer<T>.Default, elements);
        }

        /// <summary>
        ///     Returns a regular expression that matches either this regular expression or the specified sequence of elements
        ///     using the specified equality comparer (cf. <c>|</c> or <c>[...]</c> in traditional regular expression syntax).</summary>
        /// <seealso cref="Or(T,IEqualityComparer{T})"/>
        /// <seealso cref="Or(Predicate{T})"/>
        public TGenerex Or(IEqualityComparer<T> comparer, params T[] elements)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");
            if (elements == null)
                throw new ArgumentNullException("elements");
            return Or(Constructor(
                elementsMatcher(elements, comparer, false),
                elementsMatcher(elements, comparer, true)
            ));
        }

        /// <summary>
        ///     Returns a regular expression that matches either this regular expression or a single element that satisfies
        ///     the specified predicate (cf. <c>|</c> in traditional regular expression syntax).</summary>
        /// <seealso cref="Or(T,IEqualityComparer{T})"/>
        /// <seealso cref="Or(IEqualityComparer{T},IEnumerable{T})"/>
        public TGenerex Or(Predicate<T> predicate)
        {
            return Or(Constructor(forwardPredicateMatcher(predicate), backwardPredicateMatcher(predicate)));
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf.
        ///     <c>?</c> in traditional regular expression syntax).</summary>
        public TGenerex OptionalGreedy() { return repeatBetween(0, 1, true); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero times or once. Zero times is
        ///     prioritised (cf. <c>??</c> in traditional regular expression syntax).</summary>
        public TGenerex Optional() { return repeatBetween(0, 1, false); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero or more times. More times are
        ///     prioritised (cf. <c>*</c> in traditional regular expression syntax).</summary>
        public TGenerex RepeatGreedy() { return Constructor(repeatInfinite(_forwardMatcher, true), repeatInfinite(_backwardMatcher, true)); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero or more times. Fewer times are
        ///     prioritised (cf. <c>*?</c> in traditional regular expression syntax).</summary>
        public TGenerex Repeat() { return Constructor(repeatInfinite(_forwardMatcher, false), repeatInfinite(_backwardMatcher, false)); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression the specified number of times or more. More
        ///     times are prioritised (cf. <c>{min,}</c> in traditional regular expression syntax).</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="min"/> is negative.</exception>
        public TGenerex RepeatGreedy(int min) { return repeatMin(min, true); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression the specified number of times or more. Fewer
        ///     times are prioritised (cf. <c>{min,}?</c> in traditional regular expression syntax).</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="min"/> is negative.</exception>
        public TGenerex Repeat(int min) { return repeatMin(min, false); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression any number of times within specified
        ///     boundaries. More times are prioritised (cf. <c>{min,max}</c> in traditional regular expression syntax).</summary>
        /// <param name="min">
        ///     Minimum number of times to match.</param>
        /// <param name="max">
        ///     Maximum number of times to match.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="min"/> is negative.</exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="max"/> is smaller than <paramref name="min"/>.</exception>
        public TGenerex RepeatGreedy(int min, int max) { return repeatBetween(min, max, true); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression any number of times within specified
        ///     boundaries. Fewer times are prioritised (cf. <c>{min,max}?</c> in traditional regular expression syntax).</summary>
        /// <param name="min">
        ///     Minimum number of times to match.</param>
        /// <param name="max">
        ///     Maximum number of times to match.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="min"/> is negative.</exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="max"/> is smaller than <paramref name="min"/>.</exception>
        public TGenerex Repeat(int min, int max) { return repeatBetween(min, max, false); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression the specified number of times (cf.
        ///     <c>{times}</c> in traditional regular expression syntax).</summary>
        /// <param name="times">
        ///     A non-negative number specifying the number of repetitions of the regular expression.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="times"/> is negative.</exception>
        public TGenerex Times(int times)
        {
            if (times < 0) throw new ArgumentOutOfRangeException("'times' cannot be negative.", "times");
            return repeatBetween(times, times, true);
        }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression one or more times, interspersed with a
        ///     separator. Fewer times are prioritised.</summary>
        public TGenerex RepeatWithSeparator(TGenerex separator) { return Then(separator.Then(this).Repeat()); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression one or more times, interspersed with a
        ///     separator. More times are prioritised.</summary>
        public TGenerex RepeatWithSeparatorGreedy(TGenerex separator) { return Then(separator.Then(this).RepeatGreedy()); }


        /// <summary>
        ///     Generates a matcher that matches the specified matcher zero or more times.</summary>
        /// <param name="inner">
        ///     Inner matcher.</param>
        /// <param name="greedy">
        ///     If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
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
        ///     Generates a regular expression that matches this regular expression zero or more times.</summary>
        /// <param name="greedy">
        ///     If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private TGenerex repeatInfinite(bool greedy)
        {
            return Constructor(repeatInfinite(_forwardMatcher, greedy), repeatInfinite(_backwardMatcher, greedy));
        }

        /// <summary>
        ///     Generates a matcher that matches this regular expression at least a minimum number of times.</summary>
        /// <param name="min">
        ///     Minimum number of times to match.</param>
        /// <param name="greedy">
        ///     If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="min"/> is negative.</exception>
        private TGenerex repeatMin(int min, bool greedy)
        {
            if (min < 0) throw new ArgumentOutOfRangeException("'min' cannot be negative.", "min");
            return repeatBetween(min, min, true).Then(repeatInfinite(greedy));
        }

        /// <summary>
        ///     Generates a matcher that matches this regular expression at least a minimum number of times and at most a
        ///     maximum number of times.</summary>
        /// <param name="min">
        ///     Minimum number of times to match.</param>
        /// <param name="max">
        ///     Maximum number of times to match.</param>
        /// <param name="greedy">
        ///     If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="min"/> is negative.</exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="max"/> is smaller than <paramref name="min"/>.</exception>
        private TGenerex repeatBetween(int min, int max, bool greedy)
        {
            if (min < 0) throw new ArgumentOutOfRangeException("'min' cannot be negative.", "min");
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

        /// <summary>
        ///     Turns the current regular expression into a zero-width negative look-ahead assertion (cf. <c>(?!...)</c> in
        ///     traditional regular expression syntax).</summary>
        public TGenerex LookAheadNegative() { return lookNegative(behind: false, defaultMatch: Generex.ZeroWidthMatch); }
        /// <summary>
        ///     Turns the current regular expression into a zero-width negative look-behind assertion (cf. <c>(?&lt;!...)</c>
        ///     in traditional regular expression syntax).</summary>
        public TGenerex LookBehindNegative() { return lookNegative(behind: true, defaultMatch: Generex.ZeroWidthMatch); }

        /// <summary>Returns a successful zero-width match.</summary>
        protected static IEnumerable<int> emptyMatch(T[] input, int startIndex) { return Generex.ZeroWidthMatch; }

        /// <summary>
        ///     Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TGenerexWithResult">
        ///     Generex type to return (for example, <see cref="Generex{T,TResult}"/>).</typeparam>
        /// <typeparam name="TGenerexWithResultMatch">
        ///     Generex match type that corresponds to <typeparamref name="TGenerexWithResult"/></typeparam>
        /// <typeparam name="TResult">
        ///     Type of the object returned by <paramref name="selector"/>, which is a result object associated with each
        ///     match of the regular expression.</typeparam>
        /// <param name="selector">
        ///     Function to process a regular expression match.</param>
        internal TGenerexWithResult process<TGenerexWithResult, TGenerexWithResultMatch, TResult>(Func<TGenerexMatch, TResult> selector)
            where TGenerexWithResult : GenerexWithResultBase<T, TResult, TGenerexWithResult, TGenerexWithResultMatch>
            where TGenerexWithResultMatch : GenerexMatch<T, TResult>
        {
            if (selector == null)
                throw new ArgumentNullException("selector");
            return GenerexWithResultBase<T, TResult, TGenerexWithResult, TGenerexWithResultMatch>.Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => new LengthAndResult<TResult>(selector(createMatch(input, startIndex, m)), m)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => new LengthAndResult<TResult>(selector(createBackwardsMatch(input, startIndex, m)), m))
            );
        }

        /// <summary>
        ///     Returns a regular expression that matches a single element which is not equal to the specified element.</summary>
        /// <param name="element">
        ///     List of elements excluded from matching.</param>
        public static TGenerex Not(T element) { return Not(EqualityComparer<T>.Default, element); }

        /// <summary>
        ///     Returns a regular expression that matches a single element which is not equal to the specified element.</summary>
        /// <param name="element">
        ///     List of elements excluded from matching.</param>
        /// <param name="comparer">
        ///     Equality comparer to use.</param>
        public static TGenerex Not(IEqualityComparer<T> comparer, T element)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            return Constructor(
                (input, startIndex) => startIndex >= input.Length || comparer.Equals(input[startIndex], element) ? Generex.NoMatch : Generex.OneElementMatch,
                (input, startIndex) => startIndex <= 0 || comparer.Equals(input[startIndex - 1], element) ? Generex.NoMatch : Generex.NegativeOneElementMatch
            );
        }

        /// <summary>
        ///     Returns a regular expression that matches a single element which is none of the specified elements.</summary>
        /// <param name="elements">
        ///     List of elements excluded from matching.</param>
        public static TGenerex Not(params T[] elements) { return Not(EqualityComparer<T>.Default, elements); }

        /// <summary>
        ///     Returns a regular expression that matches a single element which is none of the specified elements.</summary>
        /// <param name="elements">
        ///     List of elements excluded from matching.</param>
        /// <param name="comparer">
        ///     Equality comparer to use.</param>
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
        ///     Returns a regular expression that only matches if the subarray matched by this regular expression also
        ///     contains a match for the specified other regular expression, and if so, associates each match of this regular
        ///     expression with the result object returned by the other regular expression’s first match.</summary>
        /// <typeparam name="TOtherResult">
        ///     Type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the match object for the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the subarray matched by this regular expression.</param>
        /// <remarks>
        ///     It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///     cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</remarks>
        public TOtherGenerex And<TOtherResult, TOtherGenerex, TOtherGenerexMatch>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch>(subarray => other.Match(subarray));
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the subarray matched by this regular expression also fully
        ///     matches the specified other regular expression, and if so, associates each match of this regular expression
        ///     with the result object returned by the other regular expression’s match.</summary>
        /// <typeparam name="TOtherResult">
        ///     Type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the match object for the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the subarray matched by this regular expression.</param>
        /// <remarks>
        ///     It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///     cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</remarks>
        public TOtherGenerex AndExact<TOtherResult, TOtherGenerex, TOtherGenerexMatch>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch>(subarray => other.MatchExact(subarray));
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the subarray matched by this regular expression also
        ///     contains a match for the specified other regular expression, and if so, associates each match of this regular
        ///     expression with the result object returned by the other regular expression’s first match found when matching
        ///     backwards (starting at the end of the matched subarray).</summary>
        /// <typeparam name="TOtherResult">
        ///     Type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the match object for the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the subarray matched by this regular expression.</param>
        /// <remarks>
        ///     It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///     cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</remarks>
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

        /// <summary>Returns a regular expression that matches the first regular expression followed by the second.</summary>
        public static TGenerex operator +(GenerexNoResultBase<T, TGenerex, TGenerexMatch> one, GenerexNoResultBase<T, TGenerex, TGenerexMatch> two)
        {
            if (one == null)
                throw new ArgumentNullException("one");
            if (two == null)
                throw new ArgumentNullException("two");
            return one.Then(two);
        }

        /// <summary>
        ///     Returns a regular expression that matches the specified regular expression (first operand) followed by the
        ///     specified element (second operand).</summary>
        public static TGenerex operator +(GenerexNoResultBase<T, TGenerex, TGenerexMatch> one, T two)
        {
            if (one == null)
                throw new ArgumentNullException("one");
            if (two == null)
                throw new ArgumentNullException("two");
            return one.Then(two);
        }

        /// <summary>
        ///     Returns a regular expression that matches the specified element (first operand) followed by the specified
        ///     regular expression (second operand).</summary>
        public static TGenerex operator +(T one, GenerexNoResultBase<T, TGenerex, TGenerexMatch> two)
        {
            if (one == null)
                throw new ArgumentNullException("one");
            if (two == null)
                throw new ArgumentNullException("two");

            return Constructor(
                then(elementMatcher(one, EqualityComparer<T>.Default, false), two._forwardMatcher),
                then(two._backwardMatcher, elementMatcher(one, EqualityComparer<T>.Default, true)));
        }

        /// <summary>
        ///     Returns a regular expression that matches the specified regular expression (first operand) followed by the
        ///     specified element (second operand).</summary>
        public static TGenerex operator +(GenerexNoResultBase<T, TGenerex, TGenerexMatch> one, Predicate<T> two)
        {
            if (one == null)
                throw new ArgumentNullException("one");
            if (two == null)
                throw new ArgumentNullException("two");
            return one.Then(two);
        }

        /// <summary>
        ///     Returns a regular expression that matches the specified element (first operand) followed by the specified
        ///     regular expression (second operand).</summary>
        public static TGenerex operator +(Predicate<T> one, GenerexNoResultBase<T, TGenerex, TGenerexMatch> two)
        {
            if (one == null)
                throw new ArgumentNullException("one");
            if (two == null)
                throw new ArgumentNullException("two");

            return Constructor(
                then(forwardPredicateMatcher(one), two._forwardMatcher),
                then(two._backwardMatcher, backwardPredicateMatcher(one)));
        }

        /// <summary>
        ///     Returns a regular expression that matches either one of the specified regular expressions (cf. <c>|</c> in
        ///     traditional regular expression syntax).</summary>
        public static TGenerex operator |(GenerexNoResultBase<T, TGenerex, TGenerexMatch> one, TGenerex two)
        {
            if (one == null)
                throw new ArgumentNullException("one");
            if (two == null)
                throw new ArgumentNullException("two");
            return one.Or(two);
        }

        /// <summary>
        ///     Returns a regular expression that matches either the specified regular expression (first operand) or the
        ///     specified element (second operand) (cf. <c>|</c> in traditional regular expression syntax).</summary>
        public static TGenerex operator |(GenerexNoResultBase<T, TGenerex, TGenerexMatch> one, T two)
        {
            if (one == null)
                throw new ArgumentNullException("one");
            return one.Or(two);
        }

        /// <summary>
        ///     Returns a regular expression that matches either the specified element (first operand) or the specified
        ///     regular expression (second operand) (cf. <c>|</c> in traditional regular expression syntax).</summary>
        public static TGenerex operator |(T one, GenerexNoResultBase<T, TGenerex, TGenerexMatch> two)
        {
            if (two == null)
                throw new ArgumentNullException("two");

            return Constructor(
                or(elementMatcher(one, EqualityComparer<T>.Default, false), two._forwardMatcher),
                or(elementMatcher(one, EqualityComparer<T>.Default, true), two._backwardMatcher));
        }

        /// <summary>
        ///     Returns a regular expression that matches either the specified regular expression (first operand) or a single
        ///     element that satisfies the specified predicate (second operand) (cf. <c>|</c> in traditional regular
        ///     expression syntax).</summary>
        public static TGenerex operator |(GenerexNoResultBase<T, TGenerex, TGenerexMatch> one, Predicate<T> two)
        {
            if (one == null)
                throw new ArgumentNullException("one");
            if (two == null)
                throw new ArgumentNullException("two");
            return one.Or(two);
        }

        /// <summary>
        ///     Returns a regular expression that matches either a single element that satisfies the specified predicate
        ///     (first operand) or the specified regular expression (second operand) (cf. <c>|</c> in traditional regular
        ///     expression syntax).</summary>
        public static TGenerex operator |(Predicate<T> one, GenerexNoResultBase<T, TGenerex, TGenerexMatch> two)
        {
            if (one == null)
                throw new ArgumentNullException("one");
            if (two == null)
                throw new ArgumentNullException("two");

            return Constructor(
                or(forwardPredicateMatcher(one), two._forwardMatcher),
                or(backwardPredicateMatcher(one), two._backwardMatcher));
        }
    }
}
