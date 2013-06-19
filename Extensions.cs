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
        /// <see cref="Stringerex{TResult}.RawMatchReverse(string, int?)"/>
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
        /// <see cref="Stringerex{TResult}.RawMatchReverse(string, int?)"/>
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
        /// <see cref="Stringerex{TResult}.RawMatchReverse(string, int?)"/>
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
        /// <see cref="Stringerex{TResult}.RawMatchReverse(string, int?)"/>
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
