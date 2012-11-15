using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;

namespace RT.Generexes
{
    /// <summary>
    /// Provides regular-expression functionality for collections of arbitrary objects.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the collection.</typeparam>
    public sealed class Generex<T> : GenerexNoResultBase<T, Generex<T>, GenerexMatch<T>>
    {
        internal sealed override Generex<T> create(matcher forward, matcher backward) { return new Generex<T>(forward, backward); }
        internal sealed override GenerexMatch<T> createNoResultMatch(T[] input, int index, int matchLength) { return new GenerexMatch<T>(input, index, matchLength); }

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
        /// Instantiates a regular expression that matches a sequence of consecutive elements.
        /// </summary>
        public Generex(IEqualityComparer<T> comparer, IEnumerable<T> elements) : base(comparer, elements.ToArray()) { }

        /// <summary>
        /// Instantiates a regular expression that matches a single element that satisfies the given predicate (cf. "[...]" in traditional regular expression syntax).
        /// </summary>
        public Generex(Predicate<T> predicate) : base(predicate) { }

        /// <summary>
        /// Instantiates a regular expression that matches a sequence of consecutive regular expressions.
        /// </summary>
        public Generex(params GenerexNoResultBase<T, Generex<T>, GenerexMatch<T>>[] generexSequence)
            : base(
                sequenceMatcher(generexSequence, backward: false),
                sequenceMatcher(generexSequence, backward: true)) { }

        private Generex(matcher forward, matcher backward) : base(forward, backward) { }

        /// <summary>Implicitly converts an element into a regular expression that matches just that element.</summary>
        public static implicit operator Generex<T>(T element) { return new Generex<T>(element); }
        /// <summary>Implicitly converts a predicate into a regular expression that matches a single element satisfying the predicate.</summary>
        public static implicit operator Generex<T>(Predicate<T> predicate) { return new Generex<T>(predicate); }
    }
}
