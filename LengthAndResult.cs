﻿
namespace RT.Generexes
{
    /// <summary>Encapsulates preliminary information about matches generated by descendants of <see cref="GenerexWithResultBase{T,TResult,TGenerex,TGenerexMatch}"/>.</summary>
    /// <typeparam name="TResult">Type of the result object associated with each match of the regular expression.</typeparam>
    /// <remarks>This type is an implementation detail. It is not intended to be used outside this library.
    /// However, it cannot be marked internal because it is used as a generic type argument in a base-type declaration.</remarks>
    public struct LengthAndResult<TResult>
    {
        internal TResult Result { get; private set; }
        internal int Length { get; private set; }
        internal LengthAndResult(TResult result, int length) : this() { Result = result; Length = length; }
        internal LengthAndResult<TResult> Add(int extraLength)
        {
            return new LengthAndResult<TResult>(Result, Length + extraLength);
        }
    }
}
