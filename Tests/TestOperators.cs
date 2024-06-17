using System;
using NUnit.Framework;

namespace RT.Generexes.Tests
{
    [TestFixture]
    sealed class TestOperators : TestBase
    {
        [Test]
        public void TestPlusArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { var g = _g + (Generex<int>) null; });
            Assert.Throws<ArgumentNullException>(() => { var g = _g + (Generex<int, int>) null; });
            Assert.Throws<ArgumentNullException>(() => { var g = _gr + (Generex<int>) null; });
            Assert.Throws<ArgumentNullException>(() => { var g = (Generex<int>) null + _g; });
            Assert.Throws<ArgumentNullException>(() => { var g = (Generex<int>) null + _gr; });
            Assert.Throws<ArgumentNullException>(() => { var g = (Generex<int, int>) null + _g; });

            Assert.Throws<ArgumentNullException>(() => { var g = (Generex<int>) null + 47; });
            Assert.Throws<ArgumentNullException>(() => { var g = (Generex<int>) null + new Predicate<int>(i => i != 0); });
            Assert.Throws<ArgumentNullException>(() => { var g = _g + (Predicate<int>) null; });
            Assert.Throws<ArgumentNullException>(() => { var g = (Generex<int, int>) null + 47; });
            Assert.Throws<ArgumentNullException>(() => { var g = (Generex<int, int>) null + new Predicate<int>(i => i != 0); });
            Assert.Throws<ArgumentNullException>(() => { var g = _gr + (Predicate<int>) null; });

            Assert.Throws<ArgumentNullException>(() => { var g = 47 + (Generex<int>) null; });
            Assert.Throws<ArgumentNullException>(() => { var g = new Predicate<int>(i => i != 0) + (Generex<int>) null; });
            Assert.Throws<ArgumentNullException>(() => { var g = (Predicate<int>) null + _g; });
            Assert.Throws<ArgumentNullException>(() => { var g = 47 + (Generex<int, int>) null; });
            Assert.Throws<ArgumentNullException>(() => { var g = new Predicate<int>(i => i != 0) + (Generex<int, int>) null; });
            Assert.Throws<ArgumentNullException>(() => { var g = (Predicate<int>) null + _gr; });

            Assert.Throws<ArgumentNullException>(() => { var s = _s + (Stringerex) null; });
            Assert.Throws<ArgumentNullException>(() => { var s = _s + (Stringerex<int>) null; });
            Assert.Throws<ArgumentNullException>(() => { var s = _sr + (Stringerex) null; });
            Assert.Throws<ArgumentNullException>(() => { var s = (Stringerex) null + _s; });
            Assert.Throws<ArgumentNullException>(() => { var s = (Stringerex) null + _sr; });
            Assert.Throws<ArgumentNullException>(() => { var s = (Stringerex<int>) null + _s; });

            Assert.Throws<ArgumentNullException>(() => { var s = (Stringerex) null + 'M'; });
            Assert.Throws<ArgumentNullException>(() => { var s = (Stringerex) null + new Predicate<char>(ch => ch != 'M'); });
            Assert.Throws<ArgumentNullException>(() => { var s = _s + (Predicate<char>) null; });
            Assert.Throws<ArgumentNullException>(() => { var s = (Stringerex<int>) null + 'M'; });
            Assert.Throws<ArgumentNullException>(() => { var s = (Stringerex<int>) null + new Predicate<char>(ch => ch != 'M'); });
            Assert.Throws<ArgumentNullException>(() => { var s = _sr + (Predicate<char>) null; });

            Assert.Throws<ArgumentNullException>(() => { var s = 'M' + (Stringerex) null; });
            Assert.Throws<ArgumentNullException>(() => { var s = new Predicate<char>(ch => ch != 'M') + (Stringerex) null; });
            Assert.Throws<ArgumentNullException>(() => { var s = (Predicate<char>) null + _s; });
            Assert.Throws<ArgumentNullException>(() => { var s = 'M' + (Stringerex<int>) null; });
            Assert.Throws<ArgumentNullException>(() => { var s = new Predicate<char>(ch => ch != 'M') + (Stringerex<int>) null; });
            Assert.Throws<ArgumentNullException>(() => { var s = (Predicate<char>) null + _sr; });
        }

        [Test]
        public void TestPlus()
        {
            AssertMatches(_g + new Predicate<int>(i => i % 7 == 0), _input, True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(new Predicate<int>(i => i % 7 == 0) + _g, _input, False, False, False, False, False, null, null, null, 0, 0);
            AssertMatches(_gr + new Predicate<int>(i => i % 7 == 0), _input, True, False, True, False, True, new object[] { 0, 2, 1 }, null, new object[] { 0, 2, 1 }, 1, 1);
            AssertMatches(new Predicate<int>(i => i % 7 == 0) + _gr, _input, False, False, False, False, False, null, null, null, 0, 0);
            AssertMatches(_g + new Generex<int>(i => i % 7 == 0), _input, True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(new Generex<int>(i => i % 7 == 0) + _g, _input, False, False, False, False, False, null, null, null, 0, 0);
            AssertMatches(_gr + new Generex<int>(i => i % 7 == 0), _input, True, False, True, False, True, new object[] { 0, 2, 1 }, null, new object[] { 0, 2, 1 }, 1, 1);
            AssertMatches(new Generex<int>(i => i % 7 == 0) + _gr, _input, False, False, False, False, False, null, null, null, 0, 0);
            AssertMatches(_g + new Generex<int>(i => i % 7 == 0).Process(m => 2), _input, True, False, True, False, True, new object[] { 0, 2, 2 }, null, new object[] { 0, 2, 2 }, 1, 1);
            AssertMatches(new Generex<int>(i => i % 7 == 0).Process(m => 2) + _g, _input, False, False, False, False, False, null, null, null, 0, 0);

            AssertMatches(_s + new Predicate<char>(ch => ch == 'L'), "MLP", True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(new Predicate<char>(ch => ch == 'L') + _s, "MLP", False, False, False, False, False, null, null, null, 0, 0);
            AssertMatches(_sr + new Predicate<char>(ch => ch == 'L'), "MLP", True, False, True, False, True, new object[] { 0, 2, 1 }, null, new object[] { 0, 2, 1 }, 1, 1);
            AssertMatches(new Predicate<char>(ch => ch == 'L') + _sr, "MLP", False, False, False, False, False, null, null, null, 0, 0);
            AssertMatches(_s + new Stringerex('L'), "MLP", True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(new Stringerex('L') + _s, "MLP", False, False, False, False, False, null, null, null, 0, 0);
            AssertMatches(_sr + new Stringerex('L'), "MLP", True, False, True, False, True, new object[] { 0, 2, 1 }, null, new object[] { 0, 2, 1 }, 1, 1);
            AssertMatches(new Stringerex('L') + _sr, "MLP", False, False, False, False, False, null, null, null, 0, 0);
            AssertMatches(_s + new Stringerex('L').Process(m => 2), "MLP", True, False, True, False, True, new object[] { 0, 2, 2 }, null, new object[] { 0, 2, 2 }, 1, 1);
            AssertMatches(new Stringerex('L').Process(m => 2) + _s, "MLP", False, False, False, False, False, null, null, null, 0, 0);
        }
    }
}
