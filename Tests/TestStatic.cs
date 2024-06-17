using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace RT.Generexes.Tests
{
    [TestFixture]
    sealed class TestStatic : TestBase
    {
        new int[] _input = new int[] { 47, 24567837 };

        [Test]
        public void TestCreateExtensionMethods()
        {
            Assert.DoesNotThrow(() =>
            {
                ((int[]) null).CreateAnyGenerex();
                ((int[]) null).CreateAnythingGenerex();
                ((int[]) null).CreateAnythingGreedyGenerex();
                ((int[]) null).CreateEmptyGenerex();
                ((int[]) null).CreateEndGenerex();
                ((int[]) null).CreateFailGenerex();
                ((int[]) null).CreateGenerex(i => i == 47);
                ((int[]) null).CreateRecursiveGenerex(expr => expr);
                ((int[]) null).CreateStartGenerex();
            });

            Assert.Throws<ArgumentNullException>(() => { ((int[]) null).CreateGenerex(predicate: null); });
            Assert.Throws<ArgumentNullException>(() => { ((int[]) null).CreateRecursiveGenerex(generator: null); });

            AssertMatches(_input.CreateAnyGenerex(), _input, True, True, True, False, True, new object[] { 0, 1 }, null, new object[] { 1, 1 }, 2, 2);
            AssertMatches(_input.CreateAnythingGenerex(), _input, True, True, True, True, True, new object[] { 0, 0 }, new object[] { 0, 2 }, new object[] { 2, 0 }, 3, 3);
            AssertMatches(_input.CreateAnythingGreedyGenerex(), _input, True, True, True, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 2, 2);
            AssertMatches(_input.CreateEmptyGenerex(), _input, True, True, True, False, True, new object[] { 0, 0 }, null, new object[] { 2, 0 }, 3, 3);
            AssertMatches(_input.CreateEndGenerex(), _input, True, False, False, False, True, new object[] { 2, 0 }, null, new object[] { 2, 0 }, 1, 1);
            AssertMatches(_input.CreateFailGenerex(), _input, False, False, False, False, False, null, null, null, 0, 0);
            AssertMatches(_input.CreateGenerex(i => i == 47), _input, True, False, True, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(_input.CreateStartGenerex(), _input, True, False, False, False, True, new object[] { 0, 0 }, null, new object[] { 0, 0 }, 1, 1);
        }

        [Test]
        public void TestInAnyOrderNoResult()
        {
            Assert.Throws<ArgumentNullException>(() => Generex.InAnyOrder<int>(elements: null));
            Assert.Throws<ArgumentNullException>(() => Generex.InAnyOrder<int>(generexes: null));
            Assert.Throws<ArgumentNullException>(() => Generex.InAnyOrder<int>((Generex<int>) null));
            Assert.Throws<ArgumentNullException>(() => Generex.InAnyOrder<int>(comparer: null, elements: new int[] { 47 }));
            Assert.Throws<ArgumentNullException>(() => Generex.InAnyOrder<int>(comparer: EqualityComparer<int>.Default, elements: null));

            AssertMatches(Generex.InAnyOrder(47, 24567837), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.InAnyOrder(24567837, 47), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.InAnyOrder<int>(new Generex<int>(i => i % 7 == 5), new Generex<int>(i => i % 7 == 0)), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.InAnyOrder<int>(new Generex<int>(i => i % 7 == 0), new Generex<int>(i => i % 7 == 5)), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.InAnyOrder(_mod7, 5, 0), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.InAnyOrder(_mod7, 0, 5), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
        }

        [Test]
        public void TestInAnyOrderWithResult()
        {
            Assert.Throws<ArgumentNullException>(() => Generex.InAnyOrder<int, int>((Generex<int, int>[]) null));
            Assert.Throws<ArgumentNullException>(() => Generex.InAnyOrder<int, int>((IEnumerable<Generex<int, int>>) null));
            Assert.Throws<ArgumentNullException>(() => Generex.InAnyOrder<int, int>((Generex<int, int>) null));
            var iao1 = Generex.InAnyOrder<int, string>(new Generex<int>(i => i % 7 == 5).Process(m => "Five"), new Generex<int>(i => i % 7 == 0).Process(m => "Zero"));
            var iao2 = Generex.InAnyOrder<int, string>(new Generex<int>(i => i % 7 == 0).Process(m => "Zero"), new Generex<int>(i => i % 7 == 5).Process(m => "Five"));
            var iao3 = Generex.InAnyOrder((IEnumerable<Generex<int, string>>) new[] { new Generex<int>(i => i % 7 == 0).Process(m => "Zero"), new Generex<int>(i => i % 7 == 5).Process(m => "Five") });
            var iao4 = Generex.InAnyOrder((IEnumerable<Generex<int, string>>) new[] { new Generex<int>(i => i % 7 == 5).Process(m => "Five"), new Generex<int>(i => i % 7 == 0).Process(m => "Zero") });
            AssertMatches(iao1, _input, True, False, False, True, True, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, 1, 1);
            AssertMatches(iao2, _input, True, False, False, True, True, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, 1, 1);
            AssertMatches(iao3, _input, True, False, False, True, True, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, 1, 1);
            AssertMatches(iao4, _input, True, False, False, True, True, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, 1, 1);
        }

        [Test]
        public void TestNew()
        {
            Assert.Throws<ArgumentNullException>(() => Generex.New<int>(elements: null));
            Assert.Throws<ArgumentNullException>(() => Generex.New<int>(generexes: null));
            Assert.Throws<ArgumentNullException>(() => Generex.New<int>(predicate: null));
            Assert.Throws<ArgumentNullException>(() => Generex.New<int>(comparer: null, elements: new[] { 1 }));
            Assert.Throws<ArgumentNullException>(() => Generex.New<int>(comparer: _mod7, elements: null));
            AssertMatches(Generex.New(47, 24567837), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.New<int>(new Generex<int>(i => i % 7 == 5), new Generex<int>(i => i % 7 == 0)), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.New<int>(i => i % 7 == 5), _input, True, False, True, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(Generex.New(_mod7, 5, 0), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
        }

        [Test]
        public void TestNot()
        {
            Assert.Throws<ArgumentNullException>(() => Generex.Not<int>(elements: null));
            Assert.Throws<ArgumentNullException>(() => Generex.Not<int>(comparer: null, elements: new[] { 1 }));
            Assert.Throws<ArgumentNullException>(() => Generex.Not<int>(comparer: _mod7, elements: null));
            AssertMatches(Generex.Not(0, 1, 2, 3), _input, True, True, True, False, True, new object[] { 0, 1 }, null, new object[] { 1, 1 }, 2, 2);
            AssertMatches(Generex.Not(_mod7, 0, 1, 2, 3), _input, True, False, True, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
        }

        [Test]
        public void TestRawMatchNullable()
        {
            // RawMatchNullable, RawMatchReverseNullable
            var g = Generex.New(47).Process(m => 1);
            Assert.AreEqual(1, g.RawMatchNullable(_input));
            Assert.AreEqual(null, g.RawMatchNullable(_input, 1));
            Assert.AreEqual(1, g.RawMatchReverseNullable(_input));
            Assert.AreEqual(1, g.RawMatchReverseNullable(_input, 1));
            Assert.AreEqual(null, g.RawMatchReverseNullable(_input, 0));
            var s = new Stringerex('M').Process(m => 1);
            Assert.AreEqual(1, s.RawMatchNullable("MLP"));
            Assert.AreEqual(null, s.RawMatchNullable("MLP", 1));
            Assert.AreEqual(1, s.RawMatchReverseNullable("MLP"));
            Assert.AreEqual(1, s.RawMatchReverseNullable("MLP", 1));
            Assert.AreEqual(null, s.RawMatchReverseNullable("MLP", 0));
        }

        [Test]
        public void TestToStringerex()
        {
            var ch = new Generex<char>('M').Then('L');
            AssertMatches(ch.ToStringerex(), "MLP", True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(ch.Process(m => string.Join(",", m.Match)).ToStringerex(), "MLP", True, False, True, False, True, new object[] { 0, 2, "M,L" }, null, new object[] { 0, 2, "M,L" }, 1, 1);
        }
    }
}
