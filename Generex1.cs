﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RT.Generexes
{
    /// <summary>
    /// Provides regular-expression functionality for collections of arbitrary objects.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    public sealed class Generex<T> : GenerexNoResultBase<T, Generex<T>, GenerexMatch<T>>
    {
        /// <summary>Instantiates a <see cref="GenerexMatch{T}"/> object from an index and length.</summary>
        /// <param name="input">Original input array that was matched against.</param>
        /// <param name="index">Start index of the match.</param>
        /// <param name="matchLength">Length of the match.</param>
        protected sealed override GenerexMatch<T> createNoResultMatch(T[] input, int index, int matchLength)
        {
            return new GenerexMatch<T>(input, index, matchLength);
        }

        /// <summary>
        /// Instantiates an empty regular expression (always matches).
        /// </summary>
        public Generex() : base(emptyMatch, emptyMatch) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public Generex(params T[] elements) : base(EqualityComparer<T>.Default, elements) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public Generex(IEnumerable<T> elements) : base(EqualityComparer<T>.Default, elements.ToArray()) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        public Generex(IEqualityComparer<T> comparer, params T[] elements) : base(comparer, elements) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive elements using the specified equality comparer.
        /// </summary>
        public Generex(IEqualityComparer<T> comparer, IEnumerable<T> elements) : base(comparer, elements.ToArray()) { }

        /// <summary>
        /// Instantiates a regular expression that matches a single element that satisfies the given predicate (cf. <c>[...]</c> in traditional regular expression syntax).
        /// </summary>
        /// <param name="predicate">The predicate that identifies matching elements.</param>
        public Generex(Predicate<T> predicate) : base(predicate) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive regular expressions.
        /// </summary>
        public Generex(params Generex<T>[] generexSequence)
            : base(
                sequenceMatcher(generexSequence, backward: false),
                sequenceMatcher(generexSequence, backward: true)) { }

        private Generex(matcher forward, matcher backward) : base(forward, backward) { }
        static Generex() { Constructor = (forward, backward) => new Generex<T>(forward, backward); }

        /// <summary>Implicitly converts an element into a regular expression that matches just that element.</summary>
        public static implicit operator Generex<T>(T element) { return new Generex<T>(element); }
        /// <summary>Implicitly converts a predicate into a regular expression that matches a single element satisfying the predicate.</summary>
        public static implicit operator Generex<T>(Predicate<T> predicate) { return new Generex<T>(predicate); }

        /// <summary>
        /// Returns a regular expression that matches this regular expression, followed by the specified other,
        /// and retains the match object generated by each match of the other regular expression.
        /// </summary>
        public Generex<T, TResult> Then<TResult>(Generex<T, TResult> other) { return then<Generex<T, TResult>, GenerexMatch<T, TResult>, TResult>(other); }

        /// <summary>Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TResult">Type of the object returned by <paramref name="selector"/>, which is a result object associated with each match of the regular expression.</typeparam>
        /// <param name="selector">Function to process a regular expression match.</param>
        public Generex<T, TResult> Process<TResult>(Func<GenerexMatch<T>, TResult> selector)
        {
            return process<Generex<T, TResult>, GenerexMatch<T, TResult>, TResult>(selector);
        }
    }
}
