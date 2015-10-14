using System;
using System.Collections.Generic;

namespace RT.Generexes
{
    /// <summary>
    ///     Provides regular-expression functionality for collections of arbitrary objects.</summary>
    /// <typeparam name="T">
    ///     Type of the objects in the collection.</typeparam>
    /// <typeparam name="TResult">
    ///     Type of the result object associated with each match of the regular expression.</typeparam>
    /// <remarks>
    ///     This type is not directly instantiated; use <see cref="Generex{T}.Process"/>.</remarks>
    public sealed class Generex<T, TResult> : GenerexWithResultBase<T, TResult, Generex<T, TResult>, GenerexMatch<T, TResult>>
    {
        /// <summary>
        ///     Instantiates a <see cref="GenerexMatch{T}"/> object from an index, length and result object.</summary>
        /// <param name="result">
        ///     The result object associated with this match.</param>
        /// <param name="input">
        ///     Original input array that was matched against.</param>
        /// <param name="index">
        ///     Start index of the match.</param>
        /// <param name="length">
        ///     Length of the match.</param>
        protected sealed override GenerexMatch<T, TResult> createMatchWithResult(TResult result, T[] input, int index, int length)
        {
            return new GenerexMatch<T, TResult>(result, input, index, length);
        }

        /// <summary>Instantiates an empty regular expression which always matches and returns the specified result object.</summary>
        public Generex(TResult result) : base(result) { }

        private Generex(matcher forward, matcher backward) : base(forward, backward) { }
        static Generex() { Constructor = (forward, backward) => new Generex<T, TResult>(forward, backward); }

        /// <summary>
        ///     Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">
        ///     Function to process a regular expression match.</param>
        public Generex<T, TOtherResult> Process<TOtherResult>(Func<GenerexMatch<T, TResult>, TOtherResult> selector)
        {
            return process<Generex<T, TOtherResult>, GenerexMatch<T, TOtherResult>, TOtherResult>(selector);
        }

        /// <summary>
        ///     Processes each match of this regular expression by running each result through a provided selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">
        ///     Function to process the result of a regular expression match.</param>
        public Generex<T, TOtherResult> ProcessRaw<TOtherResult>(Func<TResult, TOtherResult> selector)
        {
            return processRaw<Generex<T, TOtherResult>, GenerexMatch<T, TOtherResult>, TOtherResult>(selector);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, followed by the specified one, and
        ///     generates a result object that combines the result of this regular expression with the match of the other.</summary>
        public Generex<T, TCombined> Then<TCombined>(Generex<T> other, Func<TResult, GenerexMatch<T>, TCombined> selector)
        {
            return then<Generex<T>, GenerexMatch<T>, int, Generex<T, TCombined>, GenerexMatch<T, TCombined>, TCombined>(other, selector);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, followed by the specified one, and
        ///     generates a result object that combines the result of this regular expression with the match of the other.</summary>
        public Generex<T, TCombined> Then<TOther, TCombined>(Generex<T, TOther> other, Func<TResult, GenerexMatch<T, TOther>, TCombined> selector)
        {
            return then<Generex<T, TOther>, GenerexMatch<T, TOther>, LengthAndResult<TOther>, Generex<T, TCombined>, GenerexMatch<T, TCombined>, TCombined>(other, selector);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, followed by the specified one, and
        ///     generates a result object that combines the original two matches.</summary>
        public Generex<T, TCombined> ThenRaw<TOther, TCombined>(Generex<T, TOther> other, Func<TResult, TOther, TCombined> selector)
        {
            return thenRaw<Generex<T, TOther>, GenerexMatch<T, TOther>, TOther, Generex<T, TCombined>, GenerexMatch<T, TCombined>, TCombined>(other, selector);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then attempts to match the specified other
        ///     regular expression and throws an exception if the second regular expression fails to match; otherwise, a
        ///     result object is generated from the current result object and the second match.</summary>
        /// <typeparam name="TCombined">
        ///     Type of the result object for the resulting regular expression.</typeparam>
        /// <param name="expectation">
        ///     The regular expression that is expected to match after the current one.</param>
        /// <param name="selector">
        ///     A selector which, in case of a match, generates the new result given the current result object and the match
        ///     of the <paramref name="expectation"/>.</param>
        /// <param name="exceptionGenerator">
        ///     A selector which, in case of no match, generates the exception object to be thrown.</param>
        /// <returns>
        ///     The resulting regular expression.</returns>
        /// <remarks>
        ///     Regular expressions created by this method cannot match backwards. The full set of affected methods is listed
        ///     at <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then{TOtherGenerex, TOtherMatch,
        ///     TOtherGenerexMatch}(Func{TGenerexMatch, GenerexBase{T, TOtherMatch, TOtherGenerex, TOtherGenerexMatch}})"/>.</remarks>
        public Generex<T, TCombined> ThenExpect<TCombined>(Generex<T> expectation, Func<TResult, GenerexMatch<T>, TCombined> selector, Func<GenerexMatch<T, TResult>, Exception> exceptionGenerator)
        {
            if (expectation == null)
                throw new ArgumentNullException("expectation");
            if (selector == null)
                throw new ArgumentNullException("selector");
            if (exceptionGenerator == null)
                throw new ArgumentNullException("exceptionGenerator");
            return then<Generex<T, TCombined>, LengthAndResult<TCombined>, GenerexMatch<T, TCombined>, GenerexMatch<T, TResult>>(m =>
                expectation.expect(() => exceptionGenerator(m)).process<Generex<T, TCombined>, GenerexMatch<T, TCombined>, TCombined>(m2 => selector(m.Result, m2)), createMatch);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then attempts to match the specified other
        ///     regular expression and throws an exception if the second regular expression fails to match; otherwise, a
        ///     result object is generated from the current result object and the second match.</summary>
        /// <typeparam name="TOther">
        ///     Type of the result object of the <paramref name="expectation"/>.</typeparam>
        /// <typeparam name="TCombined">
        ///     Type of the result object for the resulting regular expression.</typeparam>
        /// <param name="expectation">
        ///     The regular expression that is expected to match after the current one.</param>
        /// <param name="selector">
        ///     A selector which, in case of a match, generates the new result given the current result object and the match
        ///     of the <paramref name="expectation"/>.</param>
        /// <param name="exceptionGenerator">
        ///     A selector which, in case of no match, generates the exception object to be thrown.</param>
        /// <returns>
        ///     The resulting regular expression.</returns>
        /// <remarks>
        ///     Regular expressions created by this method cannot match backwards. The full set of affected methods is listed
        ///     at <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then{TOtherGenerex, TOtherMatch,
        ///     TOtherGenerexMatch}(Func{TGenerexMatch, GenerexBase{T, TOtherMatch, TOtherGenerex, TOtherGenerexMatch}})"/>.</remarks>
        public Generex<T, TCombined> ThenExpect<TOther, TCombined>(Generex<T, TOther> expectation, Func<TResult, GenerexMatch<T, TOther>, TCombined> selector, Func<GenerexMatch<T, TResult>, Exception> exceptionGenerator)
        {
            if (expectation == null)
                throw new ArgumentNullException("expectation");
            if (selector == null)
                throw new ArgumentNullException("selector");
            return then<Generex<T, TCombined>, LengthAndResult<TCombined>, GenerexMatch<T, TCombined>, GenerexMatch<T, TResult>>(m =>
                expectation.process<Generex<T, TCombined>, GenerexMatch<T, TCombined>, TCombined>(m2 => selector(m.Result, m2)).expect(() => exceptionGenerator(m)), createMatch);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then attempts to match the specified other
        ///     regular expression and throws an exception if the second regular expression fails to match; otherwise, a
        ///     result object is generated from the result objects of the two matches.</summary>
        /// <typeparam name="TOther">
        ///     Type of the result object of the <paramref name="expectation"/>.</typeparam>
        /// <typeparam name="TCombined">
        ///     Type of the result object for the resulting regular expression.</typeparam>
        /// <param name="expectation">
        ///     The regular expression that is expected to match after the current one.</param>
        /// <param name="selector">
        ///     A selector which, in case of a match, generates the new result given the current result and the result object
        ///     of the match of the <paramref name="expectation"/>.</param>
        /// <param name="exceptionGenerator">
        ///     A selector which, in case of no match, generates the exception object to be thrown.</param>
        /// <returns>
        ///     The resulting regular expression.</returns>
        /// <remarks>
        ///     Regular expressions created by this method cannot match backwards. The full set of affected methods is listed
        ///     at <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then{TOtherGenerex, TOtherMatch,
        ///     TOtherGenerexMatch}(Func{TGenerexMatch, GenerexBase{T, TOtherMatch, TOtherGenerex, TOtherGenerexMatch}})"/>.</remarks>
        public Generex<T, TCombined> ThenExpectRaw<TOther, TCombined>(Generex<T, TOther> expectation, Func<TResult, TOther, TCombined> selector, Func<GenerexMatch<T, TResult>, Exception> exceptionGenerator)
        {
            if (expectation == null)
                throw new ArgumentNullException("expectation");
            if (selector == null)
                throw new ArgumentNullException("selector");
            return then<Generex<T, TCombined>, LengthAndResult<TCombined>, GenerexMatch<T, TCombined>, GenerexMatch<T, TResult>>(m =>
                expectation.process<Generex<T, TCombined>, GenerexMatch<T, TCombined>, TCombined>(m2 => selector(m.Result, m2.Result)).expect(() => exceptionGenerator(m)), createMatch);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf.
        ///     <c>?</c> in traditional regular expression syntax).</summary>
        public Generex<T, IEnumerable<TResult>> OptionalGreedy() { return repeatBetween<Generex<T, IEnumerable<TResult>>, GenerexMatch<T, IEnumerable<TResult>>>(0, 1, greedy: true); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero times or once. Zero times is
        ///     prioritised (cf. <c>??</c> in traditional regular expression syntax).</summary>
        public Generex<T, IEnumerable<TResult>> Optional() { return repeatBetween<Generex<T, IEnumerable<TResult>>, GenerexMatch<T, IEnumerable<TResult>>>(0, 1, greedy: false); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero or more times. More times are
        ///     prioritised (cf. <c>*</c> in traditional regular expression syntax).</summary>
        public Generex<T, IEnumerable<TResult>> RepeatGreedy() { return repeatInfinite<Generex<T, IEnumerable<TResult>>, GenerexMatch<T, IEnumerable<TResult>>>(greedy: true); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero or more times. Fewer times are
        ///     prioritised (cf. <c>*?</c> in traditional regular expression syntax).</summary>
        public Generex<T, IEnumerable<TResult>> Repeat() { return repeatInfinite<Generex<T, IEnumerable<TResult>>, GenerexMatch<T, IEnumerable<TResult>>>(greedy: false); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression the specified number of times or more. More
        ///     times are prioritised (cf. <c>{min,}</c> in traditional regular expression syntax).</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="min"/> is negative.</exception>
        public Generex<T, IEnumerable<TResult>> RepeatGreedy(int min) { return repeatMin<Generex<T, IEnumerable<TResult>>, GenerexMatch<T, IEnumerable<TResult>>>(min, greedy: true); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression the specified number of times or more. Fewer
        ///     times are prioritised (cf. <c>{min,}?</c> in traditional regular expression syntax).</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="min"/> is negative.</exception>
        public Generex<T, IEnumerable<TResult>> Repeat(int min) { return repeatMin<Generex<T, IEnumerable<TResult>>, GenerexMatch<T, IEnumerable<TResult>>>(min, greedy: false); }
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
        public Generex<T, IEnumerable<TResult>> RepeatGreedy(int min, int max) { return repeatBetween<Generex<T, IEnumerable<TResult>>, GenerexMatch<T, IEnumerable<TResult>>>(min, max, greedy: true); }
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
        public Generex<T, IEnumerable<TResult>> Repeat(int min, int max) { return repeatBetween<Generex<T, IEnumerable<TResult>>, GenerexMatch<T, IEnumerable<TResult>>>(min, max, greedy: false); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression the specified number of times (cf.
        ///     <c>{times}</c> in traditional regular expression syntax).</summary>
        /// <param name="times">
        ///     A non-negative number specifying the number of repetitions of the regular expression.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="times"/> is negative.</exception>
        public Generex<T, IEnumerable<TResult>> Times(int times)
        {
            if (times < 0) throw new ArgumentOutOfRangeException("'times' cannot be negative.", "times");
            return repeatBetween<Generex<T, IEnumerable<TResult>>, GenerexMatch<T, IEnumerable<TResult>>>(times, times, true);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression one or more times, interspersed with a
        ///     separator. Fewer times are prioritised.</summary>
        public Generex<T, IEnumerable<TResult>> RepeatWithSeparator(Generex<T> separator)
        {
            return ThenRaw(separator.Then(this).Repeat(), InternalExtensions.Concat);
        }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression one or more times, interspersed with a
        ///     separator. More times are prioritised.</summary>
        public Generex<T, IEnumerable<TResult>> RepeatWithSeparatorGreedy(Generex<T> separator)
        {
            return ThenRaw(separator.Then(this).RepeatGreedy(), InternalExtensions.Concat);
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the subarray matched by this regular expression also
        ///     contains a match for the specified other regular expression, and if so, combines the first match’s result
        ///     object with the second match using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the result object associated with the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the subarray matched by this regular expression.</param>
        /// <param name="selector">
        ///     A selector function that combines the result object associated with the match of this regular expression, and
        ///     the match of <paramref name="other"/>, into a new result object.</param>
        /// <remarks>
        ///     <para>
        ///         It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///         cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</para>
        ///     <para>
        ///         The value of the <see cref="GenerexMatch{T}.Index"/> property of the match object passed into <paramref
        ///         name="selector"/> refers to the index within the subarray, not the index within the original input
        ///         sequence.</para></remarks>
        public Generex<T, TCombinedResult> And<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherGenerexMatch, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, Generex<T, TCombinedResult>, GenerexMatch<T, TCombinedResult>>(subarray => other.Match(subarray), selector);
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the subarray matched by this regular expression also fully
        ///     matches the specified other regular expression, and if so, combines the first match’s result object with the
        ///     second match using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the result object associated with the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the subarray matched by this regular expression.</param>
        /// <param name="selector">
        ///     A selector function that combines the result object associated with the match of this regular expression, and
        ///     the match of <paramref name="other"/>, into a new result object.</param>
        /// <remarks>
        ///     <para>
        ///         It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///         cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</para>
        ///     <para>
        ///         The value of the <see cref="GenerexMatch{T}.Index"/> property of the match object passed into <paramref
        ///         name="selector"/> refers to the index within the subarray, not the index within the original input
        ///         sequence.</para></remarks>
        public Generex<T, TCombinedResult> AndExact<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherGenerexMatch, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, Generex<T, TCombinedResult>, GenerexMatch<T, TCombinedResult>>(subarray => other.MatchExact(subarray), selector);
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the subarray matched by this regular expression also
        ///     contains a match for the specified other regular expression when matching backwards, and if so, combines the
        ///     first match’s result object with the second match using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the result object associated with the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the subarray matched by this regular expression.</param>
        /// <param name="selector">
        ///     A selector function that combines the result object associated with the match of this regular expression, and
        ///     the match of <paramref name="other"/>, into a new result object.</param>
        /// <remarks>
        ///     <para>
        ///         It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///         cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</para>
        ///     <para>
        ///         The value of the <see cref="GenerexMatch{T}.Index"/> property of the match object passed into <paramref
        ///         name="selector"/> refers to the index within the subarray, not the index within the original input
        ///         sequence.</para></remarks>
        public Generex<T, TCombinedResult> AndReverse<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherGenerexMatch, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, Generex<T, TCombinedResult>, GenerexMatch<T, TCombinedResult>>(subarray => other.MatchReverse(subarray), selector);
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the subarray matched by this regular expression also
        ///     contains a match for the specified other regular expression, and if so, combines the result objects associated
        ///     with both matches using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the result object associated with the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the subarray matched by this regular expression.</param>
        /// <param name="selector">
        ///     A selector function that combines the result objects associated with the matches of this regular expression
        ///     and <paramref name="other"/> into a new result object.</param>
        /// <remarks>
        ///     It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///     cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</remarks>
        public Generex<T, TCombinedResult> AndRaw<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherResult, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, Generex<T, TCombinedResult>, GenerexMatch<T, TCombinedResult>>(subarray => other.Match(subarray), (result, otherMatch) => selector(result, otherMatch.Result));
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the subarray matched by this regular expression also fully
        ///     matches the specified other regular expression, and if so, combines the result objects associated with both
        ///     matches using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the result object associated with the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the subarray matched by this regular expression.</param>
        /// <param name="selector">
        ///     A selector function that combines the result objects associated with the matches of this regular expression
        ///     and <paramref name="other"/> into a new result object.</param>
        /// <remarks>
        ///     It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///     cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</remarks>
        public Generex<T, TCombinedResult> AndExactRaw<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherResult, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, Generex<T, TCombinedResult>, GenerexMatch<T, TCombinedResult>>(subarray => other.MatchExact(subarray), (result, otherMatch) => selector(result, otherMatch.Result));
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the subarray matched by this regular expression also
        ///     contains a match for the specified other regular expression when matching backwards, and if so, combines the
        ///     result objects associated with both matches using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the result object associated with the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the subarray matched by this regular expression.</param>
        /// <param name="selector">
        ///     A selector function that combines the result objects associated with the matches of this regular expression
        ///     and <paramref name="other"/> into a new result object.</param>
        /// <remarks>
        ///     It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///     cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</remarks>
        public Generex<T, TCombinedResult> AndReverseRaw<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult>(GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherResult, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, Generex<T, TCombinedResult>, GenerexMatch<T, TCombinedResult>>(subarray => other.MatchReverse(subarray), (result, otherMatch) => selector(result, otherMatch.Result));
        }

        /// <summary>
        ///     Returns a regular expression that matches the first regular expression followed by the second and retains the
        ///     result object generated by each match of the first regular expression.</summary>
        public static Generex<T, TResult> operator +(Generex<T, TResult> one, Generex<T> two) { return one.Then(two); }

        /// <summary>
        ///     Returns a regular expression that matches the first regular expression followed by the second and retains the
        ///     result object generated by each match of the second regular expression.</summary>
        public static Generex<T, TResult> operator +(Generex<T> one, Generex<T, TResult> two) { return one.Then(two); }

        /// <summary>
        ///     Returns a regular expression that casts the result object of this regular expression to a different type.</summary>
        /// <typeparam name="TOtherResult">
        ///     Type to cast the result object to.</typeparam>
        public Generex<T, TOtherResult> Cast<TOtherResult>() { return ProcessRaw(obj => (TOtherResult) (object) obj); }

        /// <summary>
        ///     Returns a regular expression that matches only if the result object of this regular expression is of the
        ///     specified type.</summary>
        /// <typeparam name="TOtherResult">
        ///     Type of result object to filter by.</typeparam>
        public Generex<T, TOtherResult> OfType<TOtherResult>() { return WhereRaw(obj => obj is TOtherResult).Cast<TOtherResult>(); }
    }
}
