using System;
using System.Collections.Generic;
using System.Linq;

namespace RT.Generexes
{
    /// <summary>
    /// Provides regular-expression functionality for strings.
    /// </summary>
    /// <typeparam name="TResult">Type of objects generated from each match of the regular expression.</typeparam>
    /// <remarks>This type is not directly instantiated; use <see cref="Stringerex.Process"/>.</remarks>
    public sealed class Stringerex<TResult> : GenerexWithResultBase<char, TResult, Stringerex<TResult>, StringerexMatch<TResult>>
    {
        internal sealed override StringerexMatch<TResult> createMatchWithResult(TResult result, char[] input, int index, int length)
        {
            return new StringerexMatch<TResult>(result, input, index, length);
        }

        /// <summary>
        /// Instantiates an empty regular expression which always matches and returns the specified result object.
        /// </summary>
        public Stringerex(TResult result) : base(result) { }

        internal Stringerex(matcher forward, matcher backward) : base(forward, backward) { }
        static Stringerex() { Constructor = (forward, backward) => new Stringerex<TResult>(forward, backward); }

        /// <summary>
        /// Determines whether the given string contains a match for this regular expression, optionally starting the search at a specified character index.
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="startAt">Optional character index at which to start the search. Matches that start before this index are not included.</param>
        public bool IsMatch(string input, int startAt = 0) { return base.IsMatch(input.ToCharArray(), startAt); }

        /// <summary>
        /// Determines whether the given string matches this regular expression at a specific character index.
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <returns>True if a match starting at the specified index exists (which need not run all the way to the end of the string); otherwise, false.</returns>
        public bool IsMatchAt(string input, int mustStartAt = 0) { return base.IsMatchAt(input.ToCharArray(), mustStartAt); }

        /// <summary>
        /// Determines whether the given string matches this regular expression up to a specific character index.
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the string).</param>
        /// <returns>True if a match ending at the specified index exists (which need not begin at the start of the string); otherwise, false.</returns>
        public bool IsMatchUpTo(string input, int? mustEndAt = null) { return base.IsMatchUpTo(input.ToCharArray(), mustEndAt); }

        /// <summary>
        /// Determines whether the given string matches this regular expression exactly.
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the string).</param>
        /// <returns>True if a match starting and ending at the specified indexes exists; otherwise, false.</returns>
        public bool IsMatchExact(string input, int mustStartAt = 0, int? mustEndAt = null) { return base.IsMatchExact(input.ToCharArray(), mustStartAt, mustEndAt); }

        /// <summary>
        /// Determines whether the given string contains a match for this regular expression that ends before the specified maximum character index.
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="endAt">Optional index before which a match must end. The search begins by matching from this index backwards, and then proceeds towards the start of the string.</param>
        public bool IsMatchReverse(string input, int? endAt = null) { return base.IsMatchReverse(input.ToCharArray(), endAt); }

        /// <summary>
        /// Determines whether the given string matches this regular expression, and if so, returns information about the first match.
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>An object describing a regular expression match in case of success; null if no match.</returns>
        public StringerexMatch<TResult> Match(string input, int startAt = 0) { return base.Match(input.ToCharArray(), startAt); }

        /// <summary>
        /// Determines whether the given string matches this regular expression exactly, and if so, returns information about the match.
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the string).</param>
        /// <returns>An object describing the regular expression match in case of success; null if no match.</returns>
        public StringerexMatch<TResult> MatchExact(string input, int mustStartAt = 0, int? mustEndAt = null) { return base.MatchExact(input.ToCharArray(), mustStartAt, mustEndAt); }

        /// <summary>
        /// Determines whether the given string matches this regular expression, and if so, returns information about the first match
        /// found by matching the regular expression backwards (starting from the end of the string).
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>An object describing a regular expression match in case of success; null if no match.</returns>
        public StringerexMatch<TResult> MatchReverse(string input, int? endAt = null) { return base.MatchReverse(input.ToCharArray(), endAt); }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the specified
        /// string), optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        public IEnumerable<StringerexMatch<TResult>> MatchesReverse(string input, int? endAt = null) { return base.MatchesReverse(input.ToCharArray(), endAt); }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the specified character index.
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>The behaviour is analogous to <see cref="System.Text.RegularExpressions.Regex.Matches(string,string)"/>.
        /// The documentation for that method claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public IEnumerable<StringerexMatch<TResult>> Matches(string input, int startAt = 0) { return base.Matches(input.ToCharArray(), startAt); }

        /// <summary>Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TOtherResult">Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">Function to process a regular expression match.</param>
        public Stringerex<TOtherResult> Process<TOtherResult>(Func<StringerexMatch<TResult>, TOtherResult> selector)
        {
            return base.Process<Stringerex<TOtherResult>, StringerexMatch<TOtherResult>, TOtherResult>(selector);
        }

        /// <summary>Processes each match of this regular expression by running each result through a provided selector.</summary>
        /// <typeparam name="TOtherResult">Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">Function to process the result of a regular expression match.</param>
        public Stringerex<TOtherResult> ProcessRaw<TOtherResult>(Func<TResult, TOtherResult> selector)
        {
            return base.ProcessRaw<Stringerex<TOtherResult>, StringerexMatch<TOtherResult>, TOtherResult>(selector);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified ones,
        /// and generates a match object that combines the result of this regular expression with the match of the other.
        /// </summary>
        public Stringerex<TCombined> Then<TCombined>(Stringerex other, Func<TResult, StringerexMatch, TCombined> selector)
        {
            return base.Then<Stringerex, StringerexMatch, Stringerex<TCombined>, StringerexMatch<TCombined>, TCombined>(other, selector);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified one,
        /// and generates a match object that combines the result of this regular expression with the match of the other.
        /// </summary>
        public Stringerex<TCombined> Then<TOther, TCombined>(Stringerex<TOther> other, Func<TResult, StringerexMatch<TOther>, TCombined> selector)
        {
            return base.Then<Stringerex<TOther>, StringerexMatch<TOther>, TOther, Stringerex<TCombined>, StringerexMatch<TCombined>, TCombined>(other, selector);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified ones,
        /// and generates a match object that combines the original two matches.
        /// </summary>
        public Stringerex<TCombined> ThenRaw<TOther, TCombined>(Stringerex<TOther> other, Func<TResult, TOther, TCombined> selector)
        {
            return base.ThenRaw<Stringerex<TOther>, StringerexMatch<TOther>, TOther, Stringerex<TCombined>, StringerexMatch<TCombined>, TCombined>(other, selector);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf. "?" in traditional regular expression syntax).
        /// </summary>
        public Stringerex<IEnumerable<TResult>> OptionalGreedy() { return repeatBetween<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(0, 1, greedy: true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Zero times is prioritised (cf. "??" in traditional regular expression syntax).
        /// </summary>
        public Stringerex<IEnumerable<TResult>> Optional() { return repeatBetween<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(0, 1, greedy: false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. More times are prioritised (cf. "*" in traditional regular expression syntax).
        /// </summary>
        public Stringerex<IEnumerable<TResult>> RepeatGreedy() { return repeatInfinite<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(greedy: true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression zero or more times. Fewer times are prioritised (cf. "*?" in traditional regular expression syntax).
        /// </summary>
        public Stringerex<IEnumerable<TResult>> Repeat() { return repeatInfinite<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(greedy: false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. More times are prioritised (cf. "{min,}" in traditional regular expression syntax).
        /// </summary>
        public Stringerex<IEnumerable<TResult>> RepeatGreedy(int min) { return repeatMin<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(min, greedy: true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times or more. Fewer times are prioritised (cf. "{min,}?" in traditional regular expression syntax).
        /// </summary>
        public Stringerex<IEnumerable<TResult>> Repeat(int min) { return repeatMin<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(min, greedy: false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. More times are prioritised (cf. "{min,max}" in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public Stringerex<IEnumerable<TResult>> RepeatGreedy(int min, int max) { return repeatBetween<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(min, max, greedy: true); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression any number of times within specified boundaries. Fewer times are prioritised (cf. "{min,max}?" in traditional regular expression syntax).
        /// </summary>
        /// <param name="min">Minimum number of times to match.</param>
        /// <param name="max">Maximum number of times to match.</param>
        public Stringerex<IEnumerable<TResult>> Repeat(int min, int max) { return repeatBetween<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(min, max, greedy: false); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression the specified number of times (cf. "{times}" in traditional regular expression syntax).
        /// </summary>
        public Stringerex<IEnumerable<TResult>> Times(int times)
        {
            if (times < 0) throw new ArgumentException("'times' cannot be negative.", "times");
            return repeatBetween<Stringerex<IEnumerable<TResult>>, StringerexMatch<IEnumerable<TResult>>>(times, times, true);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression one or more times, interspersed with a separator. Fewer times are prioritised.
        /// </summary>
        public Stringerex<IEnumerable<TResult>> RepeatWithSeparator(Stringerex separator)
        {
            return ThenRaw(separator.Then(this).Repeat(), Extensions.Concat);
        }
        /// <summary>
        /// Returns a regular expression that matches this regular expression one or more times, interspersed with a separator. More times are prioritised.
        /// </summary>
        public Stringerex<IEnumerable<TResult>> RepeatWithSeparatorGreedy(Stringerex separator)
        {
            return ThenRaw(separator.Then(this).RepeatGreedy(), Extensions.Concat);
        }
    }
}
