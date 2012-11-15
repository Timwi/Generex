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
    /// <typeparam name="TResult">Type of objects generated from each match of the regular expression.</typeparam>
    public sealed class Generex<T, TResult> : GenerexBase<T, GenerexMatchInfo<TResult>, Generex<T, TResult>, GenerexMatch<T, TResult>>
    {
        internal Generex(matcher forward, matcher backward) : base(forward, backward) { }
        internal override int getLength(GenerexMatchInfo<TResult> match) { return match.Length; }
        internal override GenerexMatchInfo<TResult> add(GenerexMatchInfo<TResult> match, int extra) { return match.Add(extra); }
        internal override GenerexMatchInfo<TResult> setZero(GenerexMatchInfo<TResult> match) { return new GenerexMatchInfo<TResult>(match.Result, 0); }
        internal override Generex<T, TResult> create(matcher forward, matcher backward) { return new Generex<T, TResult>(forward, backward); }
        internal override GenerexMatch<T, TResult> createMatch(T[] input, int index, GenerexMatchInfo<TResult> match) { return new GenerexMatch<T, TResult>(match.Result, input, index, match.Length); }
        internal override GenerexMatch<T, TResult> createBackwardsMatch(T[] input, int index, GenerexMatchInfo<TResult> match) { return new GenerexMatch<T, TResult>(match.Result, input, index + match.Length, -match.Length); }

        /// <summary>
        /// Instantiates an empty regular expression which always matches and returns the specified result object.
        /// </summary>
        public Generex(TResult result) : this(new[] { new GenerexMatchInfo<TResult>(result, 0) }) { }

        private Generex(GenerexMatchInfo<TResult>[] emptyMatch) : this((input, startIndex) => emptyMatch) { }
        private Generex(matcher bothMatcher) : base(bothMatcher, bothMatcher) { }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns the result of the first match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>The result of the first match in case of success; default(TResult) if no match.</returns>
        public TResult RawMatch(T[] input, int startAt = 0)
        {
            return RawMatches(input, startAt).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns the result of the first match
        /// found by matching the regular expression backwards (starting from the end of the input sequence).
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>The result of the match in case of success; default(TResult) if no match.</returns>
        public TResult RawMatchReverse(T[] input, int? endAt = null)
        {
            return RawMatchesReverse(input, endAt).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression exactly, and if so, returns the match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>The result of the match in case of success; default(TResult) if no match.</returns>
        public TResult RawMatchExact(T[] input, int mustStartAt = 0, int? mustEndAt = null)
        {
            return matchExact(input, mustStartAt, mustEndAt ?? input.Length, m => m.Result);
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>The behaviour is analogous to <see cref="Regex.Matches(string,string)"/>.
        /// The documentation for that method claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public IEnumerable<TResult> RawMatches(T[] input, int startAt = 0)
        {
            return matches(input, startAt, (index, resultInfo) => resultInfo.Result, backward: false);
        }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the specified
        /// input sequence), optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        public IEnumerable<TResult> RawMatchesReverse(T[] input, int? endAt = null)
        {
            return matches(input, endAt ?? input.Length, (index, resultInfo) => resultInfo.Result, backward: true);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified ones,
        /// and generates a match object that combines the original two matches.
        /// </summary>
        public Generex<T, TCombined> ThenRaw<TOtherResult, TCombined>(Generex<T, TOtherResult> other, Func<TResult, TOtherResult, TCombined> selector)
        {
            return new Generex<T, TCombined>(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m => other._forwardMatcher(input, startIndex + m.Length)
                    .Select(m2 => new GenerexMatchInfo<TCombined>(selector(m.Result, m2.Result), m.Length + m2.Length))),
                (input, startIndex) => other._backwardMatcher(input, startIndex).SelectMany(m2 => _backwardMatcher(input, startIndex + m2.Length)
                    .Select(m => new GenerexMatchInfo<TCombined>(selector(m.Result, m2.Result), m.Length + m2.Length)))
            );
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified one,
        /// and generates a match object that combines the result of this regular expression with the match of the other.
        /// </summary>
        public Generex<T, TCombined> Then<TOtherResult, TCombined>(Generex<T, TOtherResult> other, Func<TResult, GenerexMatch<T, TOtherResult>, TCombined> selector)
        {
            return new Generex<T, TCombined>(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m => other._forwardMatcher(input, startIndex + m.Length)
                    .Select(m2 => new GenerexMatchInfo<TCombined>(selector(m.Result, new GenerexMatch<T, TOtherResult>(m2.Result, input, startIndex + m.Length, m2.Length)), m.Length + m2.Length))),
                (input, startIndex) => other._backwardMatcher(input, startIndex).Select(m2 => new { Match = new GenerexMatch<T, TOtherResult>(m2.Result, input, startIndex + m2.Length, -m2.Length), Length = m2.Length })
                    .SelectMany(inf => _backwardMatcher(input, startIndex + inf.Length).Select(m => new GenerexMatchInfo<TCombined>(selector(m.Result, inf.Match), m.Length + inf.Length)))
            );
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified ones,
        /// and generates a match object that combines the result of this regular expression with the match of the other.
        /// </summary>
        public Generex<T, TCombined> Then<TCombined>(Generex<T> other, Func<TResult, GenerexMatch<T>, TCombined> selector)
        {
            return new Generex<T, TCombined>(
                (input, startIndex) => _forwardMatcher(input, startIndex).SelectMany(m => other._forwardMatcher(input, startIndex + m.Length)
                    .Select(m2 => new GenerexMatchInfo<TCombined>(selector(m.Result, new GenerexMatch<T>(input, startIndex + m.Length, m2)), m.Length + m2))),
                (input, startIndex) => other._backwardMatcher(input, startIndex).Select(m2 => new { Match = new GenerexMatch<T>(input, startIndex + m2, -m2), Length = m2 })
                    .SelectMany(inf => _backwardMatcher(input, startIndex + inf.Length).Select(m => new GenerexMatchInfo<TCombined>(selector(m.Result, inf.Match), m.Length + inf.Length)))
            );
        }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or a single element that satisfies the specified predicate (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, TResult> Or(Predicate<T> predicate, Func<GenerexMatch<T>, TResult> selector)
        {
            return Or(new Generex<T>(predicate).Process(selector));
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf. "?" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> OptionalGreedy() { return repeatBetween(0, 1, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Zero times is prioritised (cf. "??" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> Optional() { return repeatBetween(0, 1, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. More times are prioritised (cf. "*" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> RepeatGreedy() { return new Generex<T, IEnumerable<TResult>>(repeatInfinite(_forwardMatcher, true), repeatInfinite(_backwardMatcher, true)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. Fewer times are prioritised (cf. "*?" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> Repeat() { return new Generex<T, IEnumerable<TResult>>(repeatInfinite(_forwardMatcher, false), repeatInfinite(_backwardMatcher, false)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. More times are prioritised (cf. "{min,}" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> RepeatGreedy(int min) { return repeatMin(min, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. Fewer times are prioritised (cf. "{min,}?" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> Repeat(int min) { return repeatMin(min, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. More times are prioritised (cf. "{min,max}" in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public Generex<T, IEnumerable<TResult>> RepeatGreedy(int min, int max) { return repeatBetween(min, max, true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. Fewer times are prioritised (cf. "{min,max}?" in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public Generex<T, IEnumerable<TResult>> Repeat(int min, int max) { return repeatBetween(min, max, false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times (cf. "{times}" in traditional regular expression syntax).
        /// </summary>
        public Generex<T, IEnumerable<TResult>> Times(int times)
        {
            if (times < 0) throw new ArgumentException("'times' cannot be negative.", "times");
            return repeatBetween(times, times, true);
        }
        /// <summary>
        /// Returns a regular expression that matches this regular expression one or more times, interspersed with a separator. Fewer times are prioritised.
        /// </summary>
        public Generex<T, IEnumerable<TResult>> RepeatWithSeparator(Generex<T> separator) { return ThenRaw(separator.Then(this).Repeat(), (first, rest) => first.Concat(rest)); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression one or more times, interspersed with a separator. More times are prioritised.
        /// </summary>
        public Generex<T, IEnumerable<TResult>> RepeatWithSeparatorGreedy(Generex<T> separator) { return ThenRaw(separator.Then(this).RepeatGreedy(), (first, rest) => first.Concat(rest)); }

        /// <summary>
        /// Generates a matcher that matches this regular expression zero or more times.
        /// </summary>
        /// <param name="inner">Inner matcher.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private static Generex<T, IEnumerable<TResult>>.matcher repeatInfinite(matcher inner, bool greedy)
        {
            Generex<T, IEnumerable<TResult>>.matcher newMatcher = null;
            if (greedy)
                newMatcher = (input, startIndex) => inner(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m.Length)
                    .Select(m2 => new GenerexMatchInfo<IEnumerable<TResult>>(m.Result.Concat(m2.Result), m.Length + m2.Length)))
                    .Concat(new GenerexMatchInfo<IEnumerable<TResult>>(Enumerable.Empty<TResult>(), 0));
            else
                newMatcher = (input, startIndex) => new GenerexMatchInfo<IEnumerable<TResult>>(Enumerable.Empty<TResult>(), 0)
                    .Concat(inner(input, startIndex).SelectMany(m => newMatcher(input, startIndex + m.Length)
                    .Select(m2 => new GenerexMatchInfo<IEnumerable<TResult>>(m.Result.Concat(m2.Result), m.Length + m2.Length))));
            return newMatcher;
        }

        /// <summary>
        /// Generates a regular expression that matches this regular expression zero or more times.
        /// </summary>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private Generex<T, IEnumerable<TResult>> repeatInfinite(bool greedy)
        {
            return new Generex<T, IEnumerable<TResult>>(repeatInfinite(_forwardMatcher, greedy), repeatInfinite(_backwardMatcher, greedy));
        }

        /// <summary>
        /// Generates a matcher that matches this regular expression at least a minimum number of times.
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private Generex<T, IEnumerable<TResult>> repeatMin(int min, bool greedy)
        {
            if (min < 0) throw new ArgumentException("'min' cannot be negative.", "min");
            return repeatBetween(min, min, true).ThenRaw(repeatInfinite(greedy), Enumerable.Concat);
        }

        /// <summary>
        /// Generates a matcher that matches this regular expression at least a minimum number of times and at most a maximum number of times.
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        /// <param name="greedy">If true, more matches are prioritised; otherwise, fewer matches are prioritised.</param>
        private Generex<T, IEnumerable<TResult>> repeatBetween(int min, int max, bool greedy)
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
            return new Generex<T, IEnumerable<TResult>>(rm.ForwardMatcher, rm.BackwardMatcher);
        }

        private sealed class repeatMatcher
        {
            public int MinTimes;
            public int MaxTimes;
            public bool Greedy;
            public matcher InnerForwardMatcher;
            public matcher InnerBackwardMatcher;
            public IEnumerable<GenerexMatchInfo<IEnumerable<TResult>>> ForwardMatcher(T[] input, int startIndex) { return matcher(input, startIndex, 0, backward: false); }
            public IEnumerable<GenerexMatchInfo<IEnumerable<TResult>>> BackwardMatcher(T[] input, int startIndex) { return matcher(input, startIndex, 0, backward: true); }
            private IEnumerable<GenerexMatchInfo<IEnumerable<TResult>>> matcher(T[] input, int startIndex, int iteration, bool backward)
            {
                if (!Greedy && iteration >= MinTimes)
                    yield return new GenerexMatchInfo<IEnumerable<TResult>>(Enumerable.Empty<TResult>(), 0);
                if (iteration < MaxTimes)
                {
                    foreach (var m in (backward ? InnerBackwardMatcher : InnerForwardMatcher)(input, startIndex))
                        foreach (var m2 in matcher(input, startIndex + m.Length, iteration + 1, backward))
                            yield return new GenerexMatchInfo<IEnumerable<TResult>>(
                                backward ? m2.Result.Concat(m.Result) : m.Result.Concat(m2.Result),
                                m.Length + m2.Length
                            );
                }
                if (Greedy && iteration >= MinTimes)
                    yield return new GenerexMatchInfo<IEnumerable<TResult>>(Enumerable.Empty<TResult>(), 0);
            }
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. (This always matches successfully and all matches are zero-length.)
        /// </summary>
        public Generex<T, TResult> DoRaw(Action<TResult> code)
        {
            return new Generex<T, TResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => { code(m.Result); return m; }),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => { code(m.Result); return m; })
            );
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. The return value of the specified code determines whether the expression matches successfully (all matches are zero-length).
        /// </summary>
        public Generex<T, TResult> DoRaw(Func<bool> code)
        {
            return new Generex<T, TResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => code()),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => code())
            );
        }

        /// <summary>
        /// Executes the specified code every time the regular expression engine encounters this expression. The return value of the specified code determines whether the expression matches successfully (all matches are zero-length).
        /// </summary>
        public Generex<T, TResult> DoRaw(Func<TResult, bool> code)
        {
            return new Generex<T, TResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Where(m => code(m.Result)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Where(m => code(m.Result))
            );
        }

        /// <summary>Turns the current regular expression into a zero-width negative look-ahead assertion, which returns the specified default result in case of a match.</summary>
        public Generex<T, TResult> LookAheadNegative(TResult defaultMatch) { return lookNegative(behind: false, defaultMatch: new[] { new GenerexMatchInfo<TResult>(defaultMatch, 0) }); }
        /// <summary>Turns the current regular expression into a zero-width negative look-behind assertion, which returns the specified default result in case of a match.</summary>
        public Generex<T, TResult> LookBehindNegative(TResult defaultMatch) { return lookNegative(behind: true, defaultMatch: new[] { new GenerexMatchInfo<TResult>(defaultMatch, 0) }); }

        /// <summary>Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TOtherResult">Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">Function to process a regular expression match.</param>
        public Generex<T, TOtherResult> ProcessRaw<TOtherResult>(Func<TResult, TOtherResult> selector)
        {
            return new Generex<T, TOtherResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => new GenerexMatchInfo<TOtherResult>(selector(m.Result), m.Length)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => new GenerexMatchInfo<TOtherResult>(selector(m.Result), m.Length))
            );
        }

        /// <summary>Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TOtherResult">Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">Function to process a regular expression match.</param>
        public Generex<T, TOtherResult> Process<TOtherResult>(Func<GenerexMatch<T, TResult>, TOtherResult> selector)
        {
            return new Generex<T, TOtherResult>(
                (input, startIndex) => _forwardMatcher(input, startIndex).Select(m => new GenerexMatchInfo<TOtherResult>(selector(new GenerexMatch<T, TResult>(m.Result, input, startIndex, m.Length)), m.Length)),
                (input, startIndex) => _backwardMatcher(input, startIndex).Select(m => new GenerexMatchInfo<TOtherResult>(selector(new GenerexMatch<T, TResult>(m.Result, input, startIndex + m.Length, -m.Length)), m.Length))
            );
        }

        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Generex<T, TResult> Recursive(Func<Generex<T, TResult>, Generex<T, TResult>> generator)
        {
            if (generator == null)
                throw new ArgumentNullException("generator");

            matcher recursiveForward = null, recursiveBackward = null;

            // Note the following *must* be lambdas so that they capture the above *variables* (which are modified afterwards), not their current value (which would be null)
            var carrier = new Generex<T, TResult>(
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
