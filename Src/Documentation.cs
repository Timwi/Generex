using System;
using System.Collections.Generic;

namespace RT.Generexes
{
    /// <summary>
    ///     <heading>
    ///         Flexible Regular Expressions</heading>
    ///     <para>
    ///         The goal of Generex is to allow the same kind of pattern matching, analysis, parsing and searching as typical
    ///         regular expression engines, but instead of limiting itself to matching characters in a string, it can match
    ///         against any array of objects.</para>
    ///     <para>
    ///         For an example, imagine you have a list of log entries, and you need to find clusters of at least three
    ///         consecutive log entries from the same day:</para>
    ///     <code monospace="true">
    ///         16/06/2013 15:15:50
    ///         17/06/2013 02:09:09     ┐
    ///         17/06/2013 21:00:18     ├ 3 entries here
    ///         17/06/2013 23:44:28     ┘
    ///         18/06/2013 09:39:34
    ///         19/06/2013 08:30:36     ┐
    ///         19/06/2013 09:28:52     ├ 3 entries here
    ///         19/06/2013 14:34:12     ┘
    ///         20/06/2013 00:07:58
    ///         20/06/2013 01:54:10
    ///         22/06/2013 07:31:00
    ///         23/06/2013 03:35:56     ┐
    ///         23/06/2013 10:06:10     ├ 4 entries here
    ///         23/06/2013 12:05:59     │
    ///         23/06/2013 23:43:35     ┘
    ///         24/06/2013 19:46:16
    ///         25/06/2013 17:42:17
    ///         26/06/2013 11:29:37</code>
    ///     <para>
    ///         This <em>sounds</em> like the typical kind of pattern matching that regular expressions are made for, but
    ///         instead of a <c>string</c> or <c>char[]</c>, you have a <c>DateTime[]</c> (or maybe a <c>LogEntry[]</c>, where
    ///         <c>LogEntry</c> is your own class).</para>
    ///     <para>
    ///         Generex uses the power of generics to bring you regular expression functionality on any type. As a trade-off,
    ///         instead of an extremely terse text-based syntax, it uses method calls to construct the regular expression.
    ///         Here are only a few basic examples:</para>
    ///     <code monospace="true">
    ///           Regex  │  Generex
    ///         ═════════╪════════════════════
    ///               a  │  Generex.New('a')
    ///              ab  │  a.Then(b)
    ///             a|b  │  a.Or(b)
    ///             a*?  │  a.Repeat()
    ///              a*  │  a.RepeatGreedy()
    ///           (?=a)  │  a.LookAhead()
    ///          (?&lt;=a)  │  a.LookBehind()
    ///           (?&gt;a)  │  a.Atomic()</code>
    ///     <para>
    ///         For example, a regular expression such as the following:</para>
    ///     <code>
    ///         var regex = new Regex(@"(?:x[yz])*");</code>
    ///     <para>
    ///         could be coded in Generex in either of the following ways:</para>
    ///     <code>
    ///         var regex = Generex.New('x').Then(Generex.New('y').Or('z')).RepeatGreedy();
    ///         var regex = Generex.New('x').Then(ch =&gt; ch == 'y' || ch == 'z').RepeatGreedy();</code>
    ///     <para>
    ///         This is of course significantly more verbose, although some programmers may find it more readable. It is
    ///         certainly easier to remember the words “look behind” than it is to remember the fairly cryptic code
    ///         <c>(?&lt;=...)</c>.</para>
    ///     <para>
    ///         However, a far more compelling advantage is that this code is entirely compiled and type-checked by the C#
    ///         compiler. There is no risk of having a runtime exception thrown due to syntax errors. You also won’t be using
    ///         string interpolation to construct such an expression, which would conceivably open up injection
    ///         vulnerabilities.</para>
    ///     <heading>
    ///         Matching, replacing, etc.</heading>
    ///     <para>
    ///         Generex has the same matching methods as Regex (<see cref="GenerexBase{T, TMatch, TGenerex,
    ///         TGenerexMatch}.Match(T[], int)"><c>Match()</c></see>, <see cref="GenerexBase{T, TMatch, TGenerex,
    ///         TGenerexMatch}.IsMatch(T[], int)"><c>IsMatch()</c></see>, <see cref="GenerexBase{T, TMatch, TGenerex,
    ///         TGenerexMatch}.Matches(T[], int)"><c>Matches()</c></see>, <see cref="GenerexBase{T, TMatch, TGenerex,
    ///         TGenerexMatch}.Replace(T[], Func{TGenerexMatch, IEnumerable{T}}, int, int?)"><c>Replace()</c></see>) but also
    ///         quite a few more. In particular, Generex allows you to match from the end of the input backwards using <see
    ///         cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.MatchReverse(T[], int?)"><c>MatchReverse()</c></see> and
    ///         <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.ReplaceReverse(T[], Func{TGenerexMatch,
    ///         IEnumerable{T}}, int?, int?)"><c>ReplaceReverse()</c></see>. It also allows you to specify that the regular
    ///         expression should match the entire string, not just a substring within it, using <see cref="GenerexBase{T,
    ///         TMatch, TGenerex, TGenerexMatch}.MatchExact(T[], int, int?)"><c>MatchExact()</c></see>.</para>
    ///     <heading>
    ///         Recursive regular expressions</heading>
    ///     <para>
    ///         Now, let’s look at a time-honoured classic of regular expression matching: The Matching Parentheses.</para>
    ///     <code>
    ///         ((()()(()))())(()(())()(()))</code>
    ///     <para>
    ///         The regular expression given on MSDN for matching these looks like this:</para>
    ///     <code>
    ///         ^(((?&lt;Open&gt;\())+((?&lt;Close-Open&gt;\)))+)*(?(Open)(?!))$</code>
    ///     <para>
    ///         It should be immediately clear that this is a kludge. This expression uses features that were shoehorned into
    ///         an existing syntax unsuitable for this task. In Generex, the parentheses matcher looks like this:</para>
    ///     <code>
    ///         var parenthesisMatcher = Generex&lt;char&gt;.Recursive(inner =&gt;
    ///             Generex.New('(')
    ///                    .Then(inner)
    ///                    .Then(')')
    ///                    .RepeatGreedy());</code>
    ///     <heading>
    ///         Result objects</heading>
    ///     <para>
    ///         Every match of a normal regular expression gives you an index and length, so you can extract the part of the
    ///         string that matched. You can also use capturing groups to get submatches inside matches. But still, the only
    ///         thing you can get out of that is a substring. You cannot use regular expressions to parse a structure and
    ///         represent that structure as an object of your own choosing.</para>
    ///     <para>
    ///         For example, let’s say you have a string with matching parentheses as before, but this time, every parenthesis
    ///         has a single-character name:</para>
    ///     <code>
    ///         (a(b(c)(d)(e(f)))(g))(h(i)(j(k))(l)(m(n)))</code>
    ///     <para>
    ///         You can use a regular expression to check that the parentheses match up, but then all you get is a single
    ///         match that matches the whole string. You’re still left with the task of figuring out how the parentheses are
    ///         nested. For example, is the <c>h</c> bracket inside the <c>b</c> bracket?</para>
    ///     <para>
    ///         We can modify the recursive Generex to match this kind of string:</para>
    ///     <code>
    ///         var parenthesisMatcher = Generex.Recursive&lt;char&gt;(inner =&gt;
    ///             Generex.New('(')
    ///                    .Then(Generex&lt;char&gt;.Any)   // matches any single character
    ///                    .Then(inner)
    ///                    .Then(')')
    ///                    .RepeatGreedy());</code>
    ///     <para>
    ///         but we can do better. Let’s declare ourselves a class that would contain information about such a parenthesis:</para>
    ///     <code>
    ///         public sealed class Parenthesis
    ///         {
    ///             public char Character;
    ///             public Parenthesis[] InnerParentheses;
    ///         }</code>
    ///     <para>
    ///         We want Generex to instantiate and populate this class for us. For this, we use the <see
    ///         cref="Generex{T}.Process{TResult}(Func{GenerexMatch{T}, TResult})"><c>Process()</c></see> method, which is
    ///         central to the whole concept of result objects in Generex. Every time there is a match (or submatch), <see
    ///         cref="Generex{T}.Process{TResult}(Func{GenerexMatch{T}, TResult})"><c>Process()</c></see> can be used to
    ///         convert that submatch into an arbitrary object, and later that object can be further processed. For example:</para>
    ///     <code>
    ///         var parenthesisMatcher = Generex.Recursive&lt;char, Parenthesis[]&gt;(inner =&gt;
    ///             Generex.New('(')
    ///                 .Then(
    ///                     Generex&lt;char&gt;.Any               // Matches any single character.
    ///                         .Process(m =&gt; m.Match[0])   // The character is now the result object.
    ///                 )
    ///                 .Then(inner,
    ///                     // ch = the character we matched earlier (the previous result object)
    ///                     (ch, innerMatch) =&gt; new Parenthesis
    ///                     {
    ///                         Character = ch,
    ///                         InnerParentheses = innerMatch.Result    // type Parenthesis[]
    ///                     }
    ///                 )                   // The result object is now a Parenthesis object.
    ///                 .Then(')')
    ///                 .RepeatGreedy()     // The result object is now IEnumerable&lt;Parenthesis&gt;.
    ///                 .Process(m =&gt; m.Result.ToArray()));   // The result object is now Parenthesis[].</code>
    ///     <para>
    ///         We can use this <c>parenthesisMatcher</c> now to match against any string and get the resulting structure. To
    ///         show that this works, let’s amend our class with a <c>ToString()</c> method that returns the structure in a
    ///         different format:</para>
    ///     <code>
    ///         public override string ToString()
    ///         {
    ///             return string.Format("[ {0} {1}]", Character,
    ///                 string.Join&lt;Parenthesis&gt;(", ", InnerParentheses));
    ///         }</code>
    ///     <para>
    ///         Now just match the input string from earlier:</para>
    ///     <code>
    ///         var input = "(a(b(c)(d)(e(f)))(g))(h(i)(j(k))(l)(m(n)))";
    ///         var match = parenthesisMatcher.Match(input.ToCharArray());
    ///         Console.WriteLine(string.Join&lt;Parenthesis&gt;(", ", match.Result));</code>
    ///     <para>
    ///         and we get the following output:</para>
    ///     <code>
    ///         [ a [ b [ c ], [ d ], [ e [ f ]]], [ g ]], [ h [ i ], [ j [ k ]], [ l ], [ m [ n ]]]</code>
    ///     <heading>
    ///         Stringerex</heading>
    ///     <para>
    ///         Even though Generex is intended to match collections of objects, matching against strings is still the most
    ///         common use-case. The above examples use <c>Generex&lt;char&gt;</c>, but this will expect the input as a
    ///         <c>char[]</c> and the match objects will return substrings as <c>char[]</c>. The class <see
    ///         cref="Stringerex"/> is intended to ease this; it will allow you to input strings and it will give you
    ///         substrings for each match.</para>
    ///     <para>
    ///         Additionally, there is a static class called <see cref="Stringerexes"/>, which has a few useful pre-defined
    ///         building-blocks. For example, there is a <see cref="Stringerexes.Integer"/> which matches any integer
    ///         (including negative) which you can just use anywhere inside a more complex Stringerex. It already has the
    ///         integer associated with it as the result object.</para>
    ///     <heading>
    ///         Additional Features</heading>
    ///     <para>
    ///         Here are a few Generex operators that do not exist in typical regular expressions. Note that each of these
    ///         links link to a specific overload and there may be many others taking other parameter types:</para>
    ///     <list type="bullet">
    ///         <item><description>
    ///             <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.And{TOtherGenerex,
    ///             TOtherGenerexMatch}(GenerexNoResultBase{T, TOtherGenerex, TOtherGenerexMatch})"><c>g.And(h)</c></see>:
    ///             Matches only if the subarray that matched <c>g</c> also contains a match for <c>h</c>.</description></item>
    ///         <item><description>
    ///             <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.AndExact{TOtherGenerex,
    ///             TOtherGenerexMatch}(GenerexNoResultBase{T, TOtherGenerex,
    ///             TOtherGenerexMatch})"><c>g.AndExact(h)</c></see>: Matches only if the subarray that matched <c>g</c> also
    ///             matches <c>h</c> exactly.</description></item>
    ///         <item><description>
    ///             <see cref="Generex{T, TResult}.RepeatWithSeparator(Generex{T})"><c>g.RepeatWithSeparator(h)</c></see>:
    ///             Equivalent to <c>g.Then(h.Then(g).Repeat())</c>.</description></item>
    ///         <item><description>
    ///             <see cref="GenerexBase{T, TMatch, TGenerex,
    ///             TGenerexMatch}.Do(Action{TGenerexMatch})"><c>g.Do(Action)</c></see>: Executes a delegate every time a
    ///             subexpression matches.</description></item>
    ///         <item><description>
    ///             <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Where(Func{TGenerexMatch,
    ///             bool})"><c>g.Where(...)</c></see>: Put an extra condition on a match.</description></item>
    ///         <item><description>
    ///             <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.ThenExpect(Func{TGenerexMatch, Exception},
    ///             T[])"><c>g.ThenExpect(h, ...)</c></see>: Every time <c>g</c> matches, <c>h</c> must match after it,
    ///             otherwise an exception is thrown. This is useful when you parse structures and wish to report invalid
    ///             input properly with index and reason (instead of just rejecting the whole input as “not a match”).</description></item>
    ///         <item><description>
    ///             <see cref="GenerexBase{T, TMatch, TGenerex, TGenerexMatch}.Throw(Func{TGenerexMatch,
    ///             Exception})"><c>g.Throw(...)</c></see>: Throw whenever <c>g</c> matches. Again, this is useful for
    ///             generating user-friendly errors in parsers.</description></item></list></summary>
    sealed class NamespaceDocumentation
    {
    }
}
