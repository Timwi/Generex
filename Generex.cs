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

        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Generex<T> Recursive<T>(Func<Generex<T>, Generex<T>> generator) { return Generex<T>.Recursive(generator); }
        /// <summary>Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of arbitrarily nested expressions.</summary>
        /// <param name="generator">A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Generex<T, TResult> Recursive<T, TResult>(Func<Generex<T, TResult>, Generex<T, TResult>> generator) { return Generex<T, TResult>.Recursive(generator); }
    }
}
