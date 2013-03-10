using System;
using System.Collections.Generic;
using System.Linq;

namespace RT.Generexes
{
    /// <summary>
    /// Provides static factory methods to generate <see cref="Generex{T}"/> and <see cref="Generex{T,TResult}"/> objects.
    /// </summary>
    public static class Generex
    {
        internal static IEnumerable<int> NoMatch = Enumerable.Empty<int>();
        internal static IEnumerable<int> ZeroWidthMatch = new int[] { 0 };
        internal static IEnumerable<int> OneElementMatch = new int[] { 1 };
        internal static IEnumerable<int> NegativeOneElementMatch = new int[] { -1 };

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public static Generex<T> New<T>(params T[] elements) { return new Generex<T>(elements); }
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        public static Generex<T> New<T>(IEqualityComparer<T> comparer, params T[] elements) { return new Generex<T>(comparer, elements); }
        /// <summary>
        /// Instantiates a regular expression that matches a single element that satisfies the given predicate (cf. "[...]" in traditional regular expression syntax).
        /// </summary>
        public static Generex<T> New<T>(Predicate<T> predicate) { return new Generex<T>(predicate); }
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive regular expressions.
        /// </summary>
        public static Generex<T> New<T>(params Generex<T>[] generexes) { return new Generex<T>(generexes); }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static Generex<T> Ors<T>(IEnumerable<Generex<T>> generexes) { return Generex<T>.Ors(generexes); }
        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static Generex<T> Ors<T>(params Generex<T>[] generexes) { return Generex<T>.Ors(generexes); }
        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static Generex<T, TResult> Ors<T, TResult>(IEnumerable<Generex<T, TResult>> generexes) { return Generex<T, TResult>.Ors(generexes); }
        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. "|" in traditional regular expression syntax).
        /// </summary>
        public static Generex<T, TResult> Ors<T, TResult>(params Generex<T, TResult>[] generexes) { return Generex<T, TResult>.Ors(generexes); }

        /// <summary>Returns a regular expression that matches a single element which is none of the specified elements.</summary>
        /// <param name="elements">List of elements excluded from matching.</param>
        public static Generex<T> Not<T>(params T[] elements) { return Generex<T>.Not(EqualityComparer<T>.Default, elements); }
        /// <summary>Returns a regular expression that matches a single element which is none of the specified elements.</summary>
        /// <param name="elements">List of elements excluded from matching.</param>
        /// <param name="comparer">Equality comparer to use.</param>
        public static Generex<T> Not<T>(IEqualityComparer<T> comparer, params T[] elements) { return Generex<T>.Not(comparer, elements); }

        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Generex<T> Recursive<T>(Func<Generex<T>, Generex<T>> generator) { return Generex<T>.Recursive(generator); }
        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Generex<T, TResult> Recursive<T, TResult>(Func<Generex<T, TResult>, Generex<T, TResult>> generator) { return Generex<T, TResult>.Recursive(generator); }

        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression, optionally starting the search at a specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        public static bool IsMatch<T>(this  T[] input, Generex<T> generex, int startAt = 0) { return generex.IsMatch(input, startAt); }

        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression, optionally starting the search at a specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        public static bool IsMatch<T>(this IEnumerable<T> input, Generex<T> generex, int startAt = 0) { return generex.IsMatch(input, startAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression at a specific index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <returns>True if a match starting at the specified index exists (which need not run all the way to the end of the sequence); otherwise, false.</returns>
        public static bool IsMatchAt<T>(this T[] input, Generex<T> generex, int mustStartAt = 0) { return generex.IsMatchAt(input, mustStartAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression up to a specific index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>True if a match ending at the specified index exists (which need not begin at the start of the sequence); otherwise, false.</returns>
        public static bool IsMatchUpTo<T>(this T[] input, Generex<T> generex, int? mustEndAt = null) { return generex.IsMatchUpTo(input, mustEndAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression exactly.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>True if a match starting and ending at the specified indexes exists; otherwise, false.</returns>
        public static bool IsMatchExact<T>(this T[] input, Generex<T> generex, int mustStartAt = 0, int? mustEndAt = null) { return generex.IsMatchExact(input, mustStartAt, mustEndAt); }

        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression that ends before the specified maximum index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="endAt">Optional index before which a match must end. The search begins by matching from this index backwards, and then proceeds towards the start of the input sequence.</param>
        public static bool IsMatchReverse<T>(this T[] input, Generex<T> generex, int? endAt = null) { return generex.IsMatchReverse(input, endAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>An object describing a regular expression match in case of success; null if no match.</returns>
        public static GenerexMatch<T> Match<T>(this T[] input, Generex<T> generex, int startAt = 0) { return generex.Match(input, startAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression exactly, and if so, returns information about the match.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>An object describing the regular expression match in case of success; null if no match.</returns>
        public static GenerexMatch<T> MatchExact<T>(this T[] input, Generex<T> generex, int mustStartAt = 0, int? mustEndAt = null) { return generex.MatchExact(input, mustStartAt, mustEndAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match
        /// found by matching the regular expression backwards (starting from the end of the input sequence).
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="endAt">Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>An object describing a regular expression match in case of success; null if no match.</returns>
        public static GenerexMatch<T> MatchReverse<T>(this T[] input, Generex<T> generex, int? endAt = null) { return generex.MatchReverse(input, endAt); }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the specified
        /// input sequence), optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        public static IEnumerable<GenerexMatch<T>> MatchesReverse<T>(this T[] input, Generex<T> generex, int? endAt = null) { return generex.MatchesReverse(input, endAt); }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the specified index.
        /// </summary>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>The behaviour is analogous to <see cref="System.Text.RegularExpressions.Regex.Matches(string,string)"/>.
        /// The documentation for that method claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public static IEnumerable<GenerexMatch<T>> Matches<T>(this T[] input, Generex<T> generex, int startAt = 0) { return generex.Matches(input, startAt); }

        /// <summary>
        /// Returns a regular expression that always matches and returns a zero-width match.
        /// </summary>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <seealso cref="GenerexNoResultBase{T,TGenerex,TGenerexMatch}.Empty"/>
        public static Generex<T> CreateEmptyGenerex<T>(this T[] input) { return Generex<T>.Empty; }

        /// <summary>
        /// Instantiates a regular expression that matches a single element that satisfies the given predicate (cf. "[...]" in traditional regular expression syntax).
        /// </summary>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <param name="predicate">The predicate that identifies matching elements.</param>
        /// <seealso cref="Generex{T}(Predicate{T})"/>
        public static Generex<T> CreateGenerex<T>(this T[] input, Predicate<T> predicate) { return new Generex<T>(predicate); }

        /// <summary>
        /// Returns a regular expression that matches a single element, no matter what it is (cf. "." in traditional regular expression syntax).
        /// </summary>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <seealso cref="GenerexNoResultBase{T,TGenerex,TGenerexMatch}.Any"/>
        public static Generex<T> CreateAnyGenerex<T>(this T[] input) { return Generex<T>.Any; }

        /// <summary>
        /// Returns a regular expression that matches the beginning of the input collection (cf. "^" in traditional regular expression syntax). Successful matches are always zero length.
        /// </summary>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <seealso cref="GenerexNoResultBase{T,TGenerex,TGenerexMatch}.Start"/>
        public static Generex<T> CreateStartGenerex<T>(this T[] input) { return Generex<T>.Start; }

        /// <summary>
        /// Returns a regular expression that matches the end of the input collection (cf. "$" in traditional regular expression syntax). Successful matches are always zero length.
        /// </summary>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <seealso cref="GenerexNoResultBase{T,TGenerex,TGenerexMatch}.End"/>
        public static Generex<T> CreateEndGenerex<T>(this T[] input) { return Generex<T>.End; }
    }
}
