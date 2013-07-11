
namespace RT.Generexes
{
    /// <summary>
    /// Represents the result of a regular expression match using <see cref="Generex{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    public class GenerexMatch<T>
    {
        /// <summary>
        /// Gets the index in the original collection at which the match occurred.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the length of the match.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the entire original sequence against which the regular expression was matched.
        /// </summary>
        public T[] OriginalSource { get; private set; }

        /// <summary>
        /// Returns a slice of the original collection which the regular expression matched.
        /// </summary>
        public T[] Match { get { return _match ?? (_match = OriginalSource.Subarray(Index, Length)); } }
        private T[] _match;

        internal GenerexMatch(T[] original, int index, int length)
        {
            if (length < 0)
            {
                Index = index + length;
                Length = -length;
            }
            else
            {
                Index = index;
                Length = length;
            }
            OriginalSource = original;
        }
    }

    /// <summary>
    /// Represents the result of a regular expression match using <see cref="Stringerex"/>.
    /// </summary>
    public class StringerexMatch : GenerexMatch<char>
    {
        /// <summary>
        /// Gets the entire original string against which the regular expression was matched.
        /// </summary>
        public new string OriginalSource { get { return _originalSource ?? (_originalSource = new string(base.OriginalSource)); } }
        private string _originalSource;

        /// <summary>
        /// Returns the part of the original string which the regular expression matched.
        /// </summary>
        public new string Match { get { return _match ?? (_match = new string(base.Match)); } }
        private string _match;

        internal StringerexMatch(char[] original, int index, int length) : base(original, index, length) { }
    }

    /// <summary>
    /// Represents the result of a regular expression match using <see cref="Generex{T,TResult}"/>.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    /// <typeparam name="TResult">Type of the result object associated with this regular expression match.</typeparam>
    public class GenerexMatch<T, TResult> : GenerexMatch<T>
    {
        /// <summary>Contains the object generated from this match of the regular expression.</summary>
        public TResult Result { get; private set; }

        internal GenerexMatch(TResult result, T[] original, int index, int length)
            : base(original, index, length)
        {
            Result = result;
        }
    }

    /// <summary>
    /// Represents the result of a regular expression match using <see cref="Stringerex{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">Type of the result object associated with this regular expression match.</typeparam>
    public class StringerexMatch<TResult> : GenerexMatch<char, TResult>
    {
        /// <summary>
        /// Gets the entire original string against which the regular expression was matched.
        /// </summary>
        public new string OriginalSource { get { return _originalSource ?? (_originalSource = new string(base.OriginalSource)); } }
        private string _originalSource;

        /// <summary>
        /// Returns the part of the original string which the regular expression matched.
        /// </summary>
        public new string Match { get { return _match ?? (_match = new string(base.Match)); } }
        private string _match;

        internal StringerexMatch(TResult result, char[] original, int index, int length) : base(result, original, index, length) { }
    }
}
