Full documentation
=

Full documentation: https://docs.timwi.de/RT.Generex/RT.Generexes

Flexible Regular Expressions
=

The goal of Generex is to allow the same kind of pattern matching, analysis, parsing and searching as typical regular expression engines, but instead of limiting itself to matching characters in a string, it can match against any array of objects.

For an example, imagine you have a list of log entries, and you need to find clusters of at least three consecutive log entries from the same day:

    16/06/2013 15:15:50
    17/06/2013 02:09:09     ┐
    17/06/2013 21:00:18     ├ 3 entries here
    17/06/2013 23:44:28     ┘
    18/06/2013 09:39:34
    19/06/2013 08:30:36     ┐
    19/06/2013 09:28:52     ├ 3 entries here
    19/06/2013 14:34:12     ┘
    20/06/2013 00:07:58
    20/06/2013 01:54:10
    22/06/2013 07:31:00
    23/06/2013 03:35:56     ┐
    23/06/2013 10:06:10     ├ 4 entries here
    23/06/2013 12:05:59     │
    23/06/2013 23:43:35     ┘
    24/06/2013 19:46:16
    25/06/2013 17:42:17
    26/06/2013 11:29:37


This *sounds* like the typical kind of pattern matching that regular expressions are made for, but instead of a `string` or `char[]`, you have a `DateTime[]` (or maybe a `LogEntry[]`, where `LogEntry` is your own class).

Generex uses the power of generics to bring you regular expression functionality on any type. As a trade-off, instead of an extremely terse text-based syntax, it uses method calls to construct the regular expression. Here are only a few basic examples:

      Regex  │  Generex
    ═════════╪════════════════════
          a  │  Generex.New('a')
         ab  │  a.Then(b)
        a|b  │  a.Or(b)
        a*?  │  a.Repeat()
         a*  │  a.RepeatGreedy()
      (?=a)  │  a.LookAhead()
     (?<=a)  │  a.LookBehind()
      (?>a)  │  a.Atomic()

For example, a regular expression such as the following:

    var regex = new Regex(@"(?:x[yz])*");

could be coded in Generex in either of the following ways:

    var regex = Generex.New('x').Then(Generex.New('y').Or('z')).RepeatGreedy();
    var regex = Generex.New('x').Then(ch => ch == 'y' || ch == 'z').RepeatGreedy();

This is of course significantly more verbose, although some programmers may find it more readable. It is certainly easier to remember the words “look behind” than it is to remember the fairly cryptic code `(?<=...)`.

However, a far more compelling advantage is that this code is entirely compiled and type-checked by the C# compiler. There is no risk of having a runtime exception thrown due to syntax errors. You also won’t be using string interpolation to construct such an expression, which would conceivably open up injection vulnerabilities.

Matching, replacing, etc.
-

Generex has the same matching methods as Regex (`.Match()`, `.IsMatch()`, `.Matches()`, `.Replace()`) but also quite a few more. In particular, Generex allows you to match from the end of the input backwards using `.MatchReverse()` and `.ReplaceReverse()`. It also allows you to specify that the regular expression should match the entire string, not just a substring within it, using `.MatchExact()`.

Recursive regular expressions
-

Now, let’s look at a time-honoured classic of regular expression matching: The Matching Parentheses.

    ((()()(()))())(()(())()(()))

The regular expression given on MSDN for matching these looks like this:

    ^(((?<Open>\())+((?<Close-Open>\)))+)*(?(Open)(?!))$

It should be immediately clear that this is a kludge. This expression uses features that were shoehorned into an existing syntax unsuitable for this task. In Generex, the parentheses matcher looks like this:

    var parenthesisMatcher = Generex<char>.Recursive(inner =>
        Generex.New('(')
               .Then(inner)
               .Then(')')
               .RepeatGreedy());

Result objects
-

Every match of a normal regular expression gives you an index and length, so you can extract the part of the string that matched. You can also use capturing groups to get submatches inside matches. But still, the only thing you can get out of that is a substring. You cannot use regular expressions to parse a structure and represent that structure as an object of your own choosing.

For example, let’s say you have a string with matching parentheses as before, but this time, every parenthesis has a single-character name:

    (a(b(c)(d)(e(f)))(g))(h(i)(j(k))(l)(m(n)))

You can use a regular expression to check that the parentheses match up, but then all you get is a single match that matches the whole string. You’re still left with the task of figuring out how the parentheses are nested. For example, is the `h` bracket inside the `b` bracket?

We can modify the recursive Generex to match this kind of string:

    var parenthesisMatcher = Generex.Recursive<char>(inner =>
        Generex.New('(')
               .Then(Generex<char>.Any)   // matches any single character
               .Then(inner)
               .Then(')')
               .RepeatGreedy());

but we can do better. Let’s declare ourselves a class that would contain information about such a parenthesis:

    public sealed class Parenthesis
    {
        public char Character;
        public Parenthesis[] InnerParentheses;
    }

We want Generex to instantiate and populate this class for us. For this, we use the `.Process()` method, which is central to the whole concept of result objects in Generex. Every time there is a match (or submatch), `.Process()` can be used to convert that submatch into an arbitrary object, and later that object can be further processed. For example:

    var parenthesisMatcher = Generex.Recursive<char, Parenthesis[]>(inner =>
        Generex.New('(')
            .Then(
                Generex<char>.Any               // Matches any single character.
                    .Process(m => m.Match[0])   // The character is now the result object.
            )
            .Then(inner,
                // ch = the character we matched earlier (the previous result object)
                (ch, innerMatch) => new Parenthesis
                {
                    Character = ch,
                    InnerParentheses = innerMatch.Result    // type Parenthesis[]
                }
            )                   // The result object is now a Parenthesis object.
            .Then(')')
            .RepeatGreedy()     // The result object is now IEnumerable<Parenthesis>.
            .Process(m => m.Result.ToArray()));   // The result object is now Parenthesis[].

We can use this `parenthesisMatcher` now to match against any string and get the resulting structure. To show that this works, let’s amend our class with a ToString() method that returns the structure in a different format:

        public override string ToString()
        {
            return string.Format("[ {0} {1}]", Character,
                string.Join<Parenthesis>(", ", InnerParentheses));
        }

Now just match the input string from earlier:

    var input = "(a(b(c)(d)(e(f)))(g))(h(i)(j(k))(l)(m(n)))";
    var match = parenthesisMatcher.Match(input.ToCharArray());
    Console.WriteLine(string.Join<Parenthesis>(", ", match.Result));

and we get the following output:

    [ a [ b [ c ], [ d ], [ e [ f ]]], [ g ]], [ h [ i ], [ j [ k ]], [ l ], [ m [ n ]]]

Stringerex
-

Even though Generex is intended to match collections of objects, matching against strings is still the most common use-case. The above examples use `Generex<char>`, but this will expect the input as a `char[]` and the match objects will return substrings as `char[]`. The class `Stringerex` is intended to ease this; it will allow you to input strings and it will give you substrings for each match.

Additionally, there is a static class called `Stringerexes`, which has a few useful pre-defined building-blocks. For example, there is a `Stringerexes.Integer` which matches any integer (including negative) which you can just use anywhere inside a more complex Stringerex. It already has the integer associated with it as the result object.

Additional Features
-

Here are a few Generex operators that do not exist in typical regular expressions:

* `a.And(b)`: Matches only if the subarray that matched `a` also contains a match for `b`.
* `a.AndExact(b)`: Matches only if the subarray that matched `a` also matches `b` exactly.
* `a.RepeatWithSeparator(b)`: Equivalent to `a.Then(b.Then(a).Repeat())`.
* `a.Do(Action)`: Executes a delegate every time a subexpression matches.
* `a.Where(...)`: Put an extra condition on a match.
* `a.ThenExpect(b, ...)`: Every time `a` matches, `b` must match after it, otherwise an exception is thrown. This is useful when you parse structures and wish to report invalid input properly with index and reason (instead of just rejecting the whole input as “not a match”).
* `a.Throw(...)`: Throw whenever `a` matches. Again, this is useful for generating user-friendly errors in parsers.

Full documentation
=

Full documentation: https://docs.timwi.de/RT.Generex/RT.Generexes
