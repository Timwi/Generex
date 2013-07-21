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
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <param name="elements">The sequence of elements to match.</param>
        public static Generex<T> New<T>(params T[] elements) { return new Generex<T>(elements); }
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <param name="comparer">Equality comparer to determine matching elements.</param>
        /// <param name="elements">The sequence of elements to match.</param>
        public static Generex<T> New<T>(IEqualityComparer<T> comparer, params T[] elements) { return new Generex<T>(comparer, elements); }
        /// <summary>
        /// Instantiates a regular expression that matches a single element that satisfies the given predicate (cf. <c>[...]</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <param name="predicate">Predicate that determines whether an element matches.</param>
        public static Generex<T> New<T>(Predicate<T> predicate) { return new Generex<T>(predicate); }
        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive regular expressions.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <param name="generexes">Sequence of regular expressions to match.</param>
        public static Generex<T> New<T>(params Generex<T>[] generexes) { return new Generex<T>(generexes); }

        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. <c>|</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <param name="generexes">Collection of regular expressions, one of which is matched at a time.</param>
        public static Generex<T> Ors<T>(IEnumerable<Generex<T>> generexes) { return Generex<T>.Ors(generexes); }
        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. <c>|</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <param name="generexes">Collection of regular expressions, one of which is matched at a time.</param>
        public static Generex<T> Ors<T>(params Generex<T>[] generexes) { return Generex<T>.Ors(generexes); }
        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. <c>|</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="generexes">Collection of regular expressions, one of which is matched at a time.</param>
        public static Generex<T, TResult> Ors<T, TResult>(IEnumerable<Generex<T, TResult>> generexes) { return Generex<T, TResult>.Ors(generexes); }
        /// <summary>
        /// Returns a regular expression that matches any of the specified regular expressions (cf. <c>|</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="generexes">Collection of regular expressions, one of which is matched at a time.</param>
        public static Generex<T, TResult> Ors<T, TResult>(params Generex<T, TResult>[] generexes) { return Generex<T, TResult>.Ors(generexes); }

        /// <summary>Returns a regular expression that matches a single element which is none of the specified elements.</summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <param name="elements">List of elements excluded from matching.</param>
        public static Generex<T> Not<T>(params T[] elements) { return Generex<T>.Not(EqualityComparer<T>.Default, elements); }
        /// <summary>Returns a regular expression that matches a single element which is none of the specified elements.</summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <param name="elements">List of elements excluded from matching.</param>
        /// <param name="comparer">Equality comparer to use.</param>
        public static Generex<T> Not<T>(IEqualityComparer<T> comparer, params T[] elements) { return Generex<T>.Not(comparer, elements); }

        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Generex<T> Recursive<T>(Func<Generex<T>, Generex<T>> generator) { return Generex<T>.Recursive(generator); }
        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <typeparam name="T">Type of the objects in the collection against which the regular expression will be matched.</typeparam>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Generex<T, TResult> Recursive<T, TResult>(Func<Generex<T, TResult>, Generex<T, TResult>> generator) { return Generex<T, TResult>.Recursive(generator); }

        /// <summary>Generates a regular expression that matches the specified elements in any order.</summary>
        /// <typeparam name="T">Type of the elements to match.</typeparam>
        /// <param name="elements">The elements to match.</param>
        public static Generex<T> InAnyOrder<T>(params T[] elements)
        {
            return InAnyOrder<T>(EqualityComparer<T>.Default, elements);
        }

        /// <summary>Generates a regular expression that matches the specified elements in any order.</summary>
        /// <typeparam name="T">Type of the elements to match.</typeparam>
        /// <param name="comparer">Equality comparer to determine matching elements.</param>
        /// <param name="elements">The elements to match.</param>
        public static Generex<T> InAnyOrder<T>(IEqualityComparer<T> comparer, params T[] elements)
        {
            return InAnyOrder<T>(elements.Select(elem => new Generex<T>(comparer, elem)).ToArray());
        }

        /// <summary>Generates a regular expression that matches the specified regular expressions in any order.</summary>
        /// <typeparam name="T">Type of the elements to match.</typeparam>
        /// <param name="generexes">The regular expressions to match.</param>
        public static Generex<T> InAnyOrder<T>(params Generex<T>[] generexes)
        {
            if (generexes == null)
                throw new ArgumentNullException("generexes");

            return InAnyOrder<Generex<T>, Generex<T>>(
                thenner: (prev, next) => prev.Then(next),
                orer: (one, two) => one.Or(two),
                constructor: () => new Generex<T>(),
                generexes: generexes);
        }

        /// <summary>Generates a regular expression that matches the specified regular expressions in any order.</summary>
        /// <typeparam name="T">Type of the elements to match.</typeparam>
        /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="generexes">The regular expressions to match.</param>
        public static Generex<T, IEnumerable<TResult>> InAnyOrder<T, TResult>(params Generex<T, TResult>[] generexes)
        {
            if (generexes == null)
                throw new ArgumentNullException("generexes");

            return InAnyOrder<Generex<T, TResult>, Generex<T, IEnumerable<TResult>>>(
                thenner: (prev, next) => prev.ThenRaw(next, InternalExtensions.Concat),
                orer: (one, two) => one.Or(two),
                constructor: () => new Generex<T, IEnumerable<TResult>>(Enumerable.Empty<TResult>()),
                generexes: generexes);
        }

        internal static TResultGenerex InAnyOrder<TInputGenerex, TResultGenerex>(
            Func<TInputGenerex, TResultGenerex, TResultGenerex> thenner,
            Func<TResultGenerex, TResultGenerex, TResultGenerex> orer,
            Func<TResultGenerex> constructor,
            TInputGenerex[] generexes)
            where TResultGenerex : class
        {
            TResultGenerex generex = null;
            for (int i = 0; i < generexes.Length; i++)
            {
                var subarray = new TInputGenerex[generexes.Length - 1];
                if (i > 0)
                    Array.Copy(generexes, 0, subarray, 0, i);
                if (i < generexes.Length - 1)
                    Array.Copy(generexes, i + 1, subarray, i, generexes.Length - 1 - i);
                var thisGenerex = thenner(generexes[i], InAnyOrder<TInputGenerex, TResultGenerex>(thenner, orer, constructor, subarray));
                generex = generex == null ? thisGenerex : orer(generex, thisGenerex);
            }
            return generex ?? constructor();
        }

        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression, optionally starting the search at a specified index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <typeparam name="TMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerex">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerexMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        public static bool IsMatch<T, TMatch, TGenerex, TGenerexMatch>(this  T[] input, GenerexBase<T, TMatch, TGenerex, TGenerexMatch> generex, int startAt = 0)
            where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T> { return generex.IsMatch(input, startAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression at a specific index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <typeparam name="TMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerex">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerexMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <returns>True if a match starting at the specified index exists (which need not run all the way to the end of the sequence); otherwise, false.</returns>
        public static bool IsMatchAt<T, TMatch, TGenerex, TGenerexMatch>(this T[] input, GenerexBase<T, TMatch, TGenerex, TGenerexMatch> generex, int mustStartAt = 0)
            where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T> { return generex.IsMatchAt(input, mustStartAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression up to a specific index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <typeparam name="TMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerex">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerexMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>True if a match ending at the specified index exists (which need not begin at the start of the sequence); otherwise, false.</returns>
        public static bool IsMatchUpTo<T, TMatch, TGenerex, TGenerexMatch>(this T[] input, GenerexBase<T, TMatch, TGenerex, TGenerexMatch> generex, int? mustEndAt = null)
            where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T> { return generex.IsMatchUpTo(input, mustEndAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression exactly.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <typeparam name="TMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerex">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerexMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>True if a match starting and ending at the specified indexes exists; otherwise, false.</returns>
        public static bool IsMatchExact<T, TMatch, TGenerex, TGenerexMatch>(this T[] input, GenerexBase<T, TMatch, TGenerex, TGenerexMatch> generex, int mustStartAt = 0, int? mustEndAt = null)
            where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T> { return generex.IsMatchExact(input, mustStartAt, mustEndAt); }

        /// <summary>
        /// Determines whether the given input sequence contains a match for this regular expression that ends before the specified maximum index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <typeparam name="TMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerex">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerexMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="endAt">Optional index before which a match must end. The search begins by matching from this index backwards, and then proceeds towards the start of the input sequence.</param>
        public static bool IsMatchReverse<T, TMatch, TGenerex, TGenerexMatch>(this T[] input, GenerexBase<T, TMatch, TGenerex, TGenerexMatch> generex, int? endAt = null)
            where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T> { return generex.IsMatchReverse(input, endAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <typeparam name="TMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerex">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerexMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>An object describing a regular expression match in case of success; <c>null</c> if no match.</returns>
        public static TGenerexMatch Match<T, TMatch, TGenerex, TGenerexMatch>(this T[] input, GenerexBase<T, TMatch, TGenerex, TGenerexMatch> generex, int startAt = 0)
            where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T> { return generex.Match(input, startAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression exactly, and if so, returns information about the match.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <typeparam name="TMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerex">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerexMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="mustStartAt">Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">Index at which the match must end (default is the end of the input sequence).</param>
        /// <returns>An object describing the regular expression match in case of success; <c>null</c> if no match.</returns>
        public static TGenerexMatch MatchExact<T, TMatch, TGenerex, TGenerexMatch>(this T[] input, GenerexBase<T, TMatch, TGenerex, TGenerexMatch> generex, int mustStartAt = 0, int? mustEndAt = null)
            where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T> { return generex.MatchExact(input, mustStartAt, mustEndAt); }

        /// <summary>
        /// Determines whether the given input sequence matches this regular expression, and if so, returns information about the first match
        /// found by matching the regular expression backwards (starting from the end of the input sequence).
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <typeparam name="TMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerex">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerexMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="endAt">Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>An object describing a regular expression match in case of success; <c>null</c> if no match.</returns>
        public static TGenerexMatch MatchReverse<T, TMatch, TGenerex, TGenerexMatch>(this T[] input, GenerexBase<T, TMatch, TGenerex, TGenerexMatch> generex, int? endAt = null)
            where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T> { return generex.MatchReverse(input, endAt); }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the specified
        /// input sequence), optionally starting the search at the specified index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <typeparam name="TMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerex">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerexMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="endAt">Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        public static IEnumerable<TGenerexMatch> MatchesReverse<T, TMatch, TGenerex, TGenerexMatch>(this T[] input, GenerexBase<T, TMatch, TGenerex, TGenerexMatch> generex, int? endAt = null)
            where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T> { return generex.MatchesReverse(input, endAt); }

        /// <summary>
        /// Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the specified index.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <typeparam name="TMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerex">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <typeparam name="TGenerexMatch">See <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}"/>.</typeparam>
        /// <param name="input">Input sequence to match the regular expression against.</param>
        /// <param name="generex">The regular expression to match.</param>
        /// <param name="startAt">Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>The behaviour is analogous to <see cref="System.Text.RegularExpressions.Regex.Matches(string,string)"/>.
        /// The documentation for that method claims that it returns “all occurrences of the regular expression”, but this is false.</remarks>
        public static IEnumerable<TGenerexMatch> Matches<T, TMatch, TGenerex, TGenerexMatch>(this T[] input, GenerexBase<T, TMatch, TGenerex, TGenerexMatch> generex, int startAt = 0)
            where TGenerex : GenerexBase<T, TMatch, TGenerex, TGenerexMatch>
            where TGenerexMatch : GenerexMatch<T> { return generex.Matches(input, startAt); }

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
        /// Returns a regular expression that matches any number of elements, no matter what they are; fewer are prioritized (cf. <c>.*?</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">Type of objects that the regular expression will later match against.</typeparam>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <seealso cref="GenerexNoResultBase{T,TGenerex,TGenerexMatch}.Anything"/>
        public static Generex<T> CreateAnythingGenerex<T>(this T[] input) { return Generex<T>.Anything; }

        /// <summary>
        /// Returns a regular expression that matches any number of elements, no matter what they are; more are prioritized (cf. <c>.*</c> in traditional regular expression syntax).
        /// </summary>
        /// <typeparam name="T">Type of objects that the regular expression will later match against.</typeparam>
        /// <param name="input">The value of this parameter is ignored, but can be used for type inference to enable matching of collections of anonymous types.</param>
        /// <seealso cref="GenerexNoResultBase{T,TGenerex,TGenerexMatch}.AnythingGreedy"/>
        public static Generex<T> CreateAnythingGreedyGenerex<T>(this T[] input) { return Generex<T>.AnythingGreedy; }

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
        /// Regular expressions created by this method cannot match backwards. The full set of affected methods is listed at
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then(Func{TGenerexMatch, Generex{T}})"/>.
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
        /// Regular expressions created by this method cannot match backwards. The full set of affected methods is listed at
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then(Func{TGenerexMatch, Generex{T}})"/>.
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
        /// Regular expressions created by this method cannot match backwards. The full set of affected methods is listed at
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then(Func{TGenerexMatch, Generex{T}})"/>.
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
        /// Regular expressions created by this method cannot match backwards. The full set of affected methods is listed at
        /// <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Then(Func{TGenerexMatch, Generex{T}})"/>.
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

        /// <summary>
        /// Converts the specified <see cref="Generex{T}"/> to an equivalent regular expression of type <see cref="Stringerex"/>.
        /// </summary>
        public static Stringerex ToStringerex(this Generex<char> generex)
        {
            return Stringerex.Constructor(
                (input, startIndex) => generex._forwardMatcher(input, startIndex),
                (input, startIndex) => generex._backwardMatcher(input, startIndex));
        }

        /// <summary>
        /// Converts the specified <see cref="Generex{T,TResult}"/> to an equivalent regular expression of type <see cref="Stringerex{TResult}"/>.
        /// </summary>
        public static Stringerex<TResult> ToStringerex<TResult>(this Generex<char, TResult> generex)
        {
            return Stringerex<TResult>.Constructor(
                (input, startIndex) => generex._forwardMatcher(input, startIndex),
                (input, startIndex) => generex._backwardMatcher(input, startIndex));
        }
    }
}
