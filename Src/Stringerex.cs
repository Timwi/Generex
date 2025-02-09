﻿using System.Globalization;

namespace RT.Generexes
{
    /// <summary>Provides regular-expression functionality for strings.</summary>
    public class Stringerex : GenerexNoResultBase<char, Stringerex, StringerexMatch>
    {
        /// <summary>
        ///     Instantiates a <see cref="GenerexMatch{T}"/> object from an index and length.</summary>
        /// <param name="input">
        ///     Original input array that was matched against.</param>
        /// <param name="index">
        ///     Start index of the match.</param>
        /// <param name="matchLength">
        ///     Length of the match.</param>
        protected sealed override StringerexMatch createNoResultMatch(char[] input, int index, int matchLength)
        {
            return new StringerexMatch(input, index, matchLength);
        }

        /// <summary>Instantiates an empty regular expression (always matches).</summary>
        public Stringerex() : base(emptyMatch, emptyMatch) { }

        /// <summary>Instantiates a regular expression that matches a specified character.</summary>
        public Stringerex(char ch) : base(new[] { ch }, EqualityComparer<char>.Default) { }

        /// <summary>Instantiates a regular expression that matches a sequence of consecutive characters.</summary>
        public Stringerex(string elements) : base(elements.ThrowIfNull("elements").ToCharArray(), EqualityComparer<char>.Default) { }

        /// <summary>Instantiates a regular expression that matches a sequence of consecutive characters.</summary>
        public Stringerex(IEnumerable<char> elements) : base(elements.ThrowIfNull("elements").ToArray(), EqualityComparer<char>.Default) { }

        /// <summary>
        ///     Instantiates a regular expression that matches a specified character using the specified optional equality
        ///     comparer.</summary>
        public Stringerex(char ch, IEqualityComparer<char> comparer) : base(new[] { ch }, comparer) { }

        /// <summary>
        ///     Instantiates a regular expression that matches a sequence of consecutive characters using the specified
        ///     optional equality comparer.</summary>
        public Stringerex(string elements, IEqualityComparer<char> comparer) : base(elements.ThrowIfNull("elements").ToCharArray(), comparer) { }

        /// <summary>
        ///     Instantiates a regular expression that matches a sequence of consecutive characters using the specified
        ///     optional equality comparer.</summary>
        public Stringerex(IEnumerable<char> elements, IEqualityComparer<char> comparer) : base(elements.ThrowIfNull("elements").ToArray(), comparer) { }

        /// <summary>
        ///     Instantiates a regular expression that matches a single character that satisfies the given predicate (cf.
        ///     <c>[...]</c> in traditional regular expression syntax).</summary>
        public Stringerex(Predicate<char> predicate) : base(predicate) { }

        /// <summary>
        ///     Instantiates a regular expression that matches a single Unicode character according to a predicate that takes
        ///     a string and index, such as <c>char.IsWhiteSpace</c> (cf. <c>[...]</c> in traditional regular expression
        ///     syntax).</summary>
        public Stringerex(Func<string, int, bool> charPredicate)
            : base(
                (input, startIndex) =>
                    startIndex == input.Length ? Generex.NoMatch :
                    startIndex == input.Length - 1 ? (charPredicate(input[startIndex].ToString(), 0) ? Generex.OneElementMatch : Generex.NoMatch) :
                    charPredicate(input[startIndex].ToString() + input[startIndex + 1], 0) ? new[] { char.IsSurrogate(input[startIndex]) ? 2 : 1 } : Generex.NoMatch,
                (input, startIndex) =>
                    startIndex == 0 ? Generex.NoMatch :
                    startIndex == 1 ? (charPredicate(input[0].ToString(), 0) ? Generex.OneElementMatch : Generex.NoMatch) :
                    charPredicate(input[startIndex - 2].ToString() + input[startIndex - 1], 0) ? new[] { char.IsSurrogate(input[startIndex - 2]) ? 2 : 1 } : Generex.NoMatch
            ) { }

        /// <summary>
        ///     Instantiates a regular expression that matches a single Unicode character of a specific Unicode character (cf.
        ///     <c>[...]</c> in traditional regular expression syntax).</summary>
        public Stringerex(UnicodeCategory category)
            : this((s, i) => char.GetUnicodeCategory(s, i) == category) { }

        /// <summary>Instantiates a regular expression that matches a sequence of consecutive regular expressions.</summary>
        public Stringerex(params Stringerex[] generexSequence)
            : base(
                sequenceMatcher(generexSequence, backward: false),
                sequenceMatcher(generexSequence, backward: true)) { }

        private Stringerex(matcher forward, matcher backward) : base(forward, backward) { }
        static Stringerex() { Constructor = (forward, backward) => new Stringerex(forward, backward); }

        /// <summary>Implicitly converts a character into a regular expression that matches just that character.</summary>
        public static implicit operator Stringerex(char ch) { return new Stringerex(new[] { ch }); }
        /// <summary>Implicitly converts a string into a regular expression that matches that string.</summary>
        public static implicit operator Stringerex(string str) { return new Stringerex(str); }
        /// <summary>
        ///     Implicitly converts a predicate into a regular expression that matches a single character satisfying the
        ///     predicate.</summary>
        public static implicit operator Stringerex(Predicate<char> predicate) { return new Stringerex(predicate); }

        /// <summary>
        ///     Determines whether the given string contains a match for this regular expression, optionally starting the
        ///     search at a specified character index.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="startAt">
        ///     Optional character index at which to start the search. Matches that start before this index are not included.</param>
        public bool IsMatch(string input, int startAt = 0)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return base.IsMatch(input.ToCharArray(), startAt);
        }

        /// <summary>
        ///     Determines whether the given string matches this regular expression at a specific character index.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="mustStartAt">
        ///     Index at which the match must start (default is 0).</param>
        /// <returns>
        ///     <c>true</c> if a match starting at the specified index exists (which need not run all the way to the end of
        ///     the string); otherwise, <c>false</c>.</returns>
        public bool IsMatchAt(string input, int mustStartAt = 0)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return base.IsMatchAt(input.ToCharArray(), mustStartAt);
        }

        /// <summary>
        ///     Determines whether the given string matches this regular expression up to a specific character index.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="mustEndAt">
        ///     Index at which the match must end (default is the end of the string).</param>
        /// <returns>
        ///     <c>true</c> if a match ending at the specified index exists (which need not begin at the start of the string);
        ///     otherwise, <c>false</c>.</returns>
        public bool IsMatchUpTo(string input, int? mustEndAt = null)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return base.IsMatchUpTo(input.ToCharArray(), mustEndAt);
        }

        /// <summary>
        ///     Determines whether the given string matches this regular expression exactly.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="mustStartAt">
        ///     Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">
        ///     Index at which the match must end (default is the end of the string).</param>
        /// <returns>
        ///     <c>true</c> if a match starting and ending at the specified indexes exists; otherwise, <c>false</c>.</returns>
        public bool IsMatchExact(string input, int mustStartAt = 0, int? mustEndAt = null)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return base.IsMatchExact(input.ToCharArray(), mustStartAt, mustEndAt);
        }

        /// <summary>
        ///     Determines whether the given string contains a match for this regular expression that ends before the
        ///     specified maximum character index.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="endAt">
        ///     Optional index before which a match must end. The search begins by matching from this index backwards, and
        ///     then proceeds towards the start of the string.</param>
        public bool IsMatchReverse(string input, int? endAt = null)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return base.IsMatchReverse(input.ToCharArray(), endAt);
        }

        /// <summary>
        ///     Determines whether the given string matches this regular expression, and if so, returns information about the
        ///     first match.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="startAt">
        ///     Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <returns>
        ///     An object describing a regular expression match in case of success; <c>null</c> if no match.</returns>
        public StringerexMatch Match(string input, int startAt = 0)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return base.Match(input.ToCharArray(), startAt);
        }

        /// <summary>
        ///     Determines whether the given string matches this regular expression exactly, and if so, returns information
        ///     about the match.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="mustStartAt">
        ///     Index at which the match must start (default is 0).</param>
        /// <param name="mustEndAt">
        ///     Index at which the match must end (default is the end of the string).</param>
        /// <returns>
        ///     An object describing the regular expression match in case of success; <c>null</c> if no match.</returns>
        public StringerexMatch MatchExact(string input, int mustStartAt = 0, int? mustEndAt = null)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return base.MatchExact(input.ToCharArray(), mustStartAt, mustEndAt);
        }

        /// <summary>
        ///     Determines whether the given string matches this regular expression, and if so, returns information about the
        ///     first match found by matching the regular expression backwards (starting from the end of the string).</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="endAt">
        ///     Optional index at which to end the search. Matches that end at or after this index are not included.</param>
        /// <returns>
        ///     An object describing a regular expression match in case of success; <c>null</c> if no match.</returns>
        public StringerexMatch MatchReverse(string input, int? endAt = null)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return base.MatchReverse(input.ToCharArray(), endAt);
        }

        /// <summary>
        ///     Returns a sequence of non-overlapping regular expression matches going backwards (starting at the end of the
        ///     specified string), optionally starting the search at the specified index.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="endAt">
        ///     Optional index at which to begin the reverse search. Matches that end at or after this index are not included.</param>
        public IEnumerable<StringerexMatch> MatchesReverse(string input, int? endAt = null)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return base.MatchesReverse(input.ToCharArray(), endAt);
        }

        /// <summary>
        ///     Returns a sequence of non-overlapping regular expression matches, optionally starting the search at the
        ///     specified character index.</summary>
        /// <param name="input">
        ///     String to match the regular expression against.</param>
        /// <param name="startAt">
        ///     Optional index at which to start the search. Matches that start before this index are not included.</param>
        /// <remarks>
        ///     The behaviour is analogous to <see cref="System.Text.RegularExpressions.Regex.Matches(string,string)"/>. The
        ///     documentation for that method claims that it returns “all occurrences of the regular expression”, but this is
        ///     false.</remarks>
        public IEnumerable<StringerexMatch> Matches(string input, int startAt = 0)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return base.Matches(input.ToCharArray(), startAt);
        }

        /// <summary>
        ///     Returns a regular expression that matches either this regular expression or the specified string using the
        ///     specified equality comparer (cf. <c>|</c> or <c>[...]</c> in traditional regular expression syntax).</summary>
        public Stringerex Or(string str, IEqualityComparer<char> comparer = null) { return base.Or(comparer ?? EqualityComparer<char>.Default, str.ToCharArray()); }

        /// <summary>
        ///     Returns a regular expression that matches this regular expression followed by the specified string, using the
        ///     specified equality comparer.</summary>
        public Stringerex Then(string str, IEqualityComparer<char> comparer = null) { return base.Then(comparer ?? EqualityComparer<char>.Default, str.ToCharArray()); }

        /// <summary>
        ///     Processes each match of this regular expression by running it through a provided selector.</summary>
        /// <typeparam name="TResult">
        ///     Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="selector">
        ///     Function to process a regular expression match.</param>
        public Stringerex<TResult> Process<TResult>(Func<StringerexMatch, TResult> selector) { return process<Stringerex<TResult>, StringerexMatch<TResult>, TResult>(selector); }

        /// <summary>
        ///     Generates a recursive regular expression, i.e. one that can contain itself, allowing the matching of
        ///     arbitrarily nested expressions.</summary>
        /// <param name="generator">
        ///     A function that generates the regular expression from an object that recursively represents the result.</param>
        public static Stringerex<TResult> Recursive<TResult>(Func<Stringerex<TResult>, Stringerex<TResult>> generator) { return Stringerex<TResult>.Recursive(generator); }

        /// <summary>
        ///     Generates a regular expression that matches the characters of the specified string in any order.</summary>
        /// <param name="characters">
        ///     A string containing the characters to match.</param>
        /// <param name="comparer">
        ///     The optional equality comparer to use to determine matching characters.</param>
        public static Stringerex InAnyOrder(string characters, IEqualityComparer<char> comparer = null)
        {
            if (characters == null)
                throw new ArgumentNullException("characters");

            comparer = comparer ?? EqualityComparer<char>.Default;

            return Generex.InAnyOrder<Stringerex, Stringerex>(
                thenner: (prev, next) => prev.Then(next),
                orer: (one, two) => one.Or(two),
                constructor: () => new Stringerex(),
                generexes: characters.Select(ch => new Stringerex(ch, comparer)).ToArray());
        }

        /// <summary>
        ///     Generates a regular expression that matches the specified regular expressions in any order.</summary>
        /// <param name="stringerexes">
        ///     The regular expressions to match.</param>
        public static Stringerex InAnyOrder(params Stringerex[] stringerexes)
        {
            if (stringerexes == null)
                throw new ArgumentNullException("stringerexes");

            return Generex.InAnyOrder<Stringerex, Stringerex>(
                thenner: (prev, next) => prev.Then(next),
                orer: (one, two) => one.Or(two),
                constructor: () => new Stringerex(),
                generexes: stringerexes);
        }

        /// <summary>
        ///     Generates a regular expression that matches the specified regular expressions in any order.</summary>
        /// <typeparam name="TResult">
        ///     Type of the result object associated with each match of the regular expression.</typeparam>
        /// <param name="stringerexes">
        ///     The regular expressions to match.</param>
        public static Stringerex<IEnumerable<TResult>> InAnyOrder<TResult>(params Stringerex<TResult>[] stringerexes)
        {
            if (stringerexes == null)
                throw new ArgumentNullException("stringerexes");

            return Generex.InAnyOrder<Stringerex<TResult>, Stringerex<IEnumerable<TResult>>>(
                thenner: (prev, next) => prev.ThenRaw(next, InternalExtensions.Concat),
                orer: (one, two) => one.Or(two),
                constructor: () => new Stringerex<IEnumerable<TResult>>(Enumerable.Empty<TResult>()),
                generexes: stringerexes);
        }

        /// <summary>
        ///     Returns a regular expression that matches either one of the specified regular expressions (cf. <c>|</c> in
        ///     traditional regular expression syntax).</summary>
        public static Stringerex operator |(Stringerex one, Generex<char> two) { return one.Or(two); }

        /// <summary>
        ///     Returns a regular expression that matches either one of the specified regular expressions (cf. <c>|</c> in
        ///     traditional regular expression syntax).</summary>
        public static Stringerex operator |(Generex<char> one, Stringerex two) { return one.Or(two).ToStringerex(); }

        /// <summary>
        ///     Throws an exception generated by the specified code when the regular expression engine encounters this
        ///     expression.</summary>
        public Stringerex<TResult> Throw<TResult>(Func<StringerexMatch, Exception> exceptionGenerator)
        {
            return process<Stringerex<TResult>, StringerexMatch<TResult>, TResult>(m => { throw exceptionGenerator(m); });
        }

        /// <summary>
        ///     Attempts to match the specified regular expression and throws an exception generated by the specified code if
        ///     the regular expression does not match.</summary>
        public static Stringerex Expect(Stringerex stringerex, Func<StringerexMatch, Exception> exceptionGenerator)
        {
            return Stringerex.Ors(stringerex, new Stringerex().Throw(exceptionGenerator)).Atomic();
        }

        /// <summary>
        ///     Attempts to match the specified sequence of consecutive regular expressions and throws an exception generated
        ///     by the specified code if the regular expression does not match.</summary>
        public static Stringerex Expect(Stringerex[] stringerexes, Func<StringerexMatch, Exception> exceptionGenerator)
        {
            return Stringerex.Ors(new Stringerex(stringerexes), new Stringerex().Throw(exceptionGenerator)).Atomic();
        }

        /// <summary>
        ///     Attempts to match an element with the specified predicate and throws an exception generated by the specified
        ///     code if the regular expression does not match.</summary>
        public static Stringerex Expect(Predicate<char> predicate, Func<StringerexMatch, Exception> exceptionGenerator)
        {
            return Stringerex.Ors(new Stringerex(predicate), new Stringerex().Throw(exceptionGenerator)).Atomic();
        }

        /// <summary>
        ///     Attempts to match the specified character and throws an exception generated by the specified code if the
        ///     regular expression does not match.</summary>
        public static Stringerex Expect(char element, Func<StringerexMatch, Exception> exceptionGenerator)
        {
            return Stringerex.Ors(new Stringerex(element), new Stringerex().Throw(exceptionGenerator)).Atomic();
        }

        /// <summary>
        ///     Attempts to match the specified character using the specified comparer and throws an exception generated by
        ///     the specified code if the regular expression does not match.</summary>
        public static Stringerex Expect(char element, IEqualityComparer<char> comparer, Func<StringerexMatch, Exception> exceptionGenerator)
        {
            return Stringerex.Ors(new Stringerex(element, comparer), new Stringerex().Throw(exceptionGenerator)).Atomic();
        }

        /// <summary>
        ///     Attempts to match the specified string and throws an exception generated by the specified code if the regular
        ///     expression does not match.</summary>
        public static Stringerex Expect(string elements, Func<StringerexMatch, Exception> exceptionGenerator)
        {
            return new Stringerex(elements).Or(new Stringerex().Throw(exceptionGenerator)).Atomic();
        }

        /// <summary>
        ///     Attempts to match the specified string using the specified character comparer and throws an exception
        ///     generated by the specified code if the regular expression does not match.</summary>
        public static Stringerex Expect(string elements, IEqualityComparer<char> comparer, Func<StringerexMatch, Exception> exceptionGenerator)
        {
            return new Stringerex(elements, comparer).Or(new Stringerex().Throw(exceptionGenerator)).Atomic();
        }

        /// <summary>
        ///     Attempts to match the specified sequence of characters and throws an exception generated by the specified code
        ///     if the regular expression does not match.</summary>
        public static Stringerex Expect(IEnumerable<char> elements, Func<StringerexMatch, Exception> exceptionGenerator)
        {
            return new Stringerex(elements).Or(new Stringerex().Throw(exceptionGenerator)).Atomic();
        }

        /// <summary>
        ///     Attempts to match the specified sequence of characters using the specified character comparer and throws an
        ///     exception generated by the specified code if the regular expression does not match.</summary>
        public static Stringerex Expect(IEnumerable<char> elements, IEqualityComparer<char> comparer, Func<StringerexMatch, Exception> exceptionGenerator)
        {
            return new Stringerex(elements, comparer).Or(new Stringerex().Throw(exceptionGenerator)).Atomic();
        }

        /// <summary>
        ///     Attempts to match the specified regular expression and throws an exception generated by the specified code if
        ///     the regular expression does not match.</summary>
        public static Stringerex<TResult> Expect<TResult>(Stringerex<TResult> stringerex, Func<StringerexMatch, Exception> exceptionGenerator)
        {
            return stringerex.Or(new Stringerex().Throw<TResult>(exceptionGenerator)).Atomic();
        }
    }
}
