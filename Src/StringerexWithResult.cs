using System;
using System.Collections.Generic;

namespace RT.Generexes
{
    /// <summary>
    ///     Provides regular-expression functionality for strings.</summary>
    /// <typeparam name="TResult">
    ///     Type of the result object associated with each match of the regular expression.</typeparam>
    /// <remarks>
    ///     This type is not directly instantiated; use <see cref="Stringerex.Process"/>.</remarks>
    public sealed class Stringerex<TResult> : GenerexWithResultBase<char, TResult, Stringerex<TResult>, StringerexMatch<TResult>>
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
        protected sealed override StringerexMatch<TResult> createMatchWithResult(TResult result, char[] input, int index, int length)
        {
            return new StringerexMatch<TResult>(result, input, index, length);
        }

        /// <summary>Instantiates an empty regular expression which always matches and returns the specified result object.</summary>
        public Stringerex(TResult result) : base(result) { }

        private Stringerex(matcher forward, matcher backward) : base(forward, backward) { }
        static Stringerex() { Constructor = (forward, backward) => new Stringerex<TResult>(forward, backward); }

        /// <summary>
        ///     Determines whether the given string contains a match for this regular expression, optionally starting the
        ///     search at a specified character index.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="startAt">
        ///     Optional character index at which to start the search. Matches that start before this index are not included.</param>
        public bool IsMatch(string input, int startAt = 0) { return base.IsMatch(input.ToCharArray(), startAt); }

        /// <summary>
        ///     Determines whether the given string matches this regular expression at a specific character index.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="mustStartAt">
        ///     Index at which the match must start (default is 0).</param>
        /// <returns>
        ///     <c>true</c> if a match starting at the specified index exists (which need not run all the way to the end of
        ///     the string); otherwise, <c>false</c>.</returns>
        public bool IsMatchAt(string input, int mustStartAt = 0) { return base.IsMatchAt(input.ToCharArray(), mustStartAt); }

        /// <summary>
        ///     Determines whether the given string matches this regular expression up to a specific character index.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="mustEndAt">
        ///     Index at which the match must end (default is the end of the string).</param>
        /// <returns>
        ///     <c>true</c> if a match ending at the specified index exists (which need not begin at the start of the string);
        ///     otherwise, <c>false</c>.</returns>
        public bool IsMatchUpTo(string input, int? mustEndAt = null) { return base.IsMatchUpTo(input.ToCharArray(), mustEndAt); }

        /// <summary>
        ///     Determines whether the given string matches this regular expression exactly.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="mustStartAt">
        ///     Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">
        ///     Index at which the match must end (default is the end of the string).</param>
        /// <returns>
        ///     <c>true</c> if a match starting and ending at the specified indexes exists; otherwise, <c>false</c>.</returns>
        public bool IsMatchExact(string input, int mustStartAt = 0, int? mustEndAt = null) { return base.IsMatchExact(input.ToCharArray(), mustStartAt, mustEndAt); }

        /// <summary>
        ///     Determines whether the given string contains a match for this regular expression that ends before the
        ///     specified maximum character index.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="endAt">
        ///     Optional index before which a match must end. The search begins by matching from this index backwards, and
        ///     then proceeds towards the start of the string.</param>
        public bool IsMatchReverse(string input, int? endAt = null) { return base.IsMatchReverse(input.ToCharArray(), endAt); }

        /// <summary>
        ///     Determines whether the given string matches this regular expression, and if so, returns information about the
        ///     first match.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="startAt">
        ///     Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>
        ///     An object describing a regular expression match in case of success; <c>null</c> if no match.</returns>
        public StringerexMatch<TResult> Match(string input, int startAt = 0) { return base.Match(input.ToCharArray(), startAt); }

        /// <summary>
        ///     Determines whether the given string matches this regular expression exactly, and if so, returns information
        ///     about the match.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="mustStartAt">
        ///     Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">
        ///     Index at which the match must end (default is the end of the string).</param>
        /// <returns>
        ///     An object describing the regular expression match in case of success; <c>null</c> if no match.</returns>
        public StringerexMatch<TResult> MatchExact(string input, int mustStartAt = 0, int? mustEndAt = null) { return base.MatchExact(input.ToCharArray(), mustStartAt, mustEndAt); }

        /// <summary>
        ///     Determines whether the given string matches this regular expression, and if so, returns information about the
        ///     first match found by matching the regular expression backwards (starting from the end of the string).</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="endAt">
        ///     Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>
        ///     An object describing a regular expression match in case of success; <c>null</c> if no match.</returns>
        public StringerexMatch<TResult> MatchReverse(string input, int? endAt = null) { return base.MatchReverse(input.ToCharArray(), endAt); }

        /// <summary>
        ///     Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the
        ///     specified string), optionally starting the search at the specified index.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="endAt">
        ///     Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        public IEnumerable<StringerexMatch<TResult>> MatchesReverse(string input, int? endAt = null) { return base.MatchesReverse(input.ToCharArray(), endAt); }

        /// <summary>
        ///     Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the
        ///     specified character index.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="startAt">
        ///     Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>
        ///     The behaviour is analogous to <see cref="System.Text.RegularExpressions.Regex.Matches(string,string)"/>. The
        ///     documentation for that method claims that it returns “all occurrences of the regular expression”, but this is
        ///     false.</remarks>
        public IEnumerable<StringerexMatch<TResult>> Matches(string input, int startAt = 0) { return base.Matches(input.ToCharArray(), startAt); }

        /// <summary>
        ///     Determines whether the given input string matches this regular expression, and if so, returns the result of
        ///     the first match.</summary>
        /// <param name="input">
        ///     Input string to match the regular expression against.</param>
        /// <param name="startAt">
        ///     Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>
        ///     The result of the first match in case of success; <c>default(TResult)</c> if no match.</returns>
        public TResult RawMatch(string input, int startAt = 0) { return RawMatch(input.ToCharArray(), startAt); }

        /// <summary>
        ///     Determines whether the given input string matches this regular expression, and if so, returns the result of
        ///     the first match found by matching the regular expression backwards.</summary>
        /// <param name="input">
        ///     Input string to match the regular expression against.</param>
        /// <param name="endAt">
        ///     Optional index at which to end the search. Matches that end at or after this index are not included. (Default
        ///     is the end of the input string.)</param>
        /// <returns>
        ///     The result of the match in case of success; <c>default(TResult)</c> if no match.</returns>
        public TResult RawMatchReverse(string input, int? endAt = null) { return RawMatchReverse(input.ToCharArray(), endAt); }

        /// <summary>
        ///     Determines whether the given input string matches this regular expression exactly, and if so, returns the
        ///     match.</summary>
        /// <param name="input">
        ///     Input string to match the regular expression against.</param>
        /// <param name="mustStartAt">
        ///     Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">
        ///     Index at which the match must end (default is the end of the input string).</param>
        /// <returns>
        ///     The result of the match in case of success; <c>default(TResult)</c> if no match.</returns>
        public TResult RawMatchExact(string input, int mustStartAt = 0, int? mustEndAt = null) { return RawMatchExact(input.ToCharArray(), mustStartAt, mustEndAt); }

        /// <summary>
        ///     Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the
        ///     specified index.</summary>
        /// <param name="input">
        ///     Input string to match the regular expression against.</param>
        /// <param name="startAt">
        ///     Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>
        ///     The behaviour is analogous to <see cref="System.Text.RegularExpressions.Regex.Matches(string,string)"/>. The
        ///     documentation for that method claims that it returns “all occurrences of the regular expression”, but this is
        ///     false.</remarks>
        public IEnumerable<TResult> RawMatches(string input, int startAt = 0) { return RawMatches(input.ToCharArray(), startAt); }

        /// <summary>
        ///     Returns a sequence of non-overlapping regular expression matches going backwards, optionally starting the
        ///     search at the specified index.</summary>
        /// <param name="input">
        ///     Input sequence to match the regular expression against.</param>
        /// <param name="endAt">
        ///     Optional index at which to begin the reverse search. Matches that end at or after this index are not included.
        ///     (Default is the end of the input string.)</param>
        public IEnumerable<TResult> RawMatchesReverse(string input, int? endAt = null) { return RawMatchesReverse(input.ToCharArray(), endAt); }

        /// <summary>
        ///     Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">
        ///     Function to process a regular expression match.</param>
        public Stringerex<TOtherResult> Process<TOtherResult>(Func<StringerexMatch<TResult>, TOtherResult> selector)
        {
            return process<Stringerex<TOtherResult>, StringerexMatch<TOtherResult>, TOtherResult>(selector);
        }

        /// <summary>
        ///     Processes each match of this regular expression by running each result through a provided selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">
        ///     Function to process the result of a regular expression match.</param>
        public Stringerex<TOtherResult> ProcessRaw<TOtherResult>(Func<TResult, TOtherResult> selector)
        {
            return processRaw<Stringerex<TOtherResult>, StringerexMatch<TOtherResult>, TOtherResult>(selector);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, followed by the specified one, and
        ///     generates a match object that combines the result of this regular expression with the match of the other.</summary>
        public Stringerex<TCombined> Then<TCombined>(Stringerex other, Func<TResult, StringerexMatch, TCombined> selector)
        {
            return then<Stringerex, StringerexMatch, Stringerex<TCombined>, StringerexMatch<TCombined>, TCombined>(other, selector);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, followed by the specified one, and
        ///     generates a match object that combines the result of this regular expression with the match of the other.</summary>
        public Stringerex<TCombined> Then<TOther, TCombined>(Stringerex<TOther> other, Func<TResult, StringerexMatch<TOther>, TCombined> selector)
        {
            return then<Stringerex<TOther>, StringerexMatch<TOther>, TOther, Stringerex<TCombined>, StringerexMatch<TCombined>, TCombined>(other, selector);
        }

        /// <summary>Returns a regular expression that matches this regular expression followed by the specified string.</summary>
        public Stringerex<TResult> Then(string str) { return base.Then(EqualityComparer<char>.Default, str.ToCharArray()); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression followed by the specified string, using the
        ///     specified equality comparer.</summary>
        public Stringerex<TResult> Then(IEqualityComparer<char> comparer, string str) { return base.Then(comparer, str.ToCharArray()); }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, followed by the specified one, and
        ///     generates a match object that combines the original two matches.</summary>
        public Stringerex<TCombined> ThenRaw<TOther, TCombined>(Stringerex<TOther> other, Func<TResult, TOther, TCombined> selector)
        {
            return thenRaw<Stringerex<TOther>, StringerexMatch<TOther>, TOther, Stringerex<TCombined>, StringerexMatch<TCombined>, TCombined>(other, selector);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then attempts to match the specified other
        ///     regular expression and throws an exception if the second regular expression fails to match; otherwise, a match
        ///     object is generated from the current match object and the second match.</summary>
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
        public Stringerex<TCombined> ThenExpect<TCombined>(Stringerex expectation, Func<TResult, StringerexMatch, TCombined> selector, Func<StringerexMatch<TResult>, Exception> exceptionGenerator)
        {
            return thenExpect<Stringerex, int, StringerexMatch, Stringerex<TCombined>, LengthAndResult<TCombined>, StringerexMatch<TCombined>>(expectation,
                (input, startIndex, match) => exceptionGenerator(createMatch(input, startIndex, match)),
                (input, startIndex, m1, m2) => new LengthAndResult<TCombined>(selector(m1.Result, expectation.createMatch(input, startIndex, m2)), getLength(m1) + m2));
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then attempts to match the specified other
        ///     regular expression and throws an exception if the second regular expression fails to match; otherwise, a match
        ///     object is generated from the current match object and the second match.</summary>
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
        public Stringerex<TCombined> ThenExpect<TOther, TCombined>(Stringerex<TOther> expectation, Func<TResult, StringerexMatch<TOther>, TCombined> selector, Func<StringerexMatch<TResult>, Exception> exceptionGenerator)
        {
            return thenExpect<Stringerex<TOther>, LengthAndResult<TOther>, StringerexMatch<TOther>, Stringerex<TCombined>, LengthAndResult<TCombined>, StringerexMatch<TCombined>>(expectation,
                (input, startIndex, match) => exceptionGenerator(createMatch(input, startIndex, match)),
                (input, startIndex, m1, m2) => new LengthAndResult<TCombined>(selector(m1.Result, expectation.createMatch(input, startIndex, m2)), getLength(m1) + expectation.getLength(m2)));
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then attempts to match the specified other
        ///     regular expression and throws an exception if the second regular expression fails to match; otherwise, a match
        ///     object is generated from the match objects of the two matches.</summary>
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
        public Stringerex<TCombined> ThenExpectRaw<TOther, TCombined>(Stringerex<TOther> expectation, Func<TResult, TOther, TCombined> selector, Func<StringerexMatch<TResult>, Exception> exceptionGenerator)
        {
            return thenExpect<Stringerex<TOther>, LengthAndResult<TOther>, StringerexMatch<TOther>, Stringerex<TCombined>, LengthAndResult<TCombined>, StringerexMatch<TCombined>>(expectation,
                (input, startIndex, match) => exceptionGenerator(createMatch(input, startIndex, match)),
                (input, startIndex, m1, m2) => new LengthAndResult<TCombined>(selector(m1.Result, m2.Result), getLength(m1) + expectation.getLength(m2)));
        }

        /// <summary>
        ///     Returns a regular expression that matches either this regular expression or the specified string (cf. <c>|</c>
        ///     in traditional regular expression syntax).</summary>
        /// <param name="str">
        ///     The string to match.</param>
        /// <param name="selector">
        ///     A selector that returns the result object for the new regular expression based on the string matched by
        ///     <paramref name="str"/>.</param>
        /// <param name="comparer">
        ///     An optional equality comparer to use against <paramref name="str"/>.</param>
        public Stringerex<TResult> Or(string str, Func<StringerexMatch, TResult> selector, IEqualityComparer<char> comparer = null)
        {
            var other = new Stringerex(str, comparer ?? EqualityComparer<char>.Default).Process(selector);
            return Or(Constructor(new matcher(other._forwardMatcher), new matcher(other._backwardMatcher)));
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf.
        ///     <c>?</c> in traditional regular expression syntax).</summary>
        public Stringerex<IEnumerable<TResult>> OptionalGreedy() { return repeatBetween<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(0, 1, greedy: true); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero times or once. Zero times is
        ///     prioritised (cf. <c>??</c> in traditional regular expression syntax).</summary>
        public Stringerex<IEnumerable<TResult>> Optional() { return repeatBetween<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(0, 1, greedy: false); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero or more times. More times are
        ///     prioritised (cf. <c>*</c> in traditional regular expression syntax).</summary>
        public Stringerex<IEnumerable<TResult>> RepeatGreedy() { return repeatInfinite<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(greedy: true); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero or more times. Fewer times are
        ///     prioritised (cf. <c>*?</c> in traditional regular expression syntax).</summary>
        public Stringerex<IEnumerable<TResult>> Repeat() { return repeatInfinite<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(greedy: false); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression the specified number of times or more. More
        ///     times are prioritised (cf. <c>{min,}</c> in traditional regular expression syntax).</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="min"/> is negative.</exception>
        public Stringerex<IEnumerable<TResult>> RepeatGreedy(int min) { return repeatMin<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(min, greedy: true); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression the specified number of times or more. Fewer
        ///     times are prioritised (cf. <c>{min,}?</c> in traditional regular expression syntax).</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="min"/> is negative.</exception>
        public Stringerex<IEnumerable<TResult>> Repeat(int min) { return repeatMin<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(min, greedy: false); }
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
        public Stringerex<IEnumerable<TResult>> RepeatGreedy(int min, int max) { return repeatBetween<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(min, max, greedy: true); }
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
        public Stringerex<IEnumerable<TResult>> Repeat(int min, int max) { return repeatBetween<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(min, max, greedy: false); }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression the specified number of times (cf.
        ///     <c>{times}</c> in traditional regular expression syntax).</summary>
        /// <param name="times">
        ///     A non-negative number specifying the number of times the regular expression must match.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="times"/> is negative.</exception>
        public Stringerex<IEnumerable<TResult>> Times(int times)
        {
            if (times < 0) throw new ArgumentOutOfRangeException("'times' cannot be negative.", "times");
            return repeatBetween<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(times, times, true);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression one or more times, interspersed with a
        ///     separator. Fewer times are prioritised.</summary>
        public Stringerex<IEnumerable<TResult>> RepeatWithSeparator(Stringerex separator)
        {
            return ThenRaw(separator.Then(this).Repeat(), InternalExtensions.Concat);
        }
        /// <summary>
        ///     Returns a regular expression that matches this regular expression one or more times, interspersed with a
        ///     separator. More times are prioritised.</summary>
        public Stringerex<IEnumerable<TResult>> RepeatWithSeparatorGreedy(Stringerex separator)
        {
            return ThenRaw(separator.Then(this).RepeatGreedy(), InternalExtensions.Concat);
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the substring matched by this regular expression also
        ///     contains a match for the specified other regular expression, and if so, combines the first match’s result
        ///     object with the second match using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the match object for the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the substring matched by this regular expression.</param>
        /// <param name="selector">
        ///     A selector function that combines the result object associated with the match of this regular expression, and
        ///     the match of <paramref name="other"/>, into a new result object.</param>
        /// <remarks>
        ///     <para>
        ///         It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///         cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</para>
        ///     <para>
        ///         The value of the <see cref="GenerexMatch{T}.Index"/> property of the match object passed into <paramref
        ///         name="selector"/> refers to the index within the substring, not the index within the original input
        ///         sequence.</para></remarks>
        public Stringerex<TCombinedResult> And<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult>(GenerexWithResultBase<char, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherGenerexMatch, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<char, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : StringerexMatch<TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, Stringerex<TCombinedResult>, StringerexMatch<TCombinedResult>>(substring => other.Match(substring), selector);
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the substring matched by this regular expression also fully
        ///     matches the specified other regular expression, and if so, combines the first match’s result object with the
        ///     second match using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the match object for the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the substring matched by this regular expression.</param>
        /// <param name="selector">
        ///     A selector function that combines the result object associated with the match of this regular expression, and
        ///     the match of <paramref name="other"/>, into a new result object.</param>
        /// <remarks>
        ///     <para>
        ///         It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///         cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</para>
        ///     <para>
        ///         The value of the <see cref="GenerexMatch{T}.Index"/> property of the match object passed into <paramref
        ///         name="selector"/> refers to the index within the substring, not the index within the original input
        ///         sequence.</para></remarks>
        public Stringerex<TCombinedResult> AndExact<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult>(GenerexWithResultBase<char, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherGenerexMatch, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<char, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : StringerexMatch<TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, Stringerex<TCombinedResult>, StringerexMatch<TCombinedResult>>(substring => other.MatchExact(substring), selector);
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the substring matched by this regular expression also
        ///     contains a match for the specified other regular expression when matching backwards, and if so, combines the
        ///     first match’s result object with the second match using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the match object for the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the substring matched by this regular expression.</param>
        /// <param name="selector">
        ///     A selector function that combines the result object associated with the match of this regular expression, and
        ///     the match of <paramref name="other"/>, into a new result object.</param>
        /// <remarks>
        ///     <para>
        ///         It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///         cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</para>
        ///     <para>
        ///         The value of the <see cref="GenerexMatch{T}.Index"/> property of the match object passed into <paramref
        ///         name="selector"/> refers to the index within the substring, not the index within the original input
        ///         sequence.</para></remarks>
        public Stringerex<TCombinedResult> AndReverse<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult>(GenerexWithResultBase<char, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherGenerexMatch, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<char, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : StringerexMatch<TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, Stringerex<TCombinedResult>, StringerexMatch<TCombinedResult>>(substring => other.MatchReverse(substring), selector);
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the substring matched by this regular expression also
        ///     contains a match for the specified other regular expression, and if so, combines the result objects associated
        ///     with both matches using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the match object for the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the substring matched by this regular expression.</param>
        /// <param name="selector">
        ///     A selector function that combines the result objects associated with the matches of this regular expression
        ///     and <paramref name="other"/> into a new result object.</param>
        /// <remarks>
        ///     It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///     cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</remarks>
        public Stringerex<TCombinedResult> AndRaw<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult>(GenerexWithResultBase<char, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherResult, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<char, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : StringerexMatch<TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, Stringerex<TCombinedResult>, StringerexMatch<TCombinedResult>>(substring => other.Match(substring), (result, otherMatch) => selector(result, otherMatch.Result));
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the substring matched by this regular expression also fully
        ///     matches the specified other regular expression, and if so, combines the result objects associated with both
        ///     matches using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the match object for the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the substring matched by this regular expression.</param>
        /// <param name="selector">
        ///     A selector function that combines the result objects associated with the matches of this regular expression
        ///     and <paramref name="other"/> into a new result object.</param>
        /// <remarks>
        ///     It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///     cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</remarks>
        public Stringerex<TCombinedResult> AndExactRaw<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult>(GenerexWithResultBase<char, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherResult, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<char, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : StringerexMatch<TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, Stringerex<TCombinedResult>, StringerexMatch<TCombinedResult>>(substring => other.MatchExact(substring), (result, otherMatch) => selector(result, otherMatch.Result));
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the substring matched by this regular expression also
        ///     contains a match for the specified other regular expression when matching backwards, and if so, combines the
        ///     result objects associated with both matches using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match of <paramref name="other"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the match object for the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="other">
        ///     A regular expression which must match the substring matched by this regular expression.</param>
        /// <param name="selector">
        ///     A selector function that combines the result objects associated with the matches of this regular expression
        ///     and <paramref name="other"/> into a new result object.</param>
        /// <remarks>
        ///     It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see
        ///     cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</remarks>
        public Stringerex<TCombinedResult> AndReverseRaw<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult>(GenerexWithResultBase<char, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherResult, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<char, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : StringerexMatch<TOtherResult>
        {
            return and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, Stringerex<TCombinedResult>, StringerexMatch<TCombinedResult>>(substring => other.MatchReverse(substring), (result, otherMatch) => selector(result, otherMatch.Result));
        }

        /// <summary>
        ///     Returns a regular expression that matches the first regular expression followed by the second and retains the
        ///     result object generated by each match of the first regular expression.</summary>
        public static Stringerex<TResult> operator +(Stringerex<TResult> one, Stringerex two) { return one.Then(two); }

        /// <summary>
        ///     Returns a regular expression that matches the first regular expression followed by the second and retains the
        ///     result object generated by each match of the second regular expression.</summary>
        public static Stringerex<TResult> operator +(Stringerex one, Stringerex<TResult> two) { return one.Then(two); }

        /// <summary>
        ///     Returns a regular expression that casts the result object of this regular expression to a different type.</summary>
        /// <typeparam name="TOtherResult">
        ///     Type to cast the result object to.</typeparam>
        public Stringerex<TOtherResult> Cast<TOtherResult>() { return ProcessRaw(obj => (TOtherResult) (object) obj); }

        /// <summary>
        ///     Returns a regular expression that matches only if the result object of this regular expression is of the
        ///     specified type.</summary>
        /// <typeparam name="TOtherResult">
        ///     Type of result object to filter by.</typeparam>
        public Stringerex<TOtherResult> OfType<TOtherResult>() { return WhereRaw(obj => obj is TOtherResult).Cast<TOtherResult>(); }
    }
}
