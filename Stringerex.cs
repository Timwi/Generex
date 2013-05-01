using System;
using System.Collections.Generic;
using System.Linq;

namespace RT.Generexes
{
    /// <summary>
    /// Provides regular-expression functionality for strings.
    /// </summary>
    public sealed class Stringerex : GenerexNoResultBase<char, Stringerex, StringerexMatch>
    {
        protected sealed override StringerexMatch createNoResultMatch(char[] input, int index, int matchLength)
        {
            return new StringerexMatch(input, index, matchLength);
        }

        /// <summary>
        /// Instantiates an empty regular expression (always matches).
        /// </summary>
        public Stringerex() : base(emptyMatch, emptyMatch) { }

        /// <summary>
        /// Instantiates a regular expression that matches a specified string.
        /// </summary>
        public Stringerex(string elements) : base(EqualityComparer<char>.Default, elements.ToCharArray()) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public Stringerex(IEnumerable<char> elements) : base(EqualityComparer<char>.Default, elements.ToArray()) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        public Stringerex(IEqualityComparer<char> comparer, string elements) : base(comparer, elements.ToCharArray()) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        public Stringerex(IEqualityComparer<char> comparer, IEnumerable<char> elements) : base(comparer, elements.ToArray()) { }

        /// <summary>
        /// Instantiates a regular expression that matches a single character that satisfies the given predicate (cf. <c>[...]</c> in traditional regular expression syntax).
        /// </summary>
        public Stringerex(Predicate<char> predicate) : base(predicate) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive regular expressions.
        /// </summary>
        public Stringerex(params Stringerex[] generexSequence)
            : base(
                sequenceMatcher(generexSequence, backward: false),
                sequenceMatcher(generexSequence, backward: true)) { }

        private Stringerex(matcher forward, matcher backward) : base(forward, backward) { }
        static Stringerex() { Constructor = (forward, backward) => new Stringerex(forward, backward); }

        /// <summary>Implicitly converts a character into a regular expression that matches just that character.</summary>
        public static implicit operator Stringerex(char ch) { return new Stringerex(new[] { ch }); }
        /// <summary>Implicitly converts a string into a regular expression that matches that string.</summary>
        public static implicit operator Stringerex(string str) { return new Stringerex(str); }
        /// <summary>Implicitly converts a predicate into a regular expression that matches a single character satisfying the predicate.</summary>
        public static implicit operator Stringerex(Predicate<char> predicate) { return new Stringerex(predicate); }

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
        /// <returns>An object describing a regular expression match in case of success; <c>null</c> if no match.</returns>
        public StringerexMatch Match(string input, int startAt = 0) { return base.Match(input.ToCharArray(), startAt); }

        /// <summary>
        /// Determines whether the given string matches this regular expression exactly, and if so, returns information about the match.
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the string).</param>
        /// <returns>An object describing the regular expression match in case of success; <c>null</c> if no match.</returns>
        public StringerexMatch MatchExact(string input, int mustStartAt = 0, int? mustEndAt = null) { return base.MatchExact(input.ToCharArray(), mustStartAt, mustEndAt); }

        /// <summary>
        /// Determines whether the given string matches this regular expression, and if so, returns information about the first match
        /// found by matching the regular expression backwards (starting from the end of the string).
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>An object describing a regular expression match in case of success; <c>null</c> if no match.</returns>
        public StringerexMatch MatchReverse(string input, int? endAt = null) { return base.MatchReverse(input.ToCharArray(), endAt); }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the specified
        /// string), optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        public IEnumerable<StringerexMatch> MatchesReverse(string input, int? endAt = null) { return base.MatchesReverse(input.ToCharArray(), endAt); }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the specified character index.
        /// </summary>
        /// <param name="input">String to match the regular expression against.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>The behaviour is analogous to <see cref="System.Text.RegularExpressions.Regex.Matches(string,string)"/>.
        /// The documentation for that method claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public IEnumerable<StringerexMatch> Matches(string input, int startAt = 0) { return base.Matches(input.ToCharArray(), startAt); }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified string (cf. <c>|</c> or <c>[...]</c> in traditional regular expression syntax).
        /// </summary>
        /// <seealso cref="Or(IEqualityComparer{char},string)"/>
        public Stringerex Or(string str) { return base.Or(EqualityComparer<char>.Default, str.ToCharArray()); }

        /// <summary>
        /// Returns a regular expression that matches either this regular expression or the specified string using the specified equality comparer (cf. <c>|</c> or <c>[...]</c> in traditional regular expression syntax).
        /// </summary>
        /// <seealso cref="Or(string)"/>
        public Stringerex Or(IEqualityComparer<char> comparer, string str) { return base.Or(comparer, str.ToCharArray()); }

        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified string.
        /// </summary>
        public Stringerex Then(string str) { return base.Then(EqualityComparer<char>.Default, str.ToCharArray()); }
        /// <summary>
        /// Returns a regular expression that matches this regular expression followed by the specified string, using the specified equality comparer.
        /// </summary>
        public Stringerex Then(IEqualityComparer<char> comparer, string str) { return base.Then(comparer, str.ToCharArray()); }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified other,
        /// and retains the match object generated by each match of the other regular expression.
        /// </summary>
        public Stringerex<TResult> Then<TResult>(Stringerex<TResult> other) { return then<Stringerex<TResult>, StringerexMatch<TResult>, TResult>(other); }

        /// <summary>Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TResult">Type of the object returned by <paramref name="selector"/>.</typeparam>
        /// <param name="selector">Function to process a regular expression match.</param>
        public Stringerex<TResult> Process<TResult>(Func<StringerexMatch, TResult> selector) { return process<Stringerex<TResult>, StringerexMatch<TResult>, TResult>(selector); }

        /// <summary>
        /// Instantiates a regular expression that matches the specified string.
        /// </summary>
        public static Stringerex New(string str) { return new Stringerex(str); }
        /// <summary>
        /// Instantiates a regular expression that matches the specified string using the specified equality comparer.
        /// </summary>
        public static Stringerex New(IEqualityComparer<char> comparer, string str) { return new Stringerex(comparer, str); }
        /// <summary>
        /// Instantiates a regular expression that matches a single character that satisfies the given predicate (cf. <c>[...]</c> in traditional regular expression syntax).
        /// </summary>
        public static Stringerex New(Predicate<char> predicate) { return new Stringerex(predicate); }
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive regular expressions.
        /// </summary>
        public static Stringerex New(params Stringerex[] stringerexes) { return new Stringerex(stringerexes); }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. <c>|</c> in traditional regular expression syntax).
        /// </summary>
        public static Stringerex<TResult> Ors<TResult>(IEnumerable<Stringerex<TResult>> stringerexes) { return stringerexes.Aggregate((prev, next) => prev.Or(next)); }
        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. <c>|</c> in traditional regular expression syntax).
        /// </summary>
        public static Stringerex<TResult> Ors<TResult>(params Stringerex<TResult>[] stringerexes) { return stringerexes.Aggregate((prev, next) => prev.Or(next)); }

        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Stringerex<TResult> Recursive<TResult>(Func<Stringerex<TResult>, Stringerex<TResult>> generator) { return Stringerex<TResult>.Recursive(generator); }
    }
}
