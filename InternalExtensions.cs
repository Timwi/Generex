using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RT.Generexes
{
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
