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
    class TestOr : TestBase
    {
        [Test]
        public void TestStaticOrs()
        {
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((Generex<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((Generex<int>[]) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((IEnumerable<Generex<int>>) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((Generex<int, int>) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((Generex<int, int>[]) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((IEnumerable<Generex<int, int>>) null); });
            AssertMatches(Generex.Ors(_g, _gn), _input, True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(Generex.Ors((IEnumerable<Generex<int>>) new[] { _g, _gn }), _input, True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(Generex.Ors(_gr, _gnr), _input, True, False, False, False, True, new object[] { 0, 1, 1 }, null, new object[] { 0, 1, 1 }, 1, 1);
            AssertMatches(Generex.Ors((IEnumerable<Generex<int, int>>) new[] { _gr, _gnr }), _input, True, False, False, False, True, new object[] { 0, 1, 1 }, null, new object[] { 0, 1, 1 }, 1, 1);

            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((Stringerex) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((Stringerex[]) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((IEnumerable<Stringerex>) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((Stringerex<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((Stringerex<int>[]) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((IEnumerable<Stringerex<int>>) null); });
            AssertMatches(Generex.Ors(_s, _sn), "MLP", True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(Generex.Ors((IEnumerable<Stringerex>) new[] { _s, _sn }), "MLP", True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(Generex.Ors(_sr, _snr), "MLP", True, False, False, False, True, new object[] { 0, 1, 1 }, null, new object[] { 0, 1, 1 }, 1, 1);
            AssertMatches(Generex.Ors((IEnumerable<Stringerex<int>>) new[] { _sr, _snr }), "MLP", True, False, False, False, True, new object[] { 0, 1, 1 }, null, new object[] { 0, 1, 1 }, 1, 1);

            Assert.Throws<ArgumentNullException>(() => { Stringerex.Ors((Stringerex) null); });
            Assert.Throws<ArgumentNullException>(() => { Stringerex.Ors((Stringerex[]) null); });
            Assert.Throws<ArgumentNullException>(() => { Stringerex.Ors((IEnumerable<Stringerex>) null); });
            Assert.Throws<ArgumentNullException>(() => { Stringerex<int>.Ors((Stringerex<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { Stringerex<int>.Ors((Stringerex<int>[]) null); });
            Assert.Throws<ArgumentNullException>(() => { Stringerex<int>.Ors((IEnumerable<Stringerex<int>>) null); });
            AssertMatches(Stringerex.Ors(_s, _sn), "MLP", True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(Stringerex.Ors((IEnumerable<Stringerex>) new[] { _s, _sn }), "MLP", True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(Stringerex<int>.Ors(_sr, _snr), "MLP", True, False, False, False, True, new object[] { 0, 1, 1 }, null, new object[] { 0, 1, 1 }, 1, 1);
            AssertMatches(Stringerex<int>.Ors((IEnumerable<Stringerex<int>>) new[] { _sr, _snr }), "MLP", True, False, False, False, True, new object[] { 0, 1, 1 }, null, new object[] { 0, 1, 1 }, 1, 1);
        }

        [Test]
        public void TestOrDefault()
        {
            // OrDefault, OrDefaultGreedy, OrNull, OrNullGreedy
            Assert.Throws<ArgumentNullException>(() => { Generex.OrDefault<int, int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrDefaultGreedy<int, int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrNull<int, int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrNullGreedy<int, int>(inner: null); });

            Assert.Throws<ArgumentNullException>(() => { Generex.OrDefault<int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrDefaultGreedy<int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrNull<int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrNullGreedy<int>(inner: null); });

            AssertMatches(_gr.OrDefault(), _input, True, True, True, False, True, new object[] { 0, 0, 0 }, null, new object[] { 3, 0, 0 }, 4, 4);
            AssertMatches(_gr.OrDefaultGreedy(), _input, True, True, True, False, True, new object[] { 0, 1, 1 }, null, new object[] { 3, 0, 0 }, 4, 4);
            AssertMatches(_gr.OrNull(), _input, True, True, True, False, True, new object[] { 0, 0, null }, null, new object[] { 3, 0, null }, 4, 4);
            AssertMatches(_gr.OrNullGreedy(), _input, True, True, True, False, True, new object[] { 0, 1, 1 }, null, new object[] { 3, 0, null }, 4, 4);

            AssertMatches(_sr.OrDefault(), "MLP", True, True, True, False, True, new object[] { 0, 0, 0 }, null, new object[] { 3, 0, 0 }, 4, 4);
            AssertMatches(_sr.OrDefaultGreedy(), "MLP", True, True, True, False, True, new object[] { 0, 1, 1 }, null, new object[] { 3, 0, 0 }, 4, 4);
            AssertMatches(_sr.OrNull(), "MLP", True, True, True, False, True, new object[] { 0, 0, null }, null, new object[] { 3, 0, null }, 4, 4);
            AssertMatches(_sr.OrNullGreedy(), "MLP", True, True, True, False, True, new object[] { 0, 1, 1 }, null, new object[] { 3, 0, null }, 4, 4);
        }

        [Test]
        public void TestOrElement()
        {
            Assert.Throws<ArgumentNullException>(() => { _g.Or(47, (IEqualityComparer<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { _g.Or((IEqualityComparer<int>) null, (IEnumerable<int>) new[] { 1 }); });
            Assert.Throws<ArgumentNullException>(() => { _g.Or(_mod7, (IEnumerable<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { _g.Or((IEqualityComparer<int>) null, new int[] { 1 }); });
            Assert.Throws<ArgumentNullException>(() => { _g.Or(_mod7, (int[]) null); });

            AssertMatches(_gn.Or(47), _input, True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(_gn.Or(5, _mod7), _input, True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(_gn.Or(47, 24567837), _input, True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(_gn.Or(_mod7, 5, 0), _input, True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(_gn.Or((IEnumerable<int>) new[] { 47, 24567837 }), _input, True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(_gn.Or(_mod7, (IEnumerable<int>) new[] { 5, 0 }), _input, True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);

            Assert.Throws<ArgumentNullException>(() => { _s.Or('M', (IEqualityComparer<char>) null); });
            Assert.Throws<ArgumentNullException>(() => { _s.Or((IEqualityComparer<char>) null, (IEnumerable<char>) new[] { 'Q' }); });
            Assert.Throws<ArgumentNullException>(() => { _s.Or(_ci, (IEnumerable<char>) null); });
            Assert.Throws<ArgumentNullException>(() => { _s.Or((IEqualityComparer<char>) null, new char[] { 'Q' }); });
            Assert.Throws<ArgumentNullException>(() => { _s.Or(_ci, (char[]) null); });

            AssertMatches(_sn.Or('M'), "MLP", True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(_sn.Or('m', _ci), "MLP", True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(_sn.Or('M', 'L'), "MLP", True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(_sn.Or(_ci, 'm', 'l'), "MLP", True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(_sn.Or((IEnumerable<char>) new[] { 'M', 'L' }), "MLP", True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(_sn.Or(_ci, (IEnumerable<char>) new[] { 'm', 'l' }), "MLP", True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);

            // TODO: Proper tests for these overloads
            Assert.Throws<ArgumentNullException>(() => { _gr.Or(47, (Func<GenerexMatch<int>, int>) null, (IEqualityComparer<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Or((Func<GenerexMatch<int>, int>) null, _mod7, (IEnumerable<int>) new[] { 1 }); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Or(m => m.Index, (IEqualityComparer<int>) null, (IEnumerable<int>) new[] { 1 }); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Or(m => m.Index, _mod7, (IEnumerable<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Or((Func<GenerexMatch<int>, int>) null, _mod7, (int[]) new[] { 1 }); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Or(m => m.Index, (IEqualityComparer<int>) null, (int[]) new[] { 1 }); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Or(m => m.Index, _mod7, (int[]) null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Or('M', (Func<StringerexMatch, int>) null, (IEqualityComparer<char>) null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Or("Q", (Func<StringerexMatch, int>) null, _ci); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Or("Q", m => m.Index, (IEqualityComparer<char>) null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Or((string) null, m => m.Index, _ci); });
        }

        [Test]
        public void TestOrPredicate()
        {
            Assert.Throws<ArgumentNullException>(() => { _g.Or((Predicate<int>) null); });

            AssertMatches(_gn.Or(i => i % 7 == 5), _input, True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(_gn.Or(i => i % 7 == 4), _input, False, False, False, False, False, null, null, null, 0, 0);

            Assert.Throws<ArgumentNullException>(() => { _s.Or((Predicate<char>) null); });

            AssertMatches(_sn.Or(ch => ch == 'M'), "MLP", True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(_sn.Or(ch => ch == 'Q'), "MLP", False, False, False, False, False, null, null, null, 0, 0);
        }

        [Test]
        public void TestOrGenerex()
        {
            Assert.Throws<ArgumentNullException>(() => { _g.Or((Generex<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Or((Generex<int, int>) null); });

            AssertMatches(_g.Or(_gn), _input, True, False, False, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(_gr.Or(_gnr), _input, True, False, False, False, True, new object[] { 0, 1, 1 }, null, new object[] { 0, 1, 1 }, 1, 1);
        }
    }
}
