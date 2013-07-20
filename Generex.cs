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
            params TInputGenerex[] generexes)
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
    }
}
