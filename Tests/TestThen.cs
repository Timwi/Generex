﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RT.Generexes.Tests
{
    [TestFixture]
    class TestThen : TestBase
    {
        Generex<int> _g = new Generex<int>(47);
        Generex<int, int> _gr = new Generex<int>(47).Process(_ => 1);
        Stringerex _s = new Stringerex('M');
        Stringerex<int> _sr = new Stringerex('M').Process(_ => 1);

        int[] _input = new int[] { 47, 24567837, 1701 };

        [Test]
        public void TestThenElements()
        {
            Assert.Throws<ArgumentNullException>(() => { _g.Then(elements: (int[]) null); });
            Assert.Throws<ArgumentNullException>(() => { _g.Then(elements: (IEnumerable<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { _g.Then(comparer: null, elements: new int[] { }); });
            Assert.Throws<ArgumentNullException>(() => { _g.Then(comparer: null, elements: (IEnumerable<int>) new int[] { }); });
            Assert.Throws<ArgumentNullException>(() => { _g.Then(comparer: new Mod7IntEqualityComparer(), elements: (int[]) null); });
            Assert.Throws<ArgumentNullException>(() => { _g.Then(comparer: new Mod7IntEqualityComparer(), elements: (IEnumerable<int>) null); });

            Assert.Throws<ArgumentNullException>(() => { _gr.Then(elements: (int[]) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then(elements: (IEnumerable<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then(comparer: null, elements: new int[] { }); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then(comparer: null, elements: (IEnumerable<int>) new int[] { }); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then(comparer: new Mod7IntEqualityComparer(), elements: (int[]) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then(comparer: new Mod7IntEqualityComparer(), elements: (IEnumerable<int>) null); });

            AssertMatches(_g.Then(24567837, 1701), _input, True, False, False, True, True, new object[] { 0, 3 }, new object[] { 0, 3 }, new object[] { 0, 3 }, 1, 1);
            AssertMatches(_g.Then((IEnumerable<int>) new[] { 24567837, 1701 }), _input, True, False, False, True, True, new object[] { 0, 3 }, new object[] { 0, 3 }, new object[] { 0, 3 }, 1, 1);
            AssertMatches(_g.Then(new Mod7IntEqualityComparer(), 0, 0), _input, True, False, False, True, True, new object[] { 0, 3 }, new object[] { 0, 3 }, new object[] { 0, 3 }, 1, 1);
            AssertMatches(_g.Then(new Mod7IntEqualityComparer(), (IEnumerable<int>) new[] { 0, 0 }), _input, True, False, False, True, True, new object[] { 0, 3 }, new object[] { 0, 3 }, new object[] { 0, 3 }, 1, 1);

            AssertMatches(_gr.Then(24567837, 1701), _input, True, False, False, True, True, new object[] { 0, 3, 1 }, new object[] { 0, 3, 1 }, new object[] { 0, 3, 1 }, 1, 1);
            AssertMatches(_gr.Then((IEnumerable<int>) new[] { 24567837, 1701 }), _input, True, False, False, True, True, new object[] { 0, 3, 1 }, new object[] { 0, 3, 1 }, new object[] { 0, 3, 1 }, 1, 1);
            AssertMatches(_gr.Then(new Mod7IntEqualityComparer(), 0, 0), _input, True, False, False, True, True, new object[] { 0, 3, 1 }, new object[] { 0, 3, 1 }, new object[] { 0, 3, 1 }, 1, 1);
            AssertMatches(_gr.Then(new Mod7IntEqualityComparer(), (IEnumerable<int>) new[] { 0, 0 }), _input, True, False, False, True, True, new object[] { 0, 3, 1 }, new object[] { 0, 3, 1 }, new object[] { 0, 3, 1 }, 1, 1);
        }

        [Test]
        public void TestThenPredicate()
        {
            Assert.Throws<ArgumentNullException>(() => { _g.Then(predicate: null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then(predicate: null); });

            AssertMatches(_g.Then(i => i % 7 == 0), _input, True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(_gr.Then(i => i % 7 == 0), _input, True, False, True, False, True, new object[] { 0, 2, 1 }, null, new object[] { 0, 2, 1 }, 1, 1);

            Assert.Throws<ArgumentNullException>(() => { _s.Then(predicate: null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Then(predicate: null); });

            AssertMatches(_s.Then(ch => ch == 'L'), "MLP", True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(_sr.Then(ch => ch == 'L'), "MLP", True, False, True, False, True, new object[] { 0, 2, 1 }, null, new object[] { 0, 2, 1 }, 1, 1);
        }

        [Test]
        public void TestThenGenerexNoResult()
        {
            Assert.Throws<ArgumentNullException>(() => { _g.Then((Generex<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { _g.Then((Generex<int>[]) null); });
            Assert.Throws<ArgumentNullException>(() => { _g.Then((IEnumerable<Generex<int>>) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then((Generex<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then((Generex<int>[]) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then((IEnumerable<Generex<int>>) null); });

            AssertMatches(_g.Then(new Generex<int>(i => i % 7 == 0)), _input, True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(_gr.Then(new Generex<int>(i => i % 7 == 0)), _input, True, False, True, False, True, new object[] { 0, 2, 1 }, null, new object[] { 0, 2, 1 }, 1, 1);
            AssertMatches(_g.Then((IEnumerable<Generex<int>>) new[] { new Generex<int>(i => i % 7 == 0) }), _input, True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(_gr.Then((IEnumerable<Generex<int>>) new[] { new Generex<int>(i => i % 7 == 0) }), _input, True, False, True, False, True, new object[] { 0, 2, 1 }, null, new object[] { 0, 2, 1 }, 1, 1);

            Assert.Throws<ArgumentNullException>(() => { _s.Then((Stringerex) null); });
            Assert.Throws<ArgumentNullException>(() => { _s.Then((Stringerex[]) null); });
            Assert.Throws<ArgumentNullException>(() => { _s.Then((IEnumerable<Stringerex>) null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Then((Stringerex) null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Then((Stringerex[]) null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Then((IEnumerable<Stringerex>) null); });

            AssertMatches(_s.Then(new Stringerex('L')), "MLP", True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(_sr.Then(new Stringerex('L')), "MLP", True, False, True, False, True, new object[] { 0, 2, 1 }, null, new object[] { 0, 2, 1 }, 1, 1);
            AssertMatches(_s.Then((IEnumerable<Stringerex>) new[] { new Stringerex('L') }), "MLP", True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(_sr.Then((IEnumerable<Stringerex>) new[] { new Stringerex('L') }), "MLP", True, False, True, False, True, new object[] { 0, 2, 1 }, null, new object[] { 0, 2, 1 }, 1, 1);
        }

        [Test]
        public void TestThenGenerexWithResult()
        {
            Assert.Throws<ArgumentNullException>(() => { _g.Then(other: (Generex<int, int>) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then(other: (Generex<int, int>) null, selector: (r1, m2) => r1 + m2.Result); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then<int, string>(other: new Generex<int, int>(0), selector: null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.ThenRaw(other: (Generex<int, int>) null, selector: (r1, r2) => r1 + r2); });
            Assert.Throws<ArgumentNullException>(() => { _gr.ThenRaw<int, string>(other: new Generex<int, int>(0), selector: null); });

            AssertMatches(_g.Then(new Generex<int>(i => i % 7 == 0).Process(m => 2)), _input, True, False, True, False, True, new object[] { 0, 2, 2 }, null, new object[] { 0, 2, 2 }, 1, 1);
            AssertMatches(_gr.Then(new Generex<int>(i => i % 7 == 0).Process(m => 2), (r1, m2) => r1 + m2.Result), _input, True, False, True, False, True, new object[] { 0, 2, 3 }, null, new object[] { 0, 2, 3 }, 1, 1);
            AssertMatches(_gr.ThenRaw(new Generex<int>(i => i % 7 == 0).Process(m => 2), (r1, r2) => r1 + r2), _input, True, False, True, False, True, new object[] { 0, 2, 3 }, null, new object[] { 0, 2, 3 }, 1, 1);

            Assert.Throws<ArgumentNullException>(() => { _s.Then(other: (Stringerex<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Then(other: (Stringerex<int>) null, selector: (r1, m2) => r1 + m2.Result); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Then<int, string>(other: new Stringerex<int>(0), selector: null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.ThenRaw(other: (Stringerex<int>) null, selector: (r1, r2) => r1 + r2); });
            Assert.Throws<ArgumentNullException>(() => { _sr.ThenRaw<int, string>(other: new Stringerex<int>(0), selector: null); });

            AssertMatches(_s.Then(new Stringerex('L').Process(m => 2)), "MLP", True, False, True, False, True, new object[] { 0, 2, 2 }, null, new object[] { 0, 2, 2 }, 1, 1);
            AssertMatches(_sr.Then(new Stringerex('L').Process(m => 2), (r1, m2) => r1 + m2.Result), "MLP", True, False, True, False, True, new object[] { 0, 2, 3 }, null, new object[] { 0, 2, 3 }, 1, 1);
            AssertMatches(_sr.ThenRaw(new Stringerex('L').Process(m => 2), (r1, r2) => r1 + r2), "MLP", True, False, True, False, True, new object[] { 0, 2, 3 }, null, new object[] { 0, 2, 3 }, 1, 1);
        }

        [Test]
        public void TestThenBindGenerexNoResult()
        {
            Assert.Throws<ArgumentNullException>(() => { _g.Then((Func<GenerexMatch<int>, Generex<int>>) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then((Func<GenerexMatch<int, int>, Generex<int>>) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.ThenRaw((Func<int, Generex<int>>) null); });

            AssertMatches(_g.Then(m => new Generex<int>(i => i % 7 == 0)), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2 }, null, _expectInvOp, 1, -1);
            AssertMatches(_gr.Then(m => new Generex<int>(i => i % 7 == 0)), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, -1);
            AssertMatches(_gr.ThenRaw(m => new Generex<int>(i => i % 7 == 0)), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, -1);

            Assert.Throws<ArgumentNullException>(() => { _s.Then((Func<StringerexMatch, Stringerex>) null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Then((Func<StringerexMatch<int>, Stringerex>) null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.ThenRaw((Func<int, Stringerex>) null); });

            AssertMatches(_s.Then(m => new Stringerex('L')), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2 }, null, _expectInvOp, 1, -1);
            AssertMatches(_sr.Then(m => new Stringerex('L')), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, -1);
            AssertMatches(_sr.ThenRaw(m => new Stringerex('L')), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, -1);
        }

        [Test]
        public void TestThenBindGenerexWithResult()
        {
            Assert.Throws<ArgumentNullException>(() => { _g.Then((Func<GenerexMatch<int>, Generex<int, int>>) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Then((Func<GenerexMatch<int, int>, Generex<int, string>>) null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.ThenRaw((Func<int, Generex<int, string>>) null); });

            AssertMatches(_g.Then(m => new Generex<int>(i => i % 7 == 0).Process(_ => 2)), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 2 }, null, _expectInvOp, 1, -1);
            AssertMatches(_gr.Then(m => new Generex<int>(i => i % 7 == 0).Process(_ => 2 + m.Result)), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 3 }, null, _expectInvOp, 1, -1);
            AssertMatches(_gr.ThenRaw(m => new Generex<int>(i => i % 7 == 0).Process(_ => 2 + m)), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 3 }, null, _expectInvOp, 1, -1);

            Assert.Throws<ArgumentNullException>(() => { _s.Then((Func<StringerexMatch, Stringerex<int>>) null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.Then((Func<StringerexMatch<int>, Stringerex<string>>) null); });
            Assert.Throws<ArgumentNullException>(() => { _sr.ThenRaw((Func<int, Stringerex<string>>) null); });

            AssertMatches(_s.Then(m => new Stringerex('L').Process(_ => 2)), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 2 }, null, _expectInvOp, 1, -1);
            AssertMatches(_sr.Then(m => new Stringerex('L').Process(_ => 2 + m.Result)), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 3 }, null, _expectInvOp, 1, -1);
            AssertMatches(_sr.ThenRaw(m => new Stringerex('L').Process(_ => 2 + m)), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 3 }, null, _expectInvOp, 1, -1);
        }
    }
}