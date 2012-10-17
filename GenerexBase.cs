using System;
using System.Collections.Generic;
using System.Linq;
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

        internal abstract int getLength(TMatch match);
        internal abstract TMatch add(TMatch match, int extra);
        internal abstract TGenerex create(matcher forward, matcher backward);
        internal abstract TGenerexMatch createMatch(T[] input, int index, TMatch match);

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

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>A <see cref="GenerexMatch{T}"/> object describing a regular expression match in case of success; null if no match.</returns>
        public TGenerexMatch Match(T[] input, int startAt = 0)
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
        public TGenerexMatch MatchExact(T[] input, int mustStartAt = 0, int? mustEndAt = null)
        {
            return matchExact(input, mustStartAt, mustEndAt ?? input.Length, match => createMatch(input, mustStartAt, match));
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match
        /// found by matching the regular expression backwards (starting from the end of the input sequence).
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>A <see cref="GenerexMatch{T}"/> object describing a regular expression match in case of success; null if no match.</returns>
        public TGenerexMatch MatchReverse(T[] input, int? endAt = null)
        {
            return MatchesReverse(input, endAt).FirstOrDefault();
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the specified
        /// input sequence), optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        public IEnumerable<TGenerexMatch> MatchesReverse(T[] input, int? endAt = null)
        {
            return matches(input, endAt ?? input.Length, (index, match) => createMatch(input, index, match), backward: true);
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>The behaviour is analogous to <see cref="Regex.Matches(string,string)"/>.
        /// The documentation for that method claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public IEnumerable<TGenerexMatch> Matches(T[] input, int startAt = 0)
        {
            return matches(input, startAt, (index, match) => createMatch(input, index, match), backward: false);
        }

        /// <summary>
        /// Returns a regular expression that matches a consecutive sequence of regular expressions, beginning with this one, followed by the specified ones.
        /// </summary>
        public TGenerex Then(params Generex<T>[] other)
        {
            return create(
                other.Select(o => o._forwardMatcher).Aggregate(_forwardMatcher, then),
                then(other.Reverse().Select(o => o._backwardMatcher).Aggregate(thenSimple), _backwardMatcher)
            );
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements.
        /// </summary>
        public TGenerex Then(params T[] elements) { return Then(new Generex<T>(elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements.
        /// </summary>
        public TGenerex Then(IEnumerable<T> elements) { return Then(new Generex<T>(elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements, using the specified equality comparer.
        /// </summary>
        public TGenerex Then(IEqualityComparer<T> comparer, params T[] elements) { return Then(new Generex<T>(comparer, elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified sequence of elements, using the specified equality comparer.
        /// </summary>
        public TGenerex Then(IEqualityComparer<T> comparer, IEnumerable<T> elements) { return Then(new Generex<T>(comparer, elements)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by a single element that satisfies the specified predicate.
        /// </summary>
        public TGenerex Then(Predicate<T> predicate) { return Then(new Generex<T>(predicate)); }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified other regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public TGenerex Or(TGenerex other)
        {
            return create(or(_forwardMatcher, other._forwardMatcher), or(_backwardMatcher, other._backwardMatcher));
        }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static TGenerex Ors(params TGenerex[] other) { return other.Aggregate((a, b) => a.Or(b)); }

        /// <summary>Matches this regular expression atomically (without backtracking into it) (cf. "(?>...)" in traditional regular expression syntax).</summary>
        public TGenerex Atomic()
        {
            return create(
                (input, startIndex) => _forwardMatcher(input, startIndex).Take(1),
                (input, startIndex) => _backwardMatcher(input, startIndex).Take(1)
            );
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

        /// <summary>
        /// Generates a matcher that matches the <paramref name="first"/> regular expression followed by the <paramref name="second"/> regular expression.
        /// </summary>
        internal static Generex<T>.matcher thenSimple(Generex<T>.matcher first, Generex<T>.matcher second)
        {
            return (input, startIndex) => first(input, startIndex).SelectMany(m => second(input, startIndex + m).Select(m2 => m + m2));
        }
    }
}
