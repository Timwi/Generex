using NUnit.Framework;

namespace RT.Generexes.Tests
{
    [TestFixture]
    class TestThenExpect : TestBase
    {
        private readonly Func<GenerexMatch<int>, Exception> _excGen = m => new ExpectedException();
        private readonly Func<GenerexMatch<char>, Exception> _excGenS = m => new ExpectedException();
        private readonly Func<int, Exception> _excGenRaw = m => new ExpectedException();

        [Test]
        public void TestThenExpectElements()
        {
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect(_excGen, elements: (int[]) null));
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect(_excGen, elements: (IEnumerable<int>) null));
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect(_excGen, comparer: null, elements: new int[] { }));
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect(_excGen, comparer: null, elements: (IEnumerable<int>) new int[] { }));
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect(_excGen, comparer: _mod7, elements: (int[]) null));
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect(_excGen, comparer: _mod7, elements: (IEnumerable<int>) null));

            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect(_excGen, elements: (int[]) null));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect(_excGen, elements: (IEnumerable<int>) null));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect(_excGen, comparer: null, elements: new int[] { }));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect(_excGen, comparer: null, elements: (IEnumerable<int>) new int[] { }));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect(_excGen, comparer: _mod7, elements: (int[]) null));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect(_excGen, comparer: _mod7, elements: (IEnumerable<int>) null));

            AssertMatches(_g.ThenExpect(_excGen, 24567837, 1701), _input, True, False, InvOp, True, InvOp, new object[] { 0, 3 }, new object[] { 0, 3 }, _expectInvOp, 1, _expInvOp);
            AssertMatches(_g.ThenExpect(_excGen, (IEnumerable<int>) new[] { 24567837, 1701 }), _input, True, False, InvOp, True, InvOp, new object[] { 0, 3 }, new object[] { 0, 3 }, _expectInvOp, 1, _expInvOp);
            AssertMatches(_g.ThenExpect(_excGen, _mod7, 0, 0), _input, True, False, InvOp, True, InvOp, new object[] { 0, 3 }, new object[] { 0, 3 }, _expectInvOp, 1, _expInvOp);
            AssertMatches(_g.ThenExpect(_excGen, _mod7, (IEnumerable<int>) new[] { 0, 0 }), _input, True, False, InvOp, True, InvOp, new object[] { 0, 3 }, new object[] { 0, 3 }, _expectInvOp, 1, _expInvOp);

            AssertMatches(_gr.ThenExpect(_excGen, 24567837, 1701), _input, True, False, InvOp, True, InvOp, new object[] { 0, 3, 1 }, new object[] { 0, 3, 1 }, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpect(_excGen, (IEnumerable<int>) new[] { 24567837, 1701 }), _input, True, False, InvOp, True, InvOp, new object[] { 0, 3, 1 }, new object[] { 0, 3, 1 }, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpect(_excGen, _mod7, 0, 0), _input, True, False, InvOp, True, InvOp, new object[] { 0, 3, 1 }, new object[] { 0, 3, 1 }, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpect(_excGen, _mod7, (IEnumerable<int>) new[] { 0, 0 }), _input, True, False, InvOp, True, InvOp, new object[] { 0, 3, 1 }, new object[] { 0, 3, 1 }, _expectInvOp, 1, _expInvOp);
        }

        [Test]
        public void TestThenExpectPredicate()
        {
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect(predicate: null, exceptionGenerator: _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect(predicate: null, exceptionGenerator: _excGen));

            // Met expectations
            AssertMatches(_g.ThenExpect(i => i % 7 == 0, _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpect(i => i % 7 == 0, _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, _expInvOp);
            // Unmet expectations
            AssertMatches(_g.ThenExpect(i => i % 7 == 1, _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_gr.ThenExpect(i => i % 7 == 1, _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);

            Assert.Throws<ArgumentNullException>(() => _s.ThenExpect(predicate: null, exceptionGenerator: _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpect(predicate: null, exceptionGenerator: _excGenS));

            // Met expectations
            AssertMatches(_s.ThenExpect(ch => ch == 'L', _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_sr.ThenExpect(ch => ch == 'L', _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, _expInvOp);
            // Unmet expectations
            AssertMatches(_s.ThenExpect(ch => ch == 'Q', _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_sr.ThenExpect(ch => ch == 'Q', _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
        }

        [Test]
        public void TestThenExpectGenerexNoResult()
        {
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect((Generex<int>) null, _excGen));
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect(_excGen, (Generex<int>[]) null));
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect((IEnumerable<Generex<int>>) null, _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect((Generex<int>) null, _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect(new Generex<int>(1701), (Func<int, GenerexMatch<int>, string>) null, _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect((Generex<int>) null, (m1, m2) => (m1 + m2.Length).ToString(), _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect(_excGen, (Generex<int>[]) null));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect((IEnumerable<Generex<int>>) null, _excGen));

            // Met expectations
            AssertMatches(_g.ThenExpect(new Generex<int>(i => i % 7 == 0), _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_g.ThenExpect(_excGen, new Generex<int>(i => i % 7 == 0)), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_g.ThenExpect((IEnumerable<Generex<int>>) new[] { new Generex<int>(i => i % 7 == 0) }, _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2 }, null, _expectInvOp, 1, _expInvOp);

            AssertMatches(_gr.ThenExpect(new Generex<int>(i => i % 7 == 0), _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpect(_excGen, new Generex<int>(i => i % 7 == 0)), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpect(new Generex<int>(i => i % 7 == 0), (m1, m2) => m1 + m2.Index, _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 2 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpect((IEnumerable<Generex<int>>) new[] { new Generex<int>(i => i % 7 == 0) }, _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, _expInvOp);

            // Unmet expectations
            AssertMatches(_g.ThenExpect(new Generex<int>(i => i % 7 == 1), _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_g.ThenExpect(_excGen, new Generex<int>(i => i % 7 == 1)), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_g.ThenExpect((IEnumerable<Generex<int>>) new[] { new Generex<int>(i => i % 7 == 1) }, _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);

            AssertMatches(_gr.ThenExpect(new Generex<int>(i => i % 7 == 1), _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_gr.ThenExpect(_excGen, new Generex<int>(i => i % 7 == 1)), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_gr.ThenExpect(new Generex<int>(i => i % 7 == 1), (m1, m2) => m1 + m2.Index, _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_gr.ThenExpect((IEnumerable<Generex<int>>) new[] { new Generex<int>(i => i % 7 == 1) }, _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);

            Assert.Throws<ArgumentNullException>(() => _s.ThenExpect((Stringerex) null, _excGenS));
            Assert.Throws<ArgumentNullException>(() => _s.ThenExpect(_excGenS, (Stringerex[]) null));
            Assert.Throws<ArgumentNullException>(() => _s.ThenExpect((IEnumerable<Stringerex>) null, _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpect((Stringerex) null, _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpect(new Stringerex('M'), (Func<int, StringerexMatch, string>) null, _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpect((Stringerex) null, (m1, m2) => (m1 + m2.Length).ToString(), _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpect(_excGenS, (Stringerex[]) null));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpect((IEnumerable<Stringerex>) null, _excGenS));

            // Met expectations
            AssertMatches(_s.ThenExpect(new Stringerex('L'), _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_s.ThenExpect(_excGenS, new Stringerex('L')), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_s.ThenExpect((IEnumerable<Stringerex>) new[] { new Stringerex('L') }, _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2 }, null, _expectInvOp, 1, _expInvOp);

            AssertMatches(_sr.ThenExpect(new Stringerex('L'), _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_sr.ThenExpect(_excGenS, new Stringerex('L')), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_sr.ThenExpect(new Stringerex('L'), (m1, m2) => m1 + m2.Index, _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 2 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_sr.ThenExpect((IEnumerable<Stringerex>) new[] { new Stringerex('L') }, _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, _expInvOp);

            // Unmet expectations
            AssertMatches(_s.ThenExpect(new Stringerex('Q'), _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_s.ThenExpect(_excGenS, new Stringerex('Q')), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_s.ThenExpect((IEnumerable<Stringerex>) new[] { new Stringerex('Q') }, _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);

            AssertMatches(_sr.ThenExpect(new Stringerex('Q'), _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_sr.ThenExpect(_excGenS, new Stringerex('Q')), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_sr.ThenExpect(new Stringerex('Q'), (m1, m2) => m1 + m2.Index, _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_sr.ThenExpect((IEnumerable<Stringerex>) new[] { new Stringerex('Q') }, _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
        }

        [Test]
        public void TestThenExpectGenerexWithResult()
        {
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect((Generex<int, int>) null, _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect((Generex<int, int>) null, (r1, m2) => r1 + m2.Result, _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect<int, string>(expectation: new Generex<int, int>(0), selector: null, exceptionGenerator: _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpectRaw(expectation: (Generex<int, int>) null, selector: (r1, r2) => r1 + r2, exceptionGenerator: _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpectRaw<int, string>(expectation: new Generex<int, int>(0), selector: null, exceptionGenerator: _excGen));

            // Met expectations
            AssertMatches(_g.ThenExpect(new Generex<int>(i => i % 7 == 0).Process(m => 2), _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 2 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpect(new Generex<int>(i => i % 7 == 0).Process(m => 2), (r1, m2) => r1 + m2.Result, _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 3 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpectRaw(new Generex<int>(i => i % 7 == 0).Process(m => 2), (r1, r2) => r1 + r2, _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 3 }, null, _expectInvOp, 1, _expInvOp);

            // Unmet expectations
            AssertMatches(_g.ThenExpect(new Generex<int>(i => i % 7 == 1).Process(m => 2), _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_gr.ThenExpect(new Generex<int>(i => i % 7 == 1).Process(m => 2), (r1, m2) => r1 + m2.Result, _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_gr.ThenExpectRaw(new Generex<int>(i => i % 7 == 1).Process(m => 2), (r1, r2) => r1 + r2, _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);

            Assert.Throws<ArgumentNullException>(() => _s.ThenExpect((Stringerex<int>) null, _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpect((Stringerex<int>) null, (r1, m2) => r1 + m2.Result, _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpect<int, string>(expectation: new Stringerex<int>(0), selector: null, exceptionGenerator: _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpectRaw(expectation: (Stringerex<int>) null, selector: (r1, r2) => r1 + r2, exceptionGenerator: _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpectRaw<int, string>(expectation: new Stringerex<int>(0), selector: null, exceptionGenerator: _excGenS));

            // Met expectations
            AssertMatches(_s.ThenExpect(new Stringerex('L').Process(m => 2), _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 2 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_sr.ThenExpect(new Stringerex('L').Process(m => 2), (r1, m2) => r1 + m2.Result, _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 3 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_sr.ThenExpectRaw(new Stringerex('L').Process(m => 2), (r1, r2) => r1 + r2, _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 3 }, null, _expectInvOp, 1, _expInvOp);

            // Unmet expectations
            AssertMatches(_s.ThenExpect(new Stringerex('Q').Process(m => 2), _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_sr.ThenExpect(new Stringerex('Q').Process(m => 2), (r1, m2) => r1 + m2.Result, _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_sr.ThenExpectRaw(new Stringerex('Q').Process(m => 2), (r1, r2) => r1 + r2, _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
        }

        [Test]
        public void TestThenExpectBindGenerexNoResult()
        {
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect((Func<GenerexMatch<int>, Generex<int>>) null, _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect((Func<GenerexMatch<int, int>, Generex<int>>) null, _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpectRaw((Func<int, Generex<int>>) null, _excGenRaw));

            // Met expectations
            AssertMatches(_g.ThenExpect(m => new Generex<int>(i => i % 7 == 0), _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpect(m => new Generex<int>(i => i % 7 == 0), _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpectRaw(m => new Generex<int>(i => i % 7 == 0), _excGenRaw), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, _expInvOp);

            // Unmet expectations
            AssertMatches(_g.ThenExpect(m => new Generex<int>(i => i % 7 == 1), _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_gr.ThenExpect(m => new Generex<int>(i => i % 7 == 1), _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_gr.ThenExpectRaw(m => new Generex<int>(i => i % 7 == 1), _excGenRaw), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);

            Assert.Throws<ArgumentNullException>(() => _s.ThenExpect((Func<StringerexMatch, Stringerex>) null, _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpect((Func<StringerexMatch<int>, Stringerex>) null, _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpectRaw((Func<int, Stringerex>) null, _excGenRaw));

            // Met expectations
            AssertMatches(_s.ThenExpect(m => new Stringerex('L'), _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_sr.ThenExpect(m => new Stringerex('L'), _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_sr.ThenExpectRaw(m => new Stringerex('L'), _excGenRaw), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 1 }, null, _expectInvOp, 1, _expInvOp);

            // Unmet expectations
            AssertMatches(_s.ThenExpect(m => new Stringerex('Q'), _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_sr.ThenExpect(m => new Stringerex('Q'), _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_sr.ThenExpectRaw(m => new Stringerex('Q'), _excGenRaw), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
        }

        [Test]
        public void TestThenExpectBindGenerexWithResult()
        {
            Assert.Throws<ArgumentNullException>(() => _g.ThenExpect((Func<GenerexMatch<int>, Generex<int, int>>) null, _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpect((Func<GenerexMatch<int, int>, Generex<int, string>>) null, _excGen));
            Assert.Throws<ArgumentNullException>(() => _gr.ThenExpectRaw((Func<int, Generex<int, int>>) null, _excGenRaw));

            // Met expectations
            AssertMatches(_g.ThenExpect(m => new Generex<int>(i => i % 7 == 0).Process(m2 => m2.Index + 2), _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 3 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpect(m => new Generex<int>(i => i % 7 == 0).Process(m2 => m2.Index + 2 + m.Result), _excGen), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 4 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_gr.ThenExpectRaw(m => new Generex<int>(i => i % 7 == 0).Process(m2 => m2.Index + 2 + m), _excGenRaw), _input, True, False, InvOp, False, InvOp, new object[] { 0, 2, 4 }, null, _expectInvOp, 1, _expInvOp);

            // Unmet expectations
            AssertMatches(_g.ThenExpect(m => new Generex<int>(i => i % 7 == 1).Process(m2 => m2.Index + 2), _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_gr.ThenExpect(m => new Generex<int>(i => i % 7 == 1).Process(m2 => m2.Index + 2 + m.Result), _excGen), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_gr.ThenExpectRaw(m => new Generex<int>(i => i % 7 == 1).Process(m2 => m2.Index + 2 + m), _excGenRaw), _input, Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);

            Assert.Throws<ArgumentNullException>(() => _s.ThenExpect((Func<StringerexMatch, Stringerex<int>>) null, _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpect((Func<StringerexMatch<int>, Stringerex<string>>) null, _excGenS));
            Assert.Throws<ArgumentNullException>(() => _sr.ThenExpectRaw((Func<int, Stringerex<int>>) null, _excGenRaw));

            // Met expectations
            AssertMatches(_s.ThenExpect(m => new Stringerex('L').Process(m2 => m2.Index + 2), _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 3 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_sr.ThenExpect(m => new Stringerex('L').Process(m2 => m2.Index + 2 + m.Result), _excGenS), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 4 }, null, _expectInvOp, 1, _expInvOp);
            AssertMatches(_sr.ThenExpectRaw(m => new Stringerex('L').Process(m2 => m2.Index + 2 + m), _excGenRaw), "MLP", True, False, InvOp, False, InvOp, new object[] { 0, 2, 4 }, null, _expectInvOp, 1, _expInvOp);

            // Unmet expectations
            AssertMatches(_s.ThenExpect(m => new Stringerex('Q').Process(m2 => m2.Index + 2), _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_sr.ThenExpect(m => new Stringerex('Q').Process(m2 => m2.Index + 2 + m.Result), _excGenS), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
            AssertMatches(_sr.ThenExpectRaw(m => new Stringerex('Q').Process(m2 => m2.Index + 2 + m), _excGenRaw), "MLP", Exception, False, InvOp, Exception, InvOp, _expectException, _expectException, _expectInvOp, _expException, _expInvOp);
        }
    }
}
