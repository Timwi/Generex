using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RT.Generexes
{
    /// <summary>
    ///     Abstract base class for <see cref="Generex{T,TResult}"/> and <see cref="Stringerex{TResult}"/>.</summary>
    /// <typeparam name="T">
    ///     Type of the objects in the collection.</typeparam>
    /// <typeparam name="TResult">
    ///     Type of the result object associated with each match of the regular expression.</typeparam>
    /// <typeparam name="TGenerex">
    ///     The derived type. (Pass the type itself recursively.)</typeparam>
    /// <typeparam name="TGenerexMatch">
    ///     Type describing a match of a regular expression.</typeparam>
    public abstract class GenerexWithResultBase<T, TResult, TGenerex, TGenerexMatch> : GenerexBase<T, LengthAndResult<TResult>, TGenerex, TGenerexMatch>
        where TGenerex : GenerexWithResultBase<T, TResult, TGenerex, TGenerexMatch>
        where TGenerexMatch : GenerexMatch<T, TResult>
    {
        /// <summary>Retrieves the length of the specified <paramref name="match"/>.</summary>
        internal sealed override int getLength(LengthAndResult<TResult> match) { return match.Length; }
        /// <summary>
        ///     Returns a new match that is longer than the specified <paramref name="match"/> by the specified <paramref
        ///     name="extra"/> amount.</summary>
        protected sealed override LengthAndResult<TResult> add(LengthAndResult<TResult> match, int extra) { return match.Add(extra); }
        /// <summary>Returns the specified <paramref name="match"/>, but with its length set to zero.</summary>
        protected sealed override LengthAndResult<TResult> setZero(LengthAndResult<TResult> match) { return new LengthAndResult<TResult>(match.Result, 0); }
        internal sealed override TGenerexMatch createMatch(T[] input, int index, LengthAndResult<TResult> match) { return createMatchWithResult(match.Result, input, index, match.Length); }
        internal sealed override TGenerexMatch createBackwardsMatch(T[] input, int index, LengthAndResult<TResult> match) { return createMatchWithResult(match.Result, input, index + match.Length, -match.Length); }

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
        protected abstract TGenerexMatch createMatchWithResult(TResult result, T[] input, int index, int length);

        /// <summary>Instantiates an empty regular expression which always matches and returns the specified result object.</summary>
        public GenerexWithResultBase(TResult result) : this(new[] { new LengthAndResult<TResult>(result, 0) }) { }

        private GenerexWithResultBase(LengthAndResult<TResult>[] emptyMatch) : this((input, startIndex) => emptyMatch) { }
        private GenerexWithResultBase(matcher bothMatcher) : base(bothMatcher, bothMatcher) { }

        internal GenerexWithResultBase(matcher forward, matcher backward) : base(forward, backward) { }

        /// <summary>
        ///     Determines whether the given input sequence matches this regular expression, and if so, returns the result of
        ///     the first match.</summary>
        /// <param name="input">
        ///     Input sequence to match the regular expression against.</param>
        /// <param name="startAt">
        ///     Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>
        ///     The result of the first match in case of success; <c>default(TResult)</c> if no match.</returns>
        public TResult RawMatch(T[] input, int startAt = 0)
        {
            return RawMatches(input, startAt).FirstOrDefault();
        }

        /// <summary>
        ///     Determines whether the given input sequence matches this regular expression, and if so, returns the result of
        ///     the first match found by matching the regular expression backwards.</summary>
        /// <param name="input">
        ///     Input sequence to match the regular expression against.</param>
        /// <param name="endAt">
        ///     Optional index at which to end the search. Matches that end at or after this index are not included. (Default
        ///     is the end of the input sequence.)</param>
        /// <returns>
        ///     The result of the match in case of success; <c>default(TResult)</c> if no match.</returns>
        public TResult RawMatchReverse(T[] input, int? endAt = null)
        {
            return RawMatchesReverse(input, endAt).FirstOrDefault();
        }

        /// <summary>
        ///     Determines whether the given input sequence matches this regular expression exactly, and if so, returns the
        ///     match.</summary>
        /// <param name="input">
        ///     Input sequence to match the regular expression against.</param>
        /// <param name="mustStartAt">
        ///     Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">
        ///     Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>
        ///     The result of the match in case of success; <c>default(TResult)</c> if no match.</returns>
        public TResult RawMatchExact(T[] input, int mustStartAt = 0, int? mustEndAt = null)
        {
            return matchExact(input, mustStartAt, mustEndAt ?? input.Length, m => m.Result);
        }

        /// <summary>
        ///     Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the
        ///     specified index.</summary>
        /// <param name="input">
        ///     Input sequence to match the regular expression against.</param>
        /// <param name="startAt">
        ///     Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>
        ///     The behaviour is analogous to <see cref="Regex.Matches(string,string)"/>. The documentation for that method
        ///     claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public IEnumerable<TResult> RawMatches(T[] input, int startAt = 0)
        {
            return matches(input, startAt, (index, resultInfo) => resultInfo.Result, backward: false);
        }

        /// <summary>
        ///     Returns a sequence of non-overlapping regular expression matches going backwards, optionally starting the
        ///     search at the specified index.</summary>
        /// <param name="input">
        ///     Input sequence to match the regular expression against.</param>
        /// <param name="endAt">
        ///     Optional index at which to begin the reverse search. Matches that end at or after this index are not included.
        ///     (Default is the end of the input sequence.)</param>
        public IEnumerable<TResult> RawMatchesReverse(T[] input, int? endAt = null)
        {
            return matches(input, endAt ?? input.Length, (index, resultInfo) => resultInfo.Result, backward: true);
        }

        /// <summary>
        ///     Replaces each match of this regular expression within the given input sequence with the replacement sequence
        ///     returned by the given selector.</summary>
        /// <param name="input">
        ///     Input sequence to match the regular expression against.</param>
        /// <param name="replaceWithRaw">
        ///     Selector which, given the result of a match, returns the replacement.</param>
        /// <param name="startAt">
        ///     Optional index within the input sequence at which to start matching.</param>
        /// <param name="maxReplace">
        ///     Optional maximum number of replacements to make (null for infinite).</param>
        /// <returns>
        ///     The resulting sequence containing the replacements.</returns>
        public T[] Replace(T[] input, Func<TResult, IEnumerable<T>> replaceWithRaw, int startAt = 0, int? maxReplace = null)
        {
            return replace(input, startAt, (index, match) => replaceWithRaw(match.Result), maxReplace, backward: false);
        }

        /// <summary>
        ///     Replaces each match of this regular expression within the given input sequence, matched from the end
        ///     backwards, with the replacement sequence returned by the given selector.</summary>
        /// <param name="input">
        ///     Input sequence to match the regular expression against.</param>
        /// <param name="replaceWithRaw">
        ///     Selector which, given the result of a match, returns the replacement.</param>
        /// <param name="endAt">
        ///     Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        /// <param name="maxReplace">
        ///     Optional maximum number of replacements to make (null for infinite).</param>
        /// <returns>
        ///     The resulting sequence containing the replacements.</returns>
        public T[] ReplaceReverse(T[] input, Func<TResult, IEnumerable<T>> replaceWithRaw, int? endAt = null, int? maxReplace = null)
        {
            return replace(input, endAt ?? input.Length, (index, match) => replaceWithRaw(match.Result), maxReplace, backward: true);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, followed by the specified one, and
        ///     generates a result object that combines the result of this regular expression with the match of the other.</summary>
        protected TCombinedGenerex then<TOtherGenerex, TOtherGenerexMatch, TOtherMatch, TCombinedGenerex, TCombinedGenerexMatch, TCombinedResult>(TOtherGenerex other, Func<TResult, TOtherGenerexMatch, TCombinedResult> selector)
            where TOtherGenerex : GenerexBase<T, TOtherMatch, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T>
            where TCombinedGenerex : GenerexWithResultBase<T, TCombinedResult, TCombinedGenerex, TCombinedGenerexMatch>
            where TCombinedGenerexMatch : GenerexMatch<T, TCombinedResult>
        {
            if (other == null)
                throw new ArgumentNullException("other");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return GenerexBase<T, LengthAndResult<TCombinedResult>, TCombinedGenerex, TCombinedGenerexMatch>.Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m => other._forwardMatcher(input, startIndex + m.Length)
                    .Select(m2 => new LengthAndResult<TCombinedResult>(selector(m.Result, other.createMatch(input, startIndex + m.Length, m2)), m.Length + other.getLength(m2)))),
                (input, startIndex) => other._backwardMatcher(input, startIndex).Select(m2 => new { Match = other.createBackwardsMatch(input, startIndex, m2), Length = other.getLength(m2) })
                    .SelectMany(inf => _backwardMatcher(input, startIndex + inf.Length).Select(m => new LengthAndResult<TCombinedResult>(selector(m.Result, inf.Match), m.Length + inf.Length)))
            );
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then uses a specified <paramref
        ///     name="selector"/> to create a new regular expression from the match, then matches the new regular expression
        ///     and retains the result object from the first match.</summary>
        /// <param name="selector">
        ///     A delegate that creates a new regular expression from a match of the current regular expression.</param>
        /// <returns>
        ///     The resulting regular expression.</returns>
        /// <remarks>
        ///     Regular expressions created by this method cannot match backwards. The full set of affected methods is listed
        ///     at <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then{TOtherGenerex, TOtherMatch,
        ///     TOtherGenerexMatch}(Func{TGenerexMatch, GenerexBase{T, TOtherMatch, TOtherGenerex, TOtherGenerexMatch}})"/>.</remarks>
        public TGenerex Then<TOtherGenerex, TOtherGenerexMatch>(Func<TGenerexMatch, GenerexNoResultBase<T, TOtherGenerex, TOtherGenerexMatch>> selector)
            where TOtherGenerex : GenerexNoResultBase<T, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T>
        {
            if (selector == null)
                throw new ArgumentNullException("selector");
            return then<TGenerex, LengthAndResult<TResult>, TGenerexMatch, TGenerexMatch>(m => selector(m).process<TGenerex, TGenerexMatch, TResult>(_ => m.Result), createMatch);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, followed by the specified one, and
        ///     generates a result object that combines the original two matches.</summary>
        protected TCombinedGenerex thenRaw<TOtherGenerex, TOtherGenerexMatch, TOtherResult, TCombinedGenerex, TCombinedGenerexMatch, TCombinedResult>(
            GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch> other, Func<TResult, TOtherResult, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
            where TCombinedGenerex : GenerexWithResultBase<T, TCombinedResult, TCombinedGenerex, TCombinedGenerexMatch>
            where TCombinedGenerexMatch : GenerexMatch<T, TCombinedResult>
        {
            if (other == null)
                throw new ArgumentNullException("other");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return GenerexWithResultBase<T, TCombinedResult, TCombinedGenerex, TCombinedGenerexMatch>.Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m => other._forwardMatcher(input, startIndex + m.Length)
                    .Select(m2 => new LengthAndResult<TCombinedResult>(selector(m.Result, m2.Result), m.Length + m2.Length))),
                (input, startIndex) => other._backwardMatcher(input, startIndex).SelectMany(m2 => _backwardMatcher(input, startIndex + m2.Length)
                    .Select(m => new LengthAndResult<TCombinedResult>(selector(m.Result, m2.Result), m.Length + m2.Length)))
            );
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then uses a specified <paramref
        ///     name="selector"/> to create a new regular expression from the result of the match, and then matches the new
        ///     regular expression. This regular expression’s result object remains unaltered.</summary>
        /// <typeparam name="TOtherGenerex">
        ///     Type of the regular expression generated by the <paramref name="selector"/>.</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     Type of match object returned by matches of <typeparamref name="TOtherGenerex"/>.</typeparam>
        /// <param name="selector">
        ///     A delegate that creates a new regular expression from the result of a match of the current regular expression.</param>
        /// <returns>
        ///     The resulting regular expression.</returns>
        /// <remarks>
        ///     Regular expressions created by this method cannot match backwards. The full set of affected methods is listed
        ///     at <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then{TOtherGenerex, TOtherMatch,
        ///     TOtherGenerexMatch}(Func{TGenerexMatch, GenerexBase{T, TOtherMatch, TOtherGenerex, TOtherGenerexMatch}})"/>.</remarks>
        public TGenerex ThenRaw<TOtherGenerex, TOtherGenerexMatch>(Func<TResult, GenerexNoResultBase<T, TOtherGenerex, TOtherGenerexMatch>> selector)
            where TOtherGenerex : GenerexNoResultBase<T, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T>
        {
            if (selector == null)
                throw new ArgumentNullException("selector");
            return then<TGenerex, LengthAndResult<TResult>, TGenerexMatch, TResult>(m => selector(m).process<TGenerex, TGenerexMatch, TResult>(_ => m), (input, startIndex, match) => match.Result);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then uses a specified <paramref
        ///     name="selector"/> to create a new regular expression from the result of the match, and then matches the new
        ///     regular expression. This regular expression’s result object is replaced with the new one’s.</summary>
        /// <typeparam name="TOtherGenerex">
        ///     Type of the regular expression generated by the <paramref name="selector"/>.</typeparam>
        /// <typeparam name="TOtherResult">
        ///     Type of the result object associated with the regular expression generated by the <paramref name="selector"/>.</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     Type of match object returned by matches of <typeparamref name="TOtherGenerex"/>.</typeparam>
        /// <param name="selector">
        ///     A delegate that creates a new regular expression from the result of a match of the current regular expression.</param>
        /// <returns>
        ///     The resulting regular expression.</returns>
        /// <remarks>
        ///     Regular expressions created by this method cannot match backwards. The full set of affected methods is listed
        ///     at <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then{TOtherGenerex, TOtherMatch,
        ///     TOtherGenerexMatch}(Func{TGenerexMatch, GenerexBase{T, TOtherMatch, TOtherGenerex, TOtherGenerexMatch}})"/>.</remarks>
        public TOtherGenerex ThenRaw<TOtherGenerex, TOtherResult, TOtherGenerexMatch>(Func<TResult, GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>> selector)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            if (selector == null)
                throw new ArgumentNullException("selector");
            return then<TOtherGenerex, LengthAndResult<TOtherResult>, TOtherGenerexMatch, TResult>(m => selector(m), (input, startIndex, match) => match.Result);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then uses a specified <paramref
        ///     name="selector"/> to create a new regular expression from the match; then attempts to match the new regular
        ///     expression and throws an exception if that regular expression fails to match. This regular expression’s result
        ///     object remains unaltered.</summary>
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
        public TGenerex ThenExpect<TOtherGenerex, TOtherGenerexMatch>(Func<TGenerexMatch, GenerexNoResultBase<T, TOtherGenerex, TOtherGenerexMatch>> selector, Func<TGenerexMatch, Exception> exceptionGenerator)
            where TOtherGenerex : GenerexNoResultBase<T, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T>
        {
            if (selector == null)
                throw new ArgumentNullException("selector");
            return Then(m => selector(m).expect(() => exceptionGenerator(m)));
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then uses a specified <paramref
        ///     name="selector"/> to create a new regular expression from the result object; then attempts to match the new
        ///     regular expression and throws an exception if that regular expression fails to match. This regular
        ///     expression’s result object remains unaltered.</summary>
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
        public TGenerex ThenExpectRaw<TOtherGenerex, TOtherGenerexMatch>(Func<TResult, GenerexNoResultBase<T, TOtherGenerex, TOtherGenerexMatch>> selector, Func<TResult, Exception> exceptionGenerator)
            where TOtherGenerex : GenerexNoResultBase<T, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T>
        {
            if (selector == null)
                throw new ArgumentNullException("selector");
            return ThenRaw(r => selector(r).expect(() => exceptionGenerator(r)));
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression, then uses a specified <paramref
        ///     name="selector"/> to create a new regular expression from the result object; then attempts to match the new
        ///     regular expression and throws an exception if that regular expression fails to match. The new regular
        ///     expression’s result object replaces the current one’s.</summary>
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
        public TOtherGenerex ThenExpectRaw<TOtherGenerex, TOtherResult, TOtherGenerexMatch>(Func<TResult, GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>> selector, Func<TResult, Exception> exceptionGenerator)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            if (selector == null)
                throw new ArgumentNullException("selector");
            return ThenRaw(r => selector(r).expect(() => exceptionGenerator(r)));
        }

        /// <summary>
        ///     Returns a regular expression that matches either this regular expression or a single element that satisfies
        ///     the specified predicate (cf. <c>|</c> in traditional regular expression syntax).</summary>
        /// <param name="predicate">
        ///     The predicate to match a single element against.</param>
        /// <param name="selector">
        ///     A selector that returns the result object for the new regular expression based on the element matched by the
        ///     <paramref name="predicate"/>.</param>
        public TGenerex Or(Predicate<T> predicate, Func<GenerexMatch<T>, TResult> selector)
        {
            var other = new Generex<T>(predicate).Process(selector);
            return Or(Constructor(new matcher(other._forwardMatcher), new matcher(other._backwardMatcher)));
        }

        /// <summary>
        ///     Returns a regular expression that matches either this regular expression or a single element using the
        ///     specified equality comparer (cf. <c>|</c> in traditional regular expression syntax).</summary>
        /// <param name="element">
        ///     The element to match.</param>
        /// <param name="selector">
        ///     A selector that returns the result object for the new regular expression based on the <paramref
        ///     name="element"/> matched.</param>
        /// <param name="comparer">
        ///     An optional equality comparer to use against the <paramref name="element"/>.</param>
        public TGenerex Or(T element, Func<GenerexMatch<T>, TResult> selector, IEqualityComparer<T> comparer = null)
        {
            var other = new Generex<T>(comparer ?? EqualityComparer<T>.Default, element).Process(selector);
            return Or(Constructor(new matcher(other._forwardMatcher), new matcher(other._backwardMatcher)));
        }

        /// <summary>
        ///     Returns a regular expression that matches either this regular expression or the specified sequence of elements
        ///     (cf. <c>|</c> in traditional regular expression syntax).</summary>
        /// <param name="elements">
        ///     The sequence of elements to match.</param>
        /// <param name="selector">
        ///     A selector that returns the result object for the new regular expression based on the sequence matched by the
        ///     <paramref name="elements"/>.</param>
        /// <param name="comparer">
        ///     An optional equality comparer to use against the elements in <paramref name="elements"/>.</param>
        public TGenerex Or(IEnumerable<T> elements, Func<GenerexMatch<T>, TResult> selector, IEqualityComparer<T> comparer = null)
        {
            var other = new Generex<T>(comparer ?? EqualityComparer<T>.Default, elements).Process(selector);
            return Or(Constructor(new matcher(other._forwardMatcher), new matcher(other._backwardMatcher)));
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression zero or more times.</summary>
        /// <param name="greedy">
        ///     <c>true</c> to prioritise longer matches (“greedy” matching); <c>false</c> to prioritise shorter matches
        ///     (“non-greedy” matching).</param>
        protected TManyGenerex repeatInfinite<TManyGenerex, TManyGenerexMatch>(bool greedy)
            where TManyGenerex : GenerexWithResultBase<T, IEnumerable<TResult>, TManyGenerex, TManyGenerexMatch>
            where TManyGenerexMatch : GenerexMatch<T, IEnumerable<TResult>>
        {
            var createRepeatInfiniteMatcher = InternalExtensions.Lambda((matcher inner) =>
            {
                GenerexWithResultBase<T, IEnumerable<TResult>, TManyGenerex, TManyGenerexMatch>.matcher newMatcher = null;
                if (greedy)
                    newMatcher = (input, startIndex) => inner(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m.Length)
                        .Select(m2 => new LengthAndResult<IEnumerable<TResult>>(m.Result.Concat(m2.Result), m.Length + m2.Length)))
                        .Concat(new LengthAndResult<IEnumerable<TResult>>(Enumerable.Empty<TResult>(), 0));
                else
                    newMatcher = (input, startIndex) => new LengthAndResult<IEnumerable<TResult>>(Enumerable.Empty<TResult>(), 0)
                        .Concat(inner(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m.Length)
                        .Select(m2 => new LengthAndResult<IEnumerable<TResult>>(m.Result.Concat(m2.Result), m.Length + m2.Length))));
                return newMatcher;
            });
            return GenerexWithResultBase<T, IEnumerable<TResult>, TManyGenerex, TManyGenerexMatch>.Constructor(
                createRepeatInfiniteMatcher(_forwardMatcher),
                createRepeatInfiniteMatcher(_backwardMatcher));
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression the specified number of times or more.</summary>
        /// <param name="min">
        ///     Minimum number of times the regular expression must match.</param>
        /// <param name="greedy">
        ///     <c>true</c> to prioritise longer matches (“greedy” matching); <c>false</c> to prioritise shorter matches
        ///     (“non-greedy” matching).</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="min"/> is negative.</exception>
        protected TManyGenerex repeatMin<TManyGenerex, TManyGenerexMatch>(int min, bool greedy)
            where TManyGenerex : GenerexWithResultBase<T, IEnumerable<TResult>, TManyGenerex, TManyGenerexMatch>
            where TManyGenerexMatch : GenerexMatch<T, IEnumerable<TResult>>
        {
            if (min < 0) throw new ArgumentOutOfRangeException("'min' cannot be negative.", "min");
            return repeatBetween<TManyGenerex, TManyGenerexMatch>(min, min, true)
                .thenRaw<TManyGenerex, TManyGenerexMatch, IEnumerable<TResult>, TManyGenerex, TManyGenerexMatch, IEnumerable<TResult>>(repeatInfinite<TManyGenerex, TManyGenerexMatch>(greedy), Enumerable.Concat);
        }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression any number of times within specified
        ///     boundaries.</summary>
        /// <param name="min">
        ///     Minimum number of times to match.</param>
        /// <param name="max">
        ///     Maximum number of times to match.</param>
        /// <param name="greedy">
        ///     <c>true</c> to prioritise longer matches (“greedy” matching); <c>false</c> to prioritise shorter matches
        ///     (“non-greedy” matching).</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="min"/> is negative.</exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="max"/> is smaller than <paramref name="min"/>.</exception>
        protected TManyGenerex repeatBetween<TManyGenerex, TManyGenerexMatch>(int min, int max, bool greedy)
            where TManyGenerex : GenerexWithResultBase<T, IEnumerable<TResult>, TManyGenerex, TManyGenerexMatch>
            where TManyGenerexMatch : GenerexMatch<T, IEnumerable<TResult>>
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
            return GenerexWithResultBase<T, IEnumerable<TResult>, TManyGenerex, TManyGenerexMatch>.Constructor(rm.ForwardMatcher, rm.BackwardMatcher);
        }

        private sealed class repeatMatcher
        {
            public int MinTimes;
            public int MaxTimes;
            public bool Greedy;
            public matcher InnerForwardMatcher;
            public matcher InnerBackwardMatcher;
            public IEnumerable<LengthAndResult<IEnumerable<TResult>>> ForwardMatcher(T[] input, int startIndex) { return matcher(input, startIndex, 0, backward: false); }
            public IEnumerable<LengthAndResult<IEnumerable<TResult>>> BackwardMatcher(T[] input, int startIndex) { return matcher(input, startIndex, 0, backward: true); }
            private IEnumerable<LengthAndResult<IEnumerable<TResult>>> matcher(T[] input, int startIndex, int iteration, bool backward)
            {
                if (!Greedy && iteration >= MinTimes)
                    yield return new LengthAndResult<IEnumerable<TResult>>(Enumerable.Empty<TResult>(), 0);
                if (iteration < MaxTimes)
                {
                    foreach (var m in (backward ? InnerBackwardMatcher : InnerForwardMatcher)(input, startIndex))
                        foreach (var m2 in matcher(input, startIndex + m.Length, iteration + 1, backward))
                            yield return new LengthAndResult<IEnumerable<TResult>>(
                                backward ? m2.Result.Concat(m.Result) : m.Result.Concat(m2.Result),
                                m.Length + m2.Length
                            );
                }
                if (Greedy && iteration >= MinTimes)
                    yield return new LengthAndResult<IEnumerable<TResult>>(Enumerable.Empty<TResult>(), 0);
            }
        }

        /// <summary>Executes the specified code every time the regular expression engine encounters this expression.</summary>
        public TGenerex DoRaw(Action<TResult> code)
        {
            return Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => { code(m.Result); return m; }),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => { code(m.Result); return m; })
            );
        }

        /// <summary>Restricts matches of this regular expression to those that satisfy the specified predicate.</summary>
        public TGenerex WhereRaw(Func<TResult, bool> code)
        {
            return Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => code(m.Result)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => code(m.Result))
            );
        }

        /// <summary>
        ///     Turns the current regular expression into a zero-width negative look-ahead assertion (cf. <c>(?!...)</c> in
        ///     traditional regular expression syntax), which returns the specified default result in case of a match.</summary>
        public TGenerex LookAheadNegative(TResult defaultMatch) { return lookNegative(behind: false, defaultMatch: new[] { new LengthAndResult<TResult>(defaultMatch, 0) }); }
        /// <summary>
        ///     Turns the current regular expression into a zero-width negative look-behind assertion (cf. <c>(?&lt;!...)</c>
        ///     in traditional regular expression syntax), which returns the specified default result in case of a match.</summary>
        public TGenerex LookBehindNegative(TResult defaultMatch) { return lookNegative(behind: true, defaultMatch: new[] { new LengthAndResult<TResult>(defaultMatch, 0) }); }

        /// <summary>
        ///     Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TOtherGenerex">
        ///     Generex type to return.</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     Generex match type that corresponds to <typeparamref name="TOtherGenerex"/></typeparam>
        /// <typeparam name="TOtherResult">
        ///     Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">
        ///     Function to process a regular expression match.</param>
        protected TOtherGenerex process<TOtherGenerex, TOtherGenerexMatch, TOtherResult>(Func<TGenerexMatch, TOtherResult> selector)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>.Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => new LengthAndResult<TOtherResult>(selector(createMatch(input, startIndex, m)), m.Length)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => new LengthAndResult<TOtherResult>(selector(createBackwardsMatch(input, startIndex, m)), m.Length))
            );
        }

        /// <summary>
        ///     Processes each match of this regular expression by running each result object through a provided selector.</summary>
        /// <typeparam name="TOtherGenerex">
        ///     Generex type to return.</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     Generex match type that corresponds to <typeparamref name="TOtherGenerex"/></typeparam>
        /// <typeparam name="TOtherResult">
        ///     Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">
        ///     Function to process a regular expression match.</param>
        protected TOtherGenerex processRaw<TOtherGenerex, TOtherGenerexMatch, TOtherResult>(Func<TResult, TOtherResult> selector)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
        {
            return GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>.Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => new LengthAndResult<TOtherResult>(selector(m.Result), m.Length)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => new LengthAndResult<TOtherResult>(selector(m.Result), m.Length))
            );
        }

        /// <summary>
        ///     Returns a regular expression that only matches if the subarray matched by this regular expression also matches
        ///     the specified matching function, and if so, combines the first match’s result object with the second match
        ///     using a specified selector.</summary>
        /// <typeparam name="TOtherResult">
        ///     The type of the result object associated with each match returned by <paramref name="innerMatch"/>.</typeparam>
        /// <typeparam name="TOtherGenerex">
        ///     The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see
        ///     cref="Stringerex{TResult}"/>, but with <typeparamref name="TOtherResult"/> in place of <c>TResult</c>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">
        ///     The type of the match object for the other regular expression. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>, but with <typeparamref
        ///     name="TOtherResult"/> in place of <c>TResult</c>.)</typeparam>
        /// <typeparam name="TCombinedResult">
        ///     The type of the combined result object returned by <paramref name="selector"/>.</typeparam>
        /// <typeparam name="TCombinedGenerex">
        ///     The type of the new regular expression to be returned. (This is either <see cref="Generex{T,TResult}"/> or
        ///     <see cref="Stringerex{TResult}"/>, but with <typeparamref name="TCombinedResult"/> in place of
        ///     <c>TResult</c>.)</typeparam>
        /// <typeparam name="TCombinedGenerexMatch">
        ///     The type of the match object for the regular expression to be returned. (This is either <see
        ///     cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>, but with <typeparamref
        ///     name="TCombinedResult"/> in place of <c>TResult</c>.)</typeparam>
        /// <param name="innerMatch">
        ///     A function that runs on the subarray matched by this regular expression and returns either a match or
        ///     <c>null</c>.</param>
        /// <param name="selector">
        ///     A selector function that combines the result object associated with the match of this regular expression, and
        ///     the match returned by <paramref name="innerMatch"/>, into a new result object.</param>
        /// <remarks>
        ///     The match object passed into <paramref name="selector"/> is the same that <paramref name="innerMatch"/>
        ///     returned. Therefore, its <see cref="GenerexMatch{T}.Index"/> property refers to the index within the subarray,
        ///     not the original input sequence.</remarks>
        protected TCombinedGenerex and<TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult, TCombinedGenerex, TCombinedGenerexMatch>(Func<T[], TOtherGenerexMatch> innerMatch, Func<TResult, TOtherGenerexMatch, TCombinedResult> selector)
            where TOtherGenerex : GenerexWithResultBase<T, TOtherResult, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T, TOtherResult>
            where TCombinedGenerex : GenerexWithResultBase<T, TCombinedResult, TCombinedGenerex, TCombinedGenerexMatch>
            where TCombinedGenerexMatch : GenerexMatch<T, TCombinedResult>
        {
            return GenerexWithResultBase<T, TCombinedResult, TCombinedGenerex, TCombinedGenerexMatch>.Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m =>
                {
                    var subarray = input.Subarray(startIndex, getLength(m));
                    var submatch = innerMatch(subarray);
                    return submatch == null ? Enumerable.Empty<LengthAndResult<TCombinedResult>>() : new[] { new LengthAndResult<TCombinedResult>(selector(m.Result, submatch), m.Length) };
                }),
                (input, startIndex) => _backwardMatcher(input, startIndex).SelectMany(m =>
                {
                    var length = getLength(m);
                    var subarray = input.Subarray(startIndex + length, -length);
                    var submatch = innerMatch(subarray);
                    return submatch == null ? Enumerable.Empty<LengthAndResult<TCombinedResult>>() : new[] { new LengthAndResult<TCombinedResult>(selector(m.Result, submatch), m.Length) };
                })
            );
        }

        /// <summary>
        ///     Returns a regular expression that matches the specified regular expression (first operand) followed by the
        ///     specified element (second operand).</summary>
        public static TGenerex operator +(GenerexWithResultBase<T, TResult, TGenerex, TGenerexMatch> one, T two) { return one.Then(two); }

        /// <summary>
        ///     Returns a regular expression that matches the specified element (first operand) followed by the specified
        ///     regular expression (second operand).</summary>
        public static TGenerex operator +(T one, GenerexWithResultBase<T, TResult, TGenerex, TGenerexMatch> two) { return new Generex<T>(one).Then(two); }

        /// <summary>
        ///     Returns a regular expression that matches the specified regular expression (first operand) followed by a
        ///     single element that satisfies the specified predicate (second operand).</summary>
        public static TGenerex operator +(GenerexWithResultBase<T, TResult, TGenerex, TGenerexMatch> one, Predicate<T> two) { return one.Then(two); }

        /// <summary>
        ///     Returns a regular expression that matches a single element that satisfies the specified predicate (first
        ///     operand) followed by the specified regular expression (second operand).</summary>
        public static TGenerex operator +(Predicate<T> one, GenerexWithResultBase<T, TResult, TGenerex, TGenerexMatch> two) { return new Generex<T>(one).Then(two); }

        /// <summary>
        ///     Returns a regular expression that matches either one of the specified regular expressions (cf. <c>|</c> in
        ///     traditional regular expression syntax).</summary>
        public static TGenerex operator |(GenerexWithResultBase<T, TResult, TGenerex, TGenerexMatch> one, TGenerex two) { return one.Or(two); }
    }
}
