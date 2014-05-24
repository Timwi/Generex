using System;
using System.Collections.Generic;
using System.Linq;

namespace RT.Generexes
{
    /// <summary>Abstract base class for all Generex regular expressions.</summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    /// <typeparam name="TMatch">Either <c>int</c> or <see cref="LengthAndResult{TResult}"/>.</typeparam>
    /// <typeparam name="TGenerex">The derived type. (Pass the type itself recursively.)</typeparam>
    /// <typeparam name="TGenerexMatch">Type describing a match of a regular expression.</typeparam>
    public abstract class GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
        where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
        where TGenerexMatch : GenerexMatch<T>
    {
        internal delegate IEnumerable<TMatch> matcher(T[] input, int startIndex);
        internal matcher _forwardMatcher, _backwardMatcher;

        private static Func<matcher, matcher, TGenerex> _constructor;
        internal static Func<matcher, matcher, TGenerex> Constructor
        {
            get
            {
                if (_constructor == null)
                {
                    if (typeof(TGenerex).TypeInitializer != null)
                        typeof(TGenerex).TypeInitializer.Invoke(null, null);
                    if (_constructor == null)
                        throw new InvalidOperationException(string.Format("The static constructor of {0} didn’t initialize the Constructor field.", typeof(TGenerex)));
                }
                return _constructor;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _constructor = value;
            }
        }

        /// <summary>Retrieves the length of the specified <paramref name="match"/>.</summary>
        protected abstract int getLength(TMatch match);
        /// <summary>Returns a new match that is longer than the specified <paramref name="match"/> by the specified <paramref name="extra"/> amount.</summary>
        protected abstract TMatch add(TMatch match, int extra);
        /// <summary>Returns the specified <paramref name="match"/>, but with its length set to zero.</summary>
        protected abstract TMatch setZero(TMatch match);
        internal abstract TGenerexMatch createMatch(T[] input, int index, TMatch match);
        internal abstract TGenerexMatch createBackwardsMatch(T[] input, int index, TMatch match);

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
        /// Determines whether the given input sequence matches this regular expression at a specific index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <returns><c>true</c> if a match starting at the specified index exists (which need not run all the way to the end of the sequence); otherwise, <c>false</c>.</returns>
        public bool IsMatchAt(T[] input, int mustStartAt = 0)
        {
            return _forwardMatcher(input, mustStartAt).Any();
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression up to a specific index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns><c>true</c> if a match ending at the specified index exists (which need not begin at the start of the sequence); otherwise, <c>false</c>.</returns>
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
        /// <returns><c>true</c> if a match starting and ending at the specified indexes exists; otherwise, <c>false</c>.</returns>
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
        /// <returns>An object describing a regular expression match in case of success; <c>null</c> if no match.</returns>
        public TGenerexMatch Match(T[] input, int startAt = 0)
        {
            return Matches(input, startAt).FirstOrDefault();
        }

        /// <summary>Performs replacements of the non-overlapping matches of the current regular expression within the specified <paramref name="input"/> array.</summary>
        /// <param name="input">The input array to match against.</param>
        /// <param name="startAt">Index at which to start the search. Depending on <paramref name="backward"/>, the
        /// matching behaviour is like that of <see cref="Matches"/> or <see cref="MatchesReverse"/>.</param>
        /// <param name="replaceWith">Delegate that determines what to replace each match with.</param>
        /// <param name="maxReplace">Maximum number of replacements to make, or <c>null</c> for unlimited.</param>
        /// <param name="backward"><c>true</c> to perform a reverse match (starting from the end of the array and going backwards); <c>false</c> to match forward.</param>
        /// <returns>The new array with the replacements performed.</returns>
        protected T[] replace(T[] input, int startAt, Func<int, TMatch, IEnumerable<T>> replaceWith, int? maxReplace, bool backward)
        {
            var result = new List<T>(backward ? input.Skip(startAt) : input.Take(startAt));
            var prevIndex = startAt;
            var matchesIter = matches(input, startAt, (i, match) => new { Index = i, Match = match }, backward);
            foreach (var match in maxReplace == null ? matchesIter : matchesIter.Take(maxReplace.Value))
            {
                if (backward)
                {
                    result.InsertRange(0, input.Skip(match.Index).Take(prevIndex - match.Index));
                    result.InsertRange(0, replaceWith(match.Index, match.Match));
                }
                else
                {
                    result.AddRange(input.Skip(prevIndex).Take(match.Index - prevIndex));
                    result.AddRange(replaceWith(match.Index, match.Match));
                }
                prevIndex = match.Index + getLength(match.Match);
            }

            return backward
                ? input.Take(prevIndex).Concat(result).ToArray()
                : result.Concat(input.Skip(prevIndex)).ToArray();
        }

        /// <summary>
        /// Replaces each match of this regular expression within the given input sequence with the replacement sequence returned by the given selector.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="replaceWith">Selector which, given a match within the input sequence, returns the replacement.</param>
        /// <param name="startAt">Optional index within the input sequence at which to start matching.</param>
        /// <param name="maxReplace">Optional maximum number of replacements to make (null for infinite).</param>
        /// <returns>The resulting sequence containing the replacements.</returns>
        public T[] Replace(T[] input, Func<TGenerexMatch, IEnumerable<T>> replaceWith, int startAt = 0, int? maxReplace = null)
        {
            return replace(input, startAt, (index, match) => replaceWith(createMatch(input, index, match)), maxReplace, backward: false);
        }

        /// <summary>
        /// Replaces each match of this regular expression within the given input sequence with a replacement sequence.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="replaceWith">The sequence to replace each match with.</param>
        /// <param name="startAt">Optional index within the input sequence at which to start matching.</param>
        /// <param name="maxReplace">Optional maximum number of replacements to make (null for infinite).</param>
        /// <returns>The resulting sequence containing the replacements.</returns>
        /// <remarks>
        /// The <paramref name="replaceWith"/> enumerable is only enumerated if a replacement is actually made.
        /// If multiple replacements are made, it is enumerated only once.
        /// </remarks>
        public T[] Replace(T[] input, IEnumerable<T> replaceWith, int startAt = 0, int? maxReplace = null)
        {
            // evaluate replaceWith only once, and only if necessary
            T[] replaceWithArray = null;
            return replace(input, startAt, (index, match) => replaceWithArray ?? (replaceWithArray = replaceWith.ToArray()), maxReplace, backward: false);
        }

        /// <summary>
        /// Replaces each match of this regular expression within the given input sequence, matched from the end backwards,
        /// with the replacement sequence returned by the given selector.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="replaceWith">Selector which, given a match within the input sequence, returns the replacement.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        /// <param name="maxReplace">Optional maximum number of replacements to make (null for infinite).</param>
        /// <returns>The resulting sequence containing the replacements.</returns>
        public T[] ReplaceReverse(T[] input, Func<TGenerexMatch, T[]> replaceWith, int? endAt = null, int? maxReplace = null)
        {
            return replace(input, endAt ?? input.Length, (index, match) => replaceWith(createBackwardsMatch(input, index, match)), maxReplace, backward: true);
        }

        /// <summary>
        /// Replaces each match of this regular expression within the given input sequence, matched from the end backwards,
        /// with a replacement sequence.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="replaceWith">The sequence to replace each match with.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        /// <param name="maxReplace">Optional maximum number of replacements to make (null for infinite).</param>
        /// <returns>The resulting sequence containing the replacements.</returns>
        /// The <paramref name="replaceWith"/> enumerable is only enumerated if a replacement is actually made.
        /// If multiple replacements are made, it is enumerated only once.
        public T[] ReplaceReverse(T[] input, IEnumerable<T> replaceWith, int? endAt = null, int? maxReplace = null)
        {
            // evaluate replaceWith only once, and only if necessary
            T[] replaceWithArray = null;
            return replace(input, endAt ?? input.Length, (index, match) => replaceWithArray ?? (replaceWithArray = replaceWith.ToArray()), maxReplace, backward: true);
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression exactly, and if so, returns information about the match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>An object describing the regular expression match in case of success; <c>null</c> if no match.</returns>
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
        /// <returns>An object describing a regular expression match in case of success; <c>null</c> if no match.</returns>
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
            return matches(input, endAt ?? input.Length, (index, match) => createBackwardsMatch(input, index, match), backward: true);
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>The behaviour is analogous to <see cref="System.Text.RegularExpressions.Regex.Matches(string,string)"/>.
        /// The documentation for that method claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public IEnumerable<TGenerexMatch> Matches(T[] input, int startAt = 0)
        {
            return matches(input, startAt, (index, match) => createMatch(input, index, match), backward: false);
        }

        /// <summary>
        /// Returns a regular expression that matches a consecutive sequence of regular expressions, beginning with this one, followed by the specified ones.
        /// </summary>
        public TGenerex Then<TGenerex2, TGenerexMatch2>(params GenerexNoResultBase<T, TGenerex2, TGenerexMatch2>[] other)
            where TGenerex2 : GenerexNoResultBase<T, TGenerex2, TGenerexMatch2>
            where TGenerexMatch2 : GenerexMatch<T>
        {
            var backwardFirst = other.Reverse().Select(o => o._backwardMatcher).Aggregate(GenerexNoResultBase<T, TGenerex2, TGenerexMatch2>.then);
            return Constructor(
                other.Select(o => o._forwardMatcher).Aggregate(_forwardMatcher, (prev, next) => (input, startIndex) => prev(input, startIndex).SelectMany(m => next(input, startIndex + getLength(m)).Select(m2 => add(m, m2)))),
                (input, startIndex) => backwardFirst(input, startIndex).SelectMany(m => _backwardMatcher(input, startIndex + m).Select(m2 => add(m2, m)))
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
        /// Returns a regular expression that matches either this regular expression or the specified other regular expression (cf. <c>|</c> in traditional regular expression syntax).
        /// </summary>
        public TGenerex Or<TOtherGenerex, TOtherGenerexMatch>(GenerexBase<T, TMatch, TOtherGenerex, TOtherGenerexMatch> other)
            where TOtherGenerex : GenerexBase<T, TMatch, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T>
        {
            return Constructor(or(_forwardMatcher, other._forwardMatcher), or(_backwardMatcher, other._backwardMatcher));
        }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. <c>|</c> in traditional regular expression syntax).
        /// </summary>
        public static TGenerex Ors(params TGenerex[] other) { return other.Aggregate((prev, next) => prev.Or(next)); }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. <c>|</c> in traditional regular expression syntax).
        /// </summary>
        public static TGenerex Ors(IEnumerable<TGenerex> other) { return other.Aggregate((prev, next) => prev.Or(next)); }

        /// <summary>Matches this regular expression atomically (without backtracking into it) (cf. <c>(?&gt;...)</c> in traditional regular expression syntax).</summary>
        public TGenerex Atomic()
        {
            return Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).Take(1),
                (input, startIndex) => _backwardMatcher(input, startIndex).Take(1)
            );
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the specified index.
        /// </summary>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Index at which to start the search. Depending on <paramref name="backward"/>, the
        /// matching behaviour is like that of <see cref="Matches"/> or <see cref="MatchesReverse"/>.</param>
        /// <param name="selector">A delegate that processes each match into a result object.</param>
        /// <param name="backward"><c>true</c> to perform a reverse match (starting from the end of the array and going backwards); <c>false</c> to match forward.</param>
        /// <returns>The set of result objects generated by <paramref name="selector"/> for each match.</returns>
        protected IEnumerable<TResult> matches<TResult>(T[] input, int startAt, Func<int, TMatch, TResult> selector, bool backward)
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

        /// <summary>Returns a single match that starts and ends at the specified indexes within the <paramref name="input"/> array.</summary>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="mustStartAt">Index within <paramref name="input"/> at which the match must start.</param>
        /// <param name="mustEndAt">Index within <paramref name="input"/> at which the match must end.</param>
        /// <param name="selector">Delegate that transforms the match (if found) into a result object.</param>
        /// <returns>The result object returned by <paramref name="selector"/> if a match was found; <c>default(TResult)</c> otherwise.</returns>
        protected TResult matchExact<TResult>(T[] input, int mustStartAt, int mustEndAt, Func<TMatch, TResult> selector)
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

        internal static matcher or<TOtherGenerex, TOtherGenerexMatch>(matcher one, GenerexBase<T, TMatch, TOtherGenerex, TOtherGenerexMatch>.matcher two)
            where TOtherGenerex : GenerexBase<T, TMatch, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T>
        {
            return new safeOrMatcher<TOtherGenerex, TOtherGenerexMatch>(one, two).Matcher;
        }

        /// <summary>
        /// This class implements the “or” (or alternation) operation without invoking both matchers at the start.
        /// (This is important in cases involving recursive regular expressions, see e.g. <see cref="Generex.Recursive{T}"/>.)
        /// </summary>
        private class safeOrMatcher<TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerex : GenerexBase<T, TMatch, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T>
        {
            public matcher One { get; private set; }
            public GenerexBase<T, TMatch, TOtherGenerex, TOtherGenerexMatch>.matcher Two { get; private set; }
            public safeOrMatcher(matcher one, GenerexBase<T, TMatch, TOtherGenerex, TOtherGenerexMatch>.matcher two)
            {
                One = one;
                Two = two;
            }
            public IEnumerable<TMatch> Matcher(T[] input, int startIndex)
            {
                foreach (var match in One(input, startIndex))
                    yield return match;
                foreach (var match in Two(input, startIndex))
                    yield return match;
            }
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. (This always matches successfully and all matches are zero-length.)
        /// </summary>
        public TGenerex Do(Action code)
        {
            return Constructor(
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
        /// Generex&lt;char&gt; myRe = someRe.Then(someOtherRe.Do(m => { captured = new string(m.Match); })).Then(yetAnotherRe);
        /// foreach (var m in myRe.Matches(input))
        ///     Console.WriteLine("Captured text: {0}", captured);
        /// </code>
        /// </example>
        public TGenerex Do(Action<TGenerexMatch> code)
        {
            return Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => { code(createMatch(input, startIndex, m)); return m; }),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => { code(createBackwardsMatch(input, startIndex, m)); return m; })
            );
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. The return value of the specified code determines whether the expression matches successfully (all matches are zero-length).
        /// </summary>
        public TGenerex Do(Func<bool> code)
        {
            return Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => code()),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => code())
            );
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. The return value of the specified code determines whether the expression matches successfully (all matches are zero-length).
        /// </summary>
        public TGenerex Do(Func<TGenerexMatch, bool> code)
        {
            return Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => code(createMatch(input, startIndex, m))),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => code(createBackwardsMatch(input, startIndex, m)))
            );
        }

        /// <summary>Turns the current regular expression into a zero-width positive look-ahead assertion (cf. <c>(?=...)</c> in traditional regular expression syntax).</summary>
        public TGenerex LookAhead() { return look(behind: false); }
        /// <summary>Turns the current regular expression into a zero-width positive look-behind assertion (cf. <c>(?&lt;=...)</c> in traditional regular expression syntax).</summary>
        public TGenerex LookBehind() { return look(behind: true); }

        /// <summary>Turns the current regular expression into a zero-width positive (look-ahead or look-behind) assertion.</summary>
        /// <param name="behind"><c>true</c> to create a look-behind assertion; <c>false</c> to create a look-ahead assertion.</param>
        private TGenerex look(bool behind)
        {
            // In a look-*behind* assertion, both matchers use the _backwardMatcher. Similarly, look-*ahead* assertions always use _forwardMatcher.
            matcher innerMatcher = behind ? _backwardMatcher : _forwardMatcher;
            matcher newMatcher = (input, startIndex) => innerMatcher(input, startIndex).Take(1).Select(setZero);
            return Constructor(newMatcher, newMatcher);
        }

        /// <summary>Turns the current regular expression into a zero-width negative (look-ahead or look-behind) assertion.</summary>
        /// <param name="behind"><c>true</c> to create a look-behind assertion; <c>false</c> to create a look-ahead assertion.</param>
        /// <param name="defaultMatch">The match sequence to return in case of a match.</param>
        protected TGenerex lookNegative(bool behind, IEnumerable<TMatch> defaultMatch)
        {
            // In a look-*behind* assertion, both matchers use the _backwardMatcher. Similarly, look-*ahead* assertions always use _forwardMatcher.
            matcher innerMatcher = behind ? _backwardMatcher : _forwardMatcher;
            matcher newMatcher = (input, startIndex) => innerMatcher(input, startIndex).Any() ? Enumerable.Empty<TMatch>() : defaultMatch;
            return Constructor(newMatcher, newMatcher);
        }

        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static TGenerex Recursive(Func<TGenerex, TGenerex> generator)
        {
            if (generator == null)
                throw new ArgumentNullException("generator");

            matcher recursiveForward = null, recursiveBackward = null;

            // Note the following *must* be lambdas so that they capture the above *variables* (which are modified afterwards), not their current value (which would be null)
            var carrier = Constructor(
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

        /// <summary>
        /// Returns a regular expression that matches this regular expression, then uses a specified selector to
        /// create a new regular expression from the match, and then matches the new regular expression.
        /// </summary>
        /// <typeparam name="TOtherGenerex">Type of the regular expression generated by the <paramref name="selector"/>.</typeparam>
        /// <typeparam name="TOtherMatch">Type of internal match information used by <typeparamref name="TOtherGenerex"/> (i.e. <c>int</c> or <c>LengthAndResult&lt;T&gt;</c>).</typeparam>
        /// <typeparam name="TOtherGenerexMatch">Type of match object returned by matches of <typeparamref name="TOtherGenerex"/>.</typeparam>
        /// <typeparam name="TMatchObject">Type of the parameter accepted by the <paramref name="selector"/>.</typeparam>
        /// <param name="selector">A delegate that creates a new regular expression from an object that represents a match of the current regular expression.</param>
        /// <param name="matchCreator">A delegate that, given the input sequence, start index and the internal match info, constructs the match object accepted by <paramref name="selector"/>.</param>
        /// <returns>The combined regular expression.</returns>
        /// <remarks>
        /// Regular expressions created by this method cannot match backwards. The full set of affected methods is listed at
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then(Func{TGenerexMatch, Generex{T}})"/>.
        /// </remarks>
        protected TOtherGenerex then<TOtherGenerex, TOtherMatch, TOtherGenerexMatch, TMatchObject>(Func<TMatchObject, TOtherGenerex> selector, Func<T[], int, TMatch, TMatchObject> matchCreator)
            where TOtherGenerex : GenerexBase<T, TOtherMatch, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T>
        {
            return GenerexBase<T, TOtherMatch, TOtherGenerex, TOtherGenerexMatch>.Constructor(

                // forward matcher: instantiate the new Generex at every match
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m =>
                {
                    var length = getLength(m);
                    var otherGenerex = selector(matchCreator(input, startIndex, m));
                    return otherGenerex._forwardMatcher(input, startIndex + length).Select(m2 => otherGenerex.add(m2, length));
                }),

                // backward matcher: impossible
                (input, startIndex) =>
                {
                    throw new InvalidOperationException("A Generex that generates Generexes from matches of an earlier Generex cannot match backwards (i.e., cannot be used in MatchReverse, IsMatchReverse, MatchesReverse, ReplaceReverse, AndReverse, or zero-width look-behind assertions).");
                }

            );
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, then uses a specified <paramref name="selector"/> to
        /// create a new regular expression from the match, and then matches the new regular expression.
        /// </summary>
        /// <typeparam name="TOtherGenerex">Type of the regular expression generated by the <paramref name="selector"/>.</typeparam>
        /// <typeparam name="TOtherMatch">Type of internal match information used by <typeparamref name="TOtherGenerex"/> (i.e. <c>int</c> or <c>LengthAndResult&lt;T&gt;</c>).</typeparam>
        /// <typeparam name="TOtherGenerexMatch">Type of match object returned by matches of <typeparamref name="TOtherGenerex"/>.</typeparam>
        /// <param name="selector">A delegate that creates a new regular expression from a match of the current regular expression.</param>
        /// <returns>The combined regular expression.</returns>
        /// <remarks>
        /// <para>This method is a helper method used by other overloads of <c>Then</c>. Prefer one of the following overloads:</para>
        /// <list type="bullet">
        ///     <item><description><see cref="Then(Func{TGenerexMatch, Generex{T}})"/></description></item>
        ///     <item><description><see cref="Then{TResult}(Func{TGenerexMatch, Generex{T, TResult}})"/></description></item>
        ///     <item><description><see cref="Generex.Then{TMatch, TGenerex, TGenerexMatch}(GenerexBase{char, TMatch, TGenerex, TGenerexMatch}, Func{TGenerexMatch, Stringerex})"/></description></item>
        ///     <item><description><see cref="Generex.Then{TMatch, TGenerex, TGenerexMatch, TResult}(GenerexBase{char, TMatch, TGenerex, TGenerexMatch}, Func{TGenerexMatch, Stringerex{TResult}})"/></description></item>
        /// </list>
        /// <para>Regular expressions created by this method cannot match backwards. The full set of affected methods is listed at
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then(Func{TGenerexMatch, Generex{T}})"/>.</para>
        /// </remarks>
        public TOtherGenerex Then<TOtherGenerex, TOtherMatch, TOtherGenerexMatch>(Func<TGenerexMatch, TOtherGenerex> selector)
            where TOtherGenerex : GenerexBase<T, TOtherMatch, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T>
        {
            return then<TOtherGenerex, TOtherMatch, TOtherGenerexMatch, TGenerexMatch>(selector, createMatch);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, then uses a specified <paramref name="selector"/> to
        /// create a new regular expression from the match, and then matches the new regular expression.
        /// </summary>
        /// <param name="selector">A delegate that creates a new regular expression from a match of the current regular expression.</param>
        /// <returns>The combined regular expression.</returns>
        /// <remarks>
        /// <para>Regular expressions created by this method (and several other overloads listed below) cannot match backwards. Thus, they cannot be used in calls to any of the following methods:</para>
        /// <list type="bullet">
        ///     <item><description>
        ///         <para><c>And</c>:</para>
        ///         <list type="bullet">
        ///             <item><description><see cref="GenerexNoResultBase{T, TGenerex, TGenerexMatch}.AndReverse{TOtherResult, TOtherGenerex, TOtherGenerexMatch}(GenerexWithResultBase{T, TOtherResult, TOtherGenerex, TOtherGenerexMatch})"/></description></item>
        ///             <item><description><see cref="Generex{T, TResult}.AndReverse{TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult}(GenerexWithResultBase{T, TOtherResult, TOtherGenerex, TOtherGenerexMatch}, Func{TResult, TOtherGenerexMatch, TCombinedResult})"/></description></item>
        ///             <item><description><see cref="Stringerex{TResult}.AndReverse{TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult}(GenerexWithResultBase{char, TOtherResult, TOtherGenerex, TOtherGenerexMatch}, Func{TResult, TOtherGenerexMatch, TCombinedResult})"/></description></item>
        ///             <item><description><see cref="Generex{T, TResult}.AndReverseRaw{TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult}(GenerexWithResultBase{T, TOtherResult, TOtherGenerex, TOtherGenerexMatch}, Func{TResult, TOtherResult, TCombinedResult})"/></description></item>
        ///             <item><description><see cref="Stringerex{TResult}.AndReverseRaw{TOtherResult, TOtherGenerex, TOtherGenerexMatch, TCombinedResult}(GenerexWithResultBase{char, TOtherResult, TOtherGenerex, TOtherGenerexMatch}, Func{TResult, TOtherResult, TCombinedResult})"/></description></item>
        ///         </list>
        ///     </description></item>
        ///     <item><description>
        ///         <para><c>LookBehind</c>:</para>
        ///         <list type="bullet">
        ///             <item><description><see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.LookBehind"/></description></item>
        ///             <item><description><see cref="GenerexNoResultBase{T, TGenerex, TGenerexMatch}.LookBehindNegative()"/></description></item>
        ///             <item><description><see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.LookBehindNegative(TResult)"/></description></item>
        ///         </list>
        ///     </description></item>
        ///     <item><description>
        ///         <para><c>IsMatch</c>:</para>
        ///         <list type="bullet">
        ///             <item><description><see cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.IsMatchReverse(T[], int?)"/></description></item>
        ///             <item><description><see cref="Generex.IsMatchReverse{T,TMatch,TGenerex,TGenerexMatch}(T[], GenerexBase{T,TMatch,TGenerex,TGenerexMatch}, int?)"/></description></item>
        ///             <item><description><see cref="Stringerex.IsMatchReverse(string, int?)"/></description></item>
        ///             <item><description><see cref="Stringerex{TResult}.IsMatchReverse(string, int?)"/></description></item>
        ///             <item><description><see cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.IsMatchUpTo(T[], int?)"/></description></item>
        ///             <item><description><see cref="Generex.IsMatchUpTo{T,TMatch,TGenerex,TGenerexMatch}(T[], GenerexBase{T,TMatch,TGenerex,TGenerexMatch}, int?)"/></description></item>
        ///             <item><description><see cref="Stringerex.IsMatchUpTo(string, int?)"/></description></item>
        ///             <item><description><see cref="Stringerex{TResult}.IsMatchUpTo(string, int?)"/></description></item>
        ///         </list>
        ///     </description></item>
        ///     <item><description>
        ///         <para><c>Match</c>:</para>
        ///         <list type="bullet">
        ///             <item><description><see cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.MatchReverse(T[], int?)"/></description></item>
        ///             <item><description><see cref="Generex.MatchReverse{T,TMatch,TGenerex,TGenerexMatch}(T[], GenerexBase{T,TMatch,TGenerex,TGenerexMatch}, int?)"/></description></item>
        ///             <item><description><see cref="Stringerex.MatchReverse(string, int?)"/></description></item>
        ///             <item><description><see cref="Stringerex{TResult}.MatchReverse(string, int?)"/></description></item>
        ///             <item><description><see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.RawMatchReverse(T[], int?)"/></description></item>
        ///             <item><description><see cref="Stringerex{TResult}.RawMatchReverse(string, int?)"/></description></item>
        ///         </list>
        ///     </description></item>
        ///     <item><description>
        ///         <para><c>Matches</c>:</para>
        ///         <list type="bullet">
        ///             <item><description><see cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.MatchesReverse(T[], int?)"/></description></item>
        ///             <item><description><see cref="Generex.MatchesReverse{T,TMatch,TGenerex,TGenerexMatch}(T[], GenerexBase{T,TMatch,TGenerex,TGenerexMatch}, int?)"/></description></item>
        ///             <item><description><see cref="Stringerex.MatchesReverse(string, int?)"/></description></item>
        ///             <item><description><see cref="Stringerex{TResult}.MatchesReverse(string, int?)"/></description></item>
        ///             <item><description><see cref="GenerexWithResultBase{T,TResult,TGenerex,TGenerexMatch}.RawMatchesReverse(T[], int?)"/></description></item>
        ///             <item><description><see cref="Stringerex{TResult}.RawMatchesReverse(string, int?)"/></description></item>
        ///         </list>
        ///     </description></item>
        ///     <item><description>
        ///         <para><c>Replace</c>:</para>
        ///         <list type="bullet">
        ///             <item><description><see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.ReplaceReverse(T[], Func{TGenerexMatch,T[]}, int?, int?)"/></description></item>
        ///             <item><description><see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.ReplaceReverse(T[], IEnumerable{T}, int?, int?)"/></description></item>
        ///             <item><description><see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.ReplaceReverse(T[], Func{TResult, IEnumerable{T}}, int?, int?)"/></description></item>
        ///         </list>
        ///     </description></item>
        /// </list>
        /// <para>The overloads affected by this restriction are:</para>
        /// <list type="bullet">
        ///     <item><description><see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then(Func{TGenerexMatch, Generex{T}})"/> (this method)</description></item>
        ///     <item><description><see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then{TResult}(Func{TGenerexMatch, Generex{T, TResult}})"/></description></item>
        ///     <item><description><see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then{TOtherGenerex, TOtherMatch, TOtherGenerexMatch}(Func{TGenerexMatch, TOtherGenerex})"/></description></item>
        ///     <item><description><see cref="Generex.Then{TMatch, TGenerex, TGenerexMatch}(GenerexBase{char, TMatch, TGenerex, TGenerexMatch}, Func{TGenerexMatch, Stringerex})"/></description></item>
        ///     <item><description><see cref="Generex.Then{TMatch, TGenerex, TGenerexMatch, TResult}(GenerexBase{char, TMatch, TGenerex, TGenerexMatch}, Func{TGenerexMatch, Stringerex{TResult}})"/></description></item>
        ///     <item><description><see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.ThenRaw(Func{TResult, Generex{T}})"/></description></item>
        ///     <item><description><see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.ThenRaw{TOtherResult}(Func{TResult, Generex{T, TOtherResult}})"/></description></item>
        ///     <item><description><see cref="Generex.ThenRaw{TResult, TGenerex, TGenerexMatch}(GenerexWithResultBase{char, TResult, TGenerex, TGenerexMatch}, Func{TGenerexMatch, Stringerex})"/></description></item>
        ///     <item><description><see cref="Generex.ThenRaw{TResult, TGenerex, TGenerexMatch, TOtherResult}(GenerexWithResultBase{char, TResult, TGenerex, TGenerexMatch}, Func{TGenerexMatch, Stringerex{TOtherResult}})"/></description></item>
        /// </list>
        /// </remarks>
        public Generex<T> Then(Func<TGenerexMatch, Generex<T>> selector)
        {
            return Then<Generex<T>, int, GenerexMatch<T>>(selector);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, then uses a specified <paramref name="selector"/> to
        /// create a new regular expression from the match, and then matches the new regular expression.
        /// </summary>
        /// <typeparam name="TResult">The type of result object associated with each match of the new regular expression returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">A delegate that creates a new regular expression from a match of the current regular expression.</param>
        /// <returns>The combined regular expression.</returns>
        /// <remarks>
        /// Regular expressions created by this method cannot match backwards. The full set of affected methods is listed at
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then(Func{TGenerexMatch, Generex{T}})"/>.
        /// </remarks>
        public Generex<T, TResult> Then<TResult>(Func<TGenerexMatch, Generex<T, TResult>> selector)
        {
            return Then<Generex<T, TResult>, LengthAndResult<TResult>, GenerexMatch<T, TResult>>(selector);
        }

        /// <summary>
        /// Returns a regular expression that only matches if the subarray matched by this regular expression also contains a match for the specified other regular expression.
        /// </summary>
        /// <typeparam name="TOtherGenerex">The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">The type of the match object for the other regular expression. (This is either <see cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <param name="other">A regular expression which must match the subarray matched by this regular expression.</param>
        /// <remarks>
        /// <para>It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. Consider the following input string:</para>
        /// <code>foo {bar [baz]} quux</code>
        /// <para>and the following regular expressions:</para>
        /// <code>
        ///     var curly = Stringerex.New('{').Then(Stringerex.Anything).Then('}');
        ///     var square = Stringerex.New('[').Then(Stringerex.Anything).Then(']');
        /// </code>
        /// <para>Now consider:</para>
        /// <list type="bullet">
        ///     <item><description><c>curly.And(square)</c> means: match the curly brackets first (yielding the substring <c>{bar [baz]}</c>) and then match the square brackets inside of that.
        ///     The result is a successful match, because the substring <c>{bar [baz]}</c> does contain <c>[baz]</c>.</description></item>
        ///     <item><description><c>square.And(curly)</c> means: match the square brackets first (yielding the substring <c>[baz]</c>) and then match the square brackets inside of that.
        ///     The result is no match, because there are no curly brackets in <c>[baz]</c>.</description></item>
        /// </list>
        /// </remarks>
        public TGenerex And<TOtherGenerex, TOtherGenerexMatch>(GenerexNoResultBase<T, TOtherGenerex, TOtherGenerexMatch> other)
            where TOtherGenerex : GenerexNoResultBase<T, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T>
        {
            return Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => other.IsMatch(input.Subarray(startIndex, getLength(m)))),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => other.IsMatch(input.Subarray(startIndex + getLength(m), -getLength(m))))
            );
        }

        /// <summary>
        /// Returns a regular expression that only matches if the subarray matched by this regular expression also fully matches the specified other regular expression.
        /// </summary>
        /// <typeparam name="TOtherGenerex">The type of the other regular expression. (This is either <see cref="Generex{T,TResult}"/> or <see cref="Stringerex{TResult}"/>.)</typeparam>
        /// <typeparam name="TOtherGenerexMatch">The type of the match object for the other regular expression. (This is either <see cref="GenerexMatch{T,TResult}"/> or <see cref="StringerexMatch{TResult}"/>.)</typeparam>
        /// <param name="other">A regular expression which must match the subarray matched by this regular expression.</param>
        /// <remarks>It is important to note that <c>a.And(b)</c> is not the same as <c>b.And(a)</c>. See <see cref="GenerexBase{T,TMatch,TGenerex,TGenerexMatch}.And"/> for an example.</remarks>
        public TGenerex AndExact<TOtherGenerex, TOtherGenerexMatch>(GenerexNoResultBase<T, TOtherGenerex, TOtherGenerexMatch> other)
            where TOtherGenerex : GenerexNoResultBase<T, TOtherGenerex, TOtherGenerexMatch>
            where TOtherGenerexMatch : GenerexMatch<T>
        {
            return Constructor(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => other.IsMatchExact(input.Subarray(startIndex, getLength(m)))),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => other.IsMatchExact(input.Subarray(startIndex + getLength(m), -getLength(m))))
            );
        }

        /// <summary>
        /// Throws an exception generated by the specified code when the regular expression engine encounters this expression.
        /// </summary>
        public TGenerex Throw(Func<TGenerexMatch, Exception> exceptionGenerator)
        {
            return Do(m => { throw exceptionGenerator(m); });
        }
    }
}
