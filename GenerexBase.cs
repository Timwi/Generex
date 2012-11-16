using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace RT.Generexes
{
    /// <summary>Abstract base class for all Generex regular expressions.</summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    /// <typeparam name="TMatch">Either int or <see cref="LengthAndResult{TResult}"/>.</typeparam>
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
                        throw new InvalidOperationException("The static constructor of {0} didn’t initialize the Constructor field.".Fmt(typeof(TGenerex)));
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

        internal abstract int getLength(TMatch match);
        internal abstract TMatch add(TMatch match, int extra);
        internal abstract TMatch setZero(TMatch match);
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
        /// <returns>An object describing a regular expression match in case of success; null if no match.</returns>
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
        /// <returns>An object describing the regular expression match in case of success; null if no match.</returns>
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
        /// <returns>An object describing a regular expression match in case of success; null if no match.</returns>
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
        /// Returns a regular expression that matches either this regular expression or the specified other regular expression (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public TGenerex Or(TGenerex other)
        {
            return Constructor(or(_forwardMatcher, other._forwardMatcher), or(_backwardMatcher, other._backwardMatcher));
        }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static TGenerex Ors(params TGenerex[] other) { return other.Aggregate((prev, next) => prev.Or(next)); }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static TGenerex Ors(IEnumerable<TGenerex> other) { return other.Aggregate((prev, next) => prev.Or(next)); }

        /// <summary>Matches this regular expression atomically (without backtracking into it) (cf. "(?>...)" in traditional regular expression syntax).</summary>
        public TGenerex Atomic()
        {
            return Constructor(
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

        private static matcher or(matcher one, matcher two)
        {
            return new safeOrMatcher(one, two).Matcher;
        }

        /// <summary>
        /// This class implements the “or” (or alternation) operation without invoking both matchers at the start.
        /// (This is important in cases involving recursive regular expressions, see <see cref="Generex{T}.Recursive"/> and <see cref="Generex{T,TResult}.Recursive"/>.)
        /// </summary>
        private class safeOrMatcher
        {
            public matcher One { get; private set; }
            public matcher Two { get; private set; }
            public safeOrMatcher(matcher one, matcher two) { One = one; Two = two; }
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

        /// <summary>Turns the current regular expression into a zero-width positive look-ahead assertion.</summary>
        public TGenerex LookAhead() { return look(behind: false); }
        /// <summary>Turns the current regular expression into a zero-width positive look-behind assertion.</summary>
        public TGenerex LookBehind() { return look(behind: true); }

        private TGenerex look(bool behind)
        {
            // In a look-*behind* assertion, both matchers use the _backwardMatcher. Similarly, look-*ahead* assertions always use _forwardMatcher.
            matcher innerMatcher = behind ? _backwardMatcher : _forwardMatcher;
            matcher newMatcher = (input, startIndex) => innerMatcher(input, startIndex).Take(1).Select(setZero);
            return Constructor(newMatcher, newMatcher);
        }

        internal TGenerex lookNegative(bool behind, IEnumerable<TMatch> defaultMatch)
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
    }
}
