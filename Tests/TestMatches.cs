using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RT.Generexes.Tests
{
    [TestFixture]
    sealed class TestMatches : TestBase
    {
        [Test]
        public void TestMatchEtc()
        {
            // Most of the functionality of .Match(), .Matches() etc. is already tested by the other tests which call .AssertMatches().
            // This is only for the null checks.
            Assert.Throws<ArgumentNullException>(() => { _g.IsMatch(null); });
            Assert.Throws<ArgumentNullException>(() => { _g.IsMatchAt(null); });
            Assert.Throws<ArgumentNullException>(() => { _g.IsMatchExact(null); });
            Assert.Throws<ArgumentNullException>(() => { _g.IsMatchReverse(null); });
            Assert.Throws<ArgumentNullException>(() => { _g.IsMatchUpTo(null); });
            Assert.Throws<ArgumentNullException>(() => { _g.Match(null); });
            Assert.Throws<ArgumentNullException>(() => { _g.Matches(null); });
            Assert.Throws<ArgumentNullException>(() => { _g.MatchesReverse(null); });
            Assert.Throws<ArgumentNullException>(() => { _g.MatchExact(null); });
            Assert.Throws<ArgumentNullException>(() => { _g.MatchReverse(null); });

            Assert.Throws<ArgumentNullException>(() => { _gr.IsMatch(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.IsMatchAt(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.IsMatchExact(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.IsMatchReverse(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.IsMatchUpTo(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Match(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Matches(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.MatchesReverse(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.MatchExact(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.MatchReverse(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.RawMatch(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.RawMatches(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.RawMatchesReverse(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.RawMatchExact(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.RawMatchReverse(null); });

            Assert.Throws<ArgumentNullException>(() => { _s.IsMatch(null); });
            Assert.Throws<ArgumentNullException>(() => { _s.IsMatchAt(null); });
            Assert.Throws<ArgumentNullException>(() => { _s.IsMatchExact(null); });
            Assert.Throws<ArgumentNullException>(() => { _s.IsMatchReverse(null); });
            Assert.Throws<ArgumentNullException>(() => { _s.IsMatchUpTo(null); });
            Assert.Throws<ArgumentNullException>(() => { _s.Match(null); });
            Assert.Throws<ArgumentNullException>(() => { _s.Matches(null); });
            Assert.Throws<ArgumentNullException>(() => { _s.MatchesReverse(null); });
            Assert.Throws<ArgumentNullException>(() => { _s.MatchExact(null); });
            Assert.Throws<ArgumentNullException>(() => { _s.MatchReverse(null); });

            Assert.Throws<ArgumentNullException>(() => { _sr.IsMatch(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.IsMatchAt(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.IsMatchExact(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.IsMatchReverse(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.IsMatchUpTo(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Match(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Matches(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.MatchesReverse(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.MatchExact(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.MatchReverse(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.RawMatch(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.RawMatches(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.RawMatchesReverse(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.RawMatchExact(null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.RawMatchReverse(null); });
        }
    }
}
