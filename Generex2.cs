
namespace RT.Generexes
{
    /// <summary>
    /// Provides regular-expression functionality for collections of arbitrary objects.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    /// <typeparam name="TResult">Type of objects generated from each match of the regular expression.</typeparam>
    public sealed class Generex<T, TResult> : GenerexWithResultBase<T, TResult, Generex<T, TResult>, GenerexMatch<T, TResult>>
    {
        internal sealed override GenerexMatch<T, TResult> createMatchWithResult(TResult result, T[] input, int index, int length)
        {
            return new GenerexMatch<T, TResult>(result, input, index, length);
        }

        /// <summary>
        /// Instantiates an empty regular expression which always matches and returns the default result object (null or zero).
        /// </summary>
        public Generex() : base(default(TResult)) { }

        /// <summary>
        /// Instantiates an empty regular expression which always matches and returns the specified result object.
        /// </summary>
        public Generex(TResult result) : base(result) { }

        internal Generex(matcher forward, matcher backward) : base(forward, backward) { }
        static Generex() { Constructor = (forward, backward) => new Generex<T, TResult>(forward, backward); }
    }
}
