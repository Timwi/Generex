using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RT.Generexes
{
    /// <summary>Provides some extension methods on some of Generex’s types.</summary>
    public static class PublicExtensions
    {
        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression, optionally starting the search at a specified index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        public static bool IsMatch<T>(this  T[] input, Generex<T> generex, int startAt = 0) { return generex.IsMatch(input, startAt); }

        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression, optionally starting the search at a specified index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        public static bool IsMatch<T>(this IEnumerable<T> input, Generex<T> generex, int startAt = 0) { return generex.IsMatch(input, startAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression at a specific index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <returns>True if a match starting at the specified index exists (which need not run all the way to the end of the sequence); otherwise, false.</returns>
        public static bool IsMatchAt<T>(this T[] input, Generex<T> generex, int mustStartAt = 0) { return generex.IsMatchAt(input, mustStartAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression up to a specific index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>True if a match ending at the specified index exists (which need not begin at the start of the sequence); otherwise, false.</returns>
        public static bool IsMatchUpTo<T>(this T[] input, Generex<T> generex, int? mustEndAt = null) { return generex.IsMatchUpTo(input, mustEndAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression exactly.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>True if a match starting and ending at the specified indexes exists; otherwise, false.</returns>
        public static bool IsMatchExact<T>(this T[] input, Generex<T> generex, int mustStartAt = 0, int? mustEndAt = null) { return generex.IsMatchExact(input, mustStartAt, mustEndAt); }

        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression that ends before the specified maximum index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="endAt">Optional index before which a match must end. The search begins by matching from this index backwards, and then proceeds towards the start of the input sequence.</param>
        public static bool IsMatchReverse<T>(this T[] input, Generex<T> generex, int? endAt = null) { return generex.IsMatchReverse(input, endAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>An object describing a regular expression match in case of success; <c>null</c> if no match.</returns>
        public static GenerexMatch<T> Match<T>(this T[] input, Generex<T> generex, int startAt = 0) { return generex.Match(input, startAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression exactly, and if so, returns information about the match.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>An object describing the regular expression match in case of success; <c>null</c> if no match.</returns>
        public static GenerexMatch<T> MatchExact<T>(this T[] input, Generex<T> generex, int mustStartAt = 0, int? mustEndAt = null) { return generex.MatchExact(input, mustStartAt, mustEndAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match
        /// found by matching the regular expression backwards (starting from the end of the input sequence).
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="endAt">Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>An object describing a regular expression match in case of success; <c>null</c> if no match.</returns>
        public static GenerexMatch<T> MatchReverse<T>(this T[] input, Generex<T> generex, int? endAt = null) { return generex.MatchReverse(input, endAt); }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the specified
        /// input sequence), optionally starting the search at the specified index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        public static IEnumerable<GenerexMatch<T>> MatchesReverse<T>(this T[] input, Generex<T> generex, int? endAt = null) { return generex.MatchesReverse(input, endAt); }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the specified index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>The behaviour is analogous to <see cref="System.Text.RegularExpressions.Regex.Matches(string,string)"/>.
        /// The documentation for that method claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public static IEnumerable<GenerexMatch<T>> Matches<T>(this T[] input, Generex<T> generex, int startAt = 0) { return generex.Matches(input, startAt); }

        /// <summary>
        /// Returns a regular expression that always matches and returns a zero-width match.
        /// </summary>
        /// <typeparam name="T">Type of objects that the regular expression will later match against.</typeparam>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <seealso cref="GenerexNoResultBase{T,TGenerex,TGenerexMatch}.Empty"/>
        public static Generex<T> CreateEmptyGenerex<T>(this T[] input) { return Generex<T>.Empty; }

        /// <summary>
        /// Instantiates a regular expression that matches a single element that satisfies the given predicate (cf. <c>[...]</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">Type of objects that the regular expression will later match against.</typeparam>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <param name="predicate">The predicate that identifies matching elements.</param>
        /// <seealso cref="Generex{T}(Predicate{T})"/>
        public static Generex<T> CreateGenerex<T>(this T[] input, Predicate<T> predicate) { return new Generex<T>(predicate); }

        /// <summary>
        /// Returns a regular expression that matches a single element, no matter what it is (cf. <c>.</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">Type of objects that the regular expression will later match against.</typeparam>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <seealso cref="GenerexNoResultBase{T,TGenerex,TGenerexMatch}.Any"/>
        public static Generex<T> CreateAnyGenerex<T>(this T[] input) { return Generex<T>.Any; }

        /// <summary>
        /// Returns a regular expression that matches the beginning of the input collection (cf. <c>^</c> in traditional regular expression syntax). Successful matches are always zero length.
        /// </summary>
        /// <typeparam name="T">Type of objects that the regular expression will later match against.</typeparam>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <seealso cref="GenerexNoResultBase{T,TGenerex,TGenerexMatch}.Start"/>
        public static Generex<T> CreateStartGenerex<T>(this T[] input) { return Generex<T>.Start; }

        /// <summary>
        /// Returns a regular expression that matches the end of the input collection (cf. <c>$</c> in traditional regular expression syntax). Successful matches are always zero length.
        /// </summary>
        /// <typeparam name="T">Type of objects that the regular expression will later match against.</typeparam>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <seealso cref="GenerexNoResultBase{T,TGenerex,TGenerexMatch}.End"/>
        public static Generex<T> CreateEndGenerex<T>(this T[] input) { return Generex<T>.End; }

        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        /// <seealso cref="Generex.Recursive{T}"/>
        public static Generex<T> CreateRecursiveGenerex<T>(this T[] input, Func<Generex<T>, Generex<T>> generator) { return Generex.Recursive<T>(generator); }

        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="input">The value of this parameter is ignored, but its static type is used for type inference to enable matching of collections of anonymous types.</param>
        /// <param name="example">The value of this parameter is ignored, but its static type is used for type inference to enable regular expressions that return result objects of anonymous types.</param>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        /// <seealso cref="Generex.Recursive{T}"/>
        public static Generex<T, TResult> CreateRecursiveGenerex<T, TResult>(this T[] input, TResult example, Func<Generex<T, TResult>, Generex<T, TResult>> generator) { return Generex.Recursive<T, TResult>(generator); }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, then uses a specified <paramref name="selector"/> to
        /// create a new regular expression from the match, and then matches the new regular expression.
        /// </summary>
        /// <param name="generex">The current regular expression.</param>
        /// <param name="selector">A delegate that creates a new regular expression from a match of the current regular expression.</param>
        /// <returns>The combined regular expression.</returns>
        /// <remarks>
        /// Regular expressions created by this method cannot match backwards. Thus, they cannot be used in calls to any of the following methods:
        /// <see cref="Stringerex.MatchReverse(string, int?)"/>, <see cref="Stringerex{TResult}.MatchReverse(string, int?)"/>,
        /// <see cref="Stringerex.IsMatchReverse(string, int?)"/>, <see cref="Stringerex{TResult}.IsMatchReverse(string, int?)"/>,
        /// <see cref="Stringerex.MatchesReverse(string, int?)"/>, <see cref="Stringerex{TResult}.MatchesReverse(string, int?)"/>,
        /// <see cref="Stringerex{TResult}.RawMatchReverse(string, int?)"/>,
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.ReplaceReverse(T[], Func{TGenerexMatch,T[]}, int?, int?)"/>,
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.ReplaceReverse(T[], IEnumerable{T}, int?, int?)"/>,
        /// <see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.ReplaceReverse(T[], Func{TResult, IEnumerable{T}}, int?, int?)"/>,
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.LookBehind"/>,
        /// <see cref="GenerexNoResultBase{T, TGenerex, TGenerexMatch}.LookBehindNegative()"/>,
        /// <see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.LookBehindNegative(TResult)"/>.
        /// </remarks>
        public static Stringerex Then<TMatch, TGenerex, TGenerexMatch>(this GenerexBase<char, TMatch, TGenerex, TGenerexMatch> generex, Func<TGenerexMatch, Stringerex> selector)
            where TGenerex : GenerexBase<char, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<char>
        {
            return generex.Then<Stringerex, int, StringerexMatch>(selector);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, then uses a specified <paramref name="selector"/> to
        /// create a new regular expression from the match, and then matches the new regular expression.
        /// </summary>
        /// <param name="generex">The current regular expression.</param>
        /// <param name="selector">A delegate that creates a new regular expression from a match of the current regular expression.</param>
        /// <returns>The combined regular expression.</returns>
        /// <remarks>
        /// Regular expressions created by this method cannot match backwards. Thus, they cannot be used in calls to any of the following methods:
        /// <see cref="Stringerex.MatchReverse(string, int?)"/>, <see cref="Stringerex{TResult}.MatchReverse(string, int?)"/>,
        /// <see cref="Stringerex.IsMatchReverse(string, int?)"/>, <see cref="Stringerex{TResult}.IsMatchReverse(string, int?)"/>,
        /// <see cref="Stringerex.MatchesReverse(string, int?)"/>, <see cref="Stringerex{TResult}.MatchesReverse(string, int?)"/>,
        /// <see cref="Stringerex{TResult}.RawMatchReverse(string, int?)"/>,
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.ReplaceReverse(T[], Func{TGenerexMatch,T[]}, int?, int?)"/>,
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.ReplaceReverse(T[], IEnumerable{T}, int?, int?)"/>,
        /// <see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.ReplaceReverse(T[], Func{TResult, IEnumerable{T}}, int?, int?)"/>,
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.LookBehind"/>,
        /// <see cref="GenerexNoResultBase{T, TGenerex, TGenerexMatch}.LookBehindNegative()"/>,
        /// <see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.LookBehindNegative(TResult)"/>.
        /// </remarks>
        public static Stringerex<TResult> Then<TMatch, TGenerex, TGenerexMatch, TResult>(this GenerexBase<char, TMatch, TGenerex, TGenerexMatch> generex, Func<TGenerexMatch, Stringerex<TResult>> selector)
            where TGenerex : GenerexBase<char, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<char>
        {
            return generex.Then<Stringerex<TResult>, LengthAndResult<TResult>, StringerexMatch<TResult>>(selector);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, then uses a specified <paramref name="selector"/> to
        /// create a new regular expression from the result of the match, and then matches the new regular expression.
        /// </summary>
        /// <param name="generex">The current regular expression.</param>
        /// <param name="selector">A delegate that creates a new regular expression from the result of a match of the current regular expression.</param>
        /// <returns>The combined regular expression.</returns>
        /// <remarks>
        /// Regular expressions created by this method cannot match backwards. Thus, they cannot be used in calls to any of the following methods:
        /// <see cref="Stringerex.MatchReverse(string, int?)"/>, <see cref="Stringerex{TResult}.MatchReverse(string, int?)"/>,
        /// <see cref="Stringerex.IsMatchReverse(string, int?)"/>, <see cref="Stringerex{TResult}.IsMatchReverse(string, int?)"/>,
        /// <see cref="Stringerex.MatchesReverse(string, int?)"/>, <see cref="Stringerex{TResult}.MatchesReverse(string, int?)"/>,
        /// <see cref="Stringerex{TResult}.RawMatchReverse(string, int?)"/>,
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.ReplaceReverse(T[], Func{TGenerexMatch,T[]}, int?, int?)"/>,
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.ReplaceReverse(T[], IEnumerable{T}, int?, int?)"/>,
        /// <see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.ReplaceReverse(T[], Func{TResult, IEnumerable{T}}, int?, int?)"/>,
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.LookBehind"/>,
        /// <see cref="GenerexNoResultBase{T, TGenerex, TGenerexMatch}.LookBehindNegative()"/>,
        /// <see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.LookBehindNegative(TResult)"/>.
        /// </remarks>
        public static Stringerex ThenRaw<TResult, TGenerex, TGenerexMatch>(this GenerexWithResultBase<char, TResult, TGenerex, TGenerexMatch> generex, Func<TGenerexMatch, Stringerex> selector)
            where TGenerex : GenerexWithResultBase<char, TResult, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<char, TResult>
        {
            return generex.Then<Stringerex, int, StringerexMatch>(selector);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, then uses a specified <paramref name="selector"/> to
        /// create a new regular expression from the result of the match, and then matches the new regular expression.
        /// </summary>
        /// <param name="generex">The current regular expression.</param>
        /// <param name="selector">A delegate that creates a new regular expression from the result of a match of the current regular expression.</param>
        /// <returns>The combined regular expression.</returns>
        /// <remarks>
        /// Regular expressions created by this method cannot match backwards. Thus, they cannot be used in calls to any of the following methods:
        /// <see cref="Stringerex.MatchReverse(string, int?)"/>, <see cref="Stringerex{TResult}.MatchReverse(string, int?)"/>,
        /// <see cref="Stringerex.IsMatchReverse(string, int?)"/>, <see cref="Stringerex{TResult}.IsMatchReverse(string, int?)"/>,
        /// <see cref="Stringerex.MatchesReverse(string, int?)"/>, <see cref="Stringerex{TResult}.MatchesReverse(string, int?)"/>,
        /// <see cref="Stringerex{TResult}.RawMatchReverse(string, int?)"/>,
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.ReplaceReverse(T[], Func{TGenerexMatch,T[]}, int?, int?)"/>,
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.ReplaceReverse(T[], IEnumerable{T}, int?, int?)"/>,
        /// <see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.ReplaceReverse(T[], Func{TResult, IEnumerable{T}}, int?, int?)"/>,
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.LookBehind"/>,
        /// <see cref="GenerexNoResultBase{T, TGenerex, TGenerexMatch}.LookBehindNegative()"/>,
        /// <see cref="GenerexWithResultBase{T, TResult, TGenerex, TGenerexMatch}.LookBehindNegative(TResult)"/>.
        /// </remarks>
        public static Stringerex<TOtherResult> ThenRaw<TResult, TGenerex, TGenerexMatch, TOtherResult>(this GenerexWithResultBase<char, TResult, TGenerex, TGenerexMatch> generex, Func<TGenerexMatch, Stringerex<TOtherResult>> selector)
            where TGenerex : GenerexWithResultBase<char, TResult, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<char, TResult>
        {
            return generex.Then<Stringerex<TOtherResult>, LengthAndResult<TOtherResult>, StringerexMatch<TOtherResult>>(selector);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf. <c>?</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">The type of objects being matched.</typeparam>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="inner">The regular expression to be modified.</param>
        /// <returns>A regular expression whose result object is a nullable version of the original result object.</returns>
        public static Generex<T, TResult?> OrNullGreedy<T, TResult>(this Generex<T, TResult> inner) where TResult : struct
        {
            return inner.ProcessRaw(result => (TResult?) result).Or(new Generex<T, TResult?>(null));
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf. <c>?</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">The type of objects being matched.</typeparam>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="inner">The regular expression to be modified.</param>
        /// <returns>A regular expression whose result object is the original result object or <c>default(TResult)</c>.</returns>
        public static Generex<T, TResult> OrDefaultGreedy<T, TResult>(this Generex<T, TResult> inner)
        {
            return inner.Or(new Generex<T, TResult>(default(TResult)));
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf. <c>?</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="inner">The regular expression to be modified.</param>
        /// <returns>A regular expression whose result object is a nullable version of the original result object.</returns>
        public static Stringerex<TResult?> OrNullGreedy<TResult>(this Stringerex<TResult> inner) where TResult : struct
        {
            return inner.ProcessRaw(result => (TResult?) result).Or(new Stringerex<TResult?>(null));
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Once is prioritised (cf. <c>?</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="inner">The regular expression to be modified.</param>
        /// <returns>A regular expression whose result object is the original result object or <c>default(TResult)</c>.</returns>
        public static Stringerex<TResult> OrDefaultGreedy<TResult>(this Stringerex<TResult> inner)
        {
            return inner.Or(new Stringerex<TResult>(default(TResult)));
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Zero times is prioritised (cf. <c>??</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">The type of objects being matched.</typeparam>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="inner">The regular expression to be modified.</param>
        /// <returns>A regular expression whose result object is a nullable version of the original result object.</returns>
        public static Generex<T, TResult?> OrNull<T, TResult>(this Generex<T, TResult> inner) where TResult : struct
        {
            return new Generex<T, TResult?>(null).Or(inner.ProcessRaw(result => (TResult?) result));
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Zero times is prioritised (cf. <c>??</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">The type of objects being matched.</typeparam>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="inner">The regular expression to be modified.</param>
        /// <returns>A regular expression whose result object is the original result object or <c>default(TResult)</c>.</returns>
        public static Generex<T, TResult> OrDefault<T, TResult>(this Generex<T, TResult> inner)
        {
            return new Generex<T, TResult>(default(TResult)).Or(inner);
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Zero times is prioritised (cf. <c>??</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="inner">The regular expression to be modified.</param>
        /// <returns>A regular expression whose result object is a nullable version of the original result object.</returns>
        public static Stringerex<TResult?> OrNull<TResult>(this Stringerex<TResult> inner) where TResult : struct
        {
            return new Stringerex<TResult?>(null).Or(inner.ProcessRaw(result => (TResult?) result));
        }

        /// <summary>
        /// Returns a regular expression that matches this regular expression zero times or once. Zero times is prioritised (cf. <c>??</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="inner">The regular expression to be modified.</param>
        /// <returns>A regular expression whose result object is the original result object or <c>default(TResult)</c>.</returns>
        public static Stringerex<TResult> OrDefault<TResult>(this Stringerex<TResult> inner)
        {
            return new Stringerex<TResult>(default(TResult)).Or(inner);
        }
    }

    static class InternalExtensions
    {
        /// <summary>Adds a single element to the end of an IEnumerable.</summary>
        /// <typeparam name="T">Type of enumerable to return.</typeparam>
        /// <returns>IEnumerable containing all the input elements, followed by the specified additional element.</returns>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T element)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return concatIterator(element, source, false);
        }

        /// <summary>Adds a single element to the start of an IEnumerable.</summary>
        /// <typeparam name="T">Type of enumerable to return.</typeparam>
        /// <returns>IEnumerable containing the specified additional element, followed by all the input elements.</returns>
        public static IEnumerable<T> Concat<T>(this T head, IEnumerable<T> tail)
        {
            if (tail == null)
                throw new ArgumentNullException("tail");
            return concatIterator(head, tail, true);
        }

        private static IEnumerable<T> concatIterator<T>(T extraElement, IEnumerable<T> source, bool insertAtStart)
        {
            if (insertAtStart)
                yield return extraElement;
            foreach (var e in source)
                yield return e;
            if (!insertAtStart)
                yield return extraElement;
        }

        /// <summary>
        /// Similar to <see cref="string.Substring(int,int)"/>, only for arrays. Returns a new array containing
        /// <paramref name="length"/> items from the specified <paramref name="startIndex"/> onwards.
        /// </summary>
        /// <remarks>Returns a new copy of the array even if <paramref name="startIndex"/> is 0 and
        /// <paramref name="length"/> is the length of the input array.</remarks>
        public static T[] Subarray<T>(this T[] array, int startIndex, int length)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "startIndex cannot be negative.");
            if (length < 0 || startIndex + length > array.Length)
                throw new ArgumentOutOfRangeException("length", "length cannot be negative or extend beyond the end of the array.");
            T[] result = new T[length];
            Array.Copy(array, startIndex, result, 0, length);
            return result;
        }

        /// <summary>
        /// Determines whether a subarray within the current array is equal to the specified other array.
        /// </summary>
        /// <param name="sourceArray">First array to examine.</param>
        /// <param name="sourceStartIndex">Start index of the subarray within the first array to compare.</param>
        /// <param name="otherArray">Array to compare the subarray against.</param>
        /// <param name="comparer">Optional equality comparer.</param>
        /// <returns>True if the current array contains the specified subarray at the specified index; false otherwise.</returns>
        public static bool SubarrayEquals<T>(this T[] sourceArray, int sourceStartIndex, T[] otherArray, IEqualityComparer<T> comparer = null)
        {
            if (otherArray == null)
                throw new ArgumentNullException("otherArray");
            return SubarrayEquals(sourceArray, sourceStartIndex, otherArray, 0, otherArray.Length, comparer);
        }

        /// <summary>
        /// Determines whether the two arrays contain the same content in the specified location.
        /// </summary>
        /// <param name="sourceArray">First array to examine.</param>
        /// <param name="sourceStartIndex">Start index of the subarray within the first array to compare.</param>
        /// <param name="otherArray">Second array to examine.</param>
        /// <param name="otherStartIndex">Start index of the subarray within the second array to compare.</param>
        /// <param name="length">Length of the subarrays to compare.</param>
        /// <param name="comparer">Optional equality comparer.</param>
        /// <returns>True if the two arrays contain the same subarrays at the specified indexes; false otherwise.</returns>
        public static bool SubarrayEquals<T>(this T[] sourceArray, int sourceStartIndex, T[] otherArray, int otherStartIndex, int length, IEqualityComparer<T> comparer = null)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (sourceStartIndex < 0)
                throw new ArgumentOutOfRangeException("The sourceStartIndex argument must be non-negative.", "sourceStartIndex");
            if (otherArray == null)
                throw new ArgumentNullException("otherArray");
            if (otherStartIndex < 0)
                throw new ArgumentOutOfRangeException("The otherStartIndex argument must be non-negative.", "otherStartIndex");
            if (length < 0 || sourceStartIndex + length > sourceArray.Length || otherStartIndex + length > otherArray.Length)
                throw new ArgumentOutOfRangeException("The length argument must be non-negative and must be such that both subarrays are within the bounds of the respective source arrays.", "length");

            if (comparer == null)
                comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < length; i++)
                if (!comparer.Equals(sourceArray[sourceStartIndex + i], otherArray[otherStartIndex + i]))
                    return false;
            return true;
        }

        public static Func<T, TResult> Lambda<T, TResult>(Func<T, TResult> func) { return func; }
    }
}
