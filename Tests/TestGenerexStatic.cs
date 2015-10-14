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
    class TestGenerexStatic : TestBase
    {
        int[] _input = new int[] { 47, 24567837 };

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
        public void TestStaticMethods()
        {
            // Generex.InAnyOrder<>()
            Assert.Throws<ArgumentNullException>(() => { Generex.InAnyOrder<int>(elements: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.InAnyOrder<int>(generexes: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.InAnyOrder<int>((Generex<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.InAnyOrder<int>(comparer: null, elements: new int[] { 47 }); });
            Assert.Throws<ArgumentNullException>(() => { Generex.InAnyOrder<int>(comparer: EqualityComparer<int>.Default, elements: null); });

            AssertMatches(Generex.InAnyOrder(47, 24567837), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.InAnyOrder(24567837, 47), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.InAnyOrder(new Generex<int>(i => i % 7 == 5), new Generex<int>(i => i % 7 == 0)), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.InAnyOrder(new Generex<int>(i => i % 7 == 0), new Generex<int>(i => i % 7 == 5)), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.InAnyOrder(new Mod7IntEqualityComparer(), 5, 0), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.InAnyOrder(new Mod7IntEqualityComparer(), 0, 5), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);

            // Generex.InAnyOrder<>() with results
            Assert.Throws<ArgumentNullException>(() => { Generex.InAnyOrder<int, int>(generexes: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.InAnyOrder<int, int>((Generex<int, int>) null); });
            var iao1 = Generex.InAnyOrder(new Generex<int>(i => i % 7 == 5).Process(m => "Five"), new Generex<int>(i => i % 7 == 0).Process(m => "Zero"));
            var iao2 = Generex.InAnyOrder(new Generex<int>(i => i % 7 == 0).Process(m => "Zero"), new Generex<int>(i => i % 7 == 5).Process(m => "Five"));
            var iao3 = Generex.InAnyOrder((IEnumerable<Generex<int, string>>) new[] { new Generex<int>(i => i % 7 == 0).Process(m => "Zero"), new Generex<int>(i => i % 7 == 5).Process(m => "Five") });
            var iao4 = Generex.InAnyOrder((IEnumerable<Generex<int, string>>) new[] { new Generex<int>(i => i % 7 == 5).Process(m => "Five"), new Generex<int>(i => i % 7 == 0).Process(m => "Zero") });
            AssertMatches(iao1, _input, True, False, False, True, True, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, 1, 1);
            AssertMatches(iao2, _input, True, False, False, True, True, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, 1, 1);
            AssertMatches(iao3, _input, True, False, False, True, True, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, 1, 1);
            AssertMatches(iao4, _input, True, False, False, True, True, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, new object[] { 0, 2, new[] { "Five", "Zero" } }, 1, 1);

            // Generex.New<>()
            Assert.Throws<ArgumentNullException>(() => { Generex.New<int>(elements: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.New<int>(generexes: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.New<int>(predicate: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.New<int>(comparer: null, elements: new[] { 1 }); });
            Assert.Throws<ArgumentNullException>(() => { Generex.New<int>(comparer: new Mod7IntEqualityComparer(), elements: null); });
            AssertMatches(Generex.New(47, 24567837), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.New(new Generex<int>(i => i % 7 == 5), new Generex<int>(i => i % 7 == 0)), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);
            AssertMatches(Generex.New<int>(i => i % 7 == 5), _input, True, False, True, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(Generex.New(new Mod7IntEqualityComparer(), 5, 0), _input, True, False, False, True, True, new object[] { 0, 2 }, new object[] { 0, 2 }, new object[] { 0, 2 }, 1, 1);

            // Generex.Not<>()
            Assert.Throws<ArgumentNullException>(() => { Generex.Not<int>(elements: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Not<int>(comparer: null, elements: new[] { 1 }); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Not<int>(comparer: new Mod7IntEqualityComparer(), elements: null); });
            AssertMatches(Generex.Not(0, 1, 2, 3), _input, True, True, True, False, True, new object[] { 0, 1 }, null, new object[] { 1, 1 }, 2, 2);
            AssertMatches(Generex.Not(new Mod7IntEqualityComparer(), 0, 1, 2, 3), _input, True, False, True, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);

            // Generex.Or[Default|Null](Greedy?)<>() (extension methods)
            Assert.Throws<ArgumentNullException>(() => { Generex.OrDefault<int, int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrDefaultGreedy<int, int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrNull<int, int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrNullGreedy<int, int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrDefault<int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrDefaultGreedy<int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrNull<int>(inner: null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.OrNullGreedy<int>(inner: null); });
            AssertMatches(Generex.New(47).Process(m => 1).OrDefault(), _input, True, True, True, False, True, new object[] { 0, 0, 0 }, null, new object[] { 2, 0, 0 }, 3, 3);
            AssertMatches(Generex.New(47).Process(m => 1).OrDefaultGreedy(), _input, True, True, True, False, True, new object[] { 0, 1, 1 }, null, new object[] { 2, 0, 0 }, 3, 3);
            AssertMatches(Generex.New(47).Process(m => 1).OrNull(), _input, True, True, True, False, True, new object[] { 0, 0, null }, null, new object[] { 2, 0, null }, 3, 3);
            AssertMatches(Generex.New(47).Process(m => 1).OrNullGreedy(), _input, True, True, True, False, True, new object[] { 0, 1, 1 }, null, new object[] { 2, 0, null }, 3, 3);
            AssertMatches(Stringerex.New('M').Process(m => 1).OrDefault(), "MLP", True, True, True, False, True, new object[] { 0, 0, 0 }, null, new object[] { 3, 0, 0 }, 4, 4);
            AssertMatches(Stringerex.New('M').Process(m => 1).OrDefaultGreedy(), "MLP", True, True, True, False, True, new object[] { 0, 1, 1 }, null, new object[] { 3, 0, 0 }, 4, 4);
            AssertMatches(Stringerex.New('M').Process(m => 1).OrNull(), "MLP", True, True, True, False, True, new object[] { 0, 0, null }, null, new object[] { 3, 0, null }, 4, 4);
            AssertMatches(Stringerex.New('M').Process(m => 1).OrNullGreedy(), "MLP", True, True, True, False, True, new object[] { 0, 1, 1 }, null, new object[] { 3, 0, null }, 4, 4);

            // Generex.Ors<>()
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((Generex<int>) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((Generex<int>[]) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((IEnumerable<Generex<int>>) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((Generex<int, int>) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((Generex<int, int>[]) null); });
            Assert.Throws<ArgumentNullException>(() => { Generex.Ors((IEnumerable<Generex<int, int>>) null); });
            AssertMatches(Generex.Ors(Generex.New(47), Generex.New(666)), _input, True, False, True, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(Generex.Ors((IEnumerable<Generex<int>>) new[] { Generex.New(47), Generex.New(666) }), _input, True, False, True, False, True, new object[] { 0, 1 }, null, new object[] { 0, 1 }, 1, 1);
            AssertMatches(Generex.Ors(Generex.New(47).Process(m => 1), Generex.New(666).Process(m => 2)), _input, True, False, True, False, True, new object[] { 0, 1, 1 }, null, new object[] { 0, 1, 1 }, 1, 1);
            AssertMatches(Generex.Ors((IEnumerable<Generex<int, int>>) new[] { Generex.New(47).Process(m => 1), Generex.New(666).Process(m => 2) }), _input, True, False, True, False, True, new object[] { 0, 1, 1 }, null, new object[] { 0, 1, 1 }, 1, 1);

            // Generex.RawMatch(Reverse?)Nullable<>()
            Assert.AreEqual(1, Generex.New(47).Process(m => 1).RawMatchNullable(_input));
            Assert.AreEqual(null, Generex.New(47).Process(m => 1).RawMatchNullable(_input, 1));
            Assert.AreEqual(1, Generex.New(47).Process(m => 1).RawMatchReverseNullable(_input));
            Assert.AreEqual(1, Generex.New(47).Process(m => 1).RawMatchReverseNullable(_input, 1));
            Assert.AreEqual(1, Stringerex.New('M').Process(m => 1).RawMatchNullable("MLP"));
            Assert.AreEqual(null, Stringerex.New('M').Process(m => 1).RawMatchNullable("MLP", 1));
            Assert.AreEqual(1, Stringerex.New('M').Process(m => 1).RawMatchReverseNullable("MLP"));
            Assert.AreEqual(1, Stringerex.New('M').Process(m => 1).RawMatchReverseNullable("MLP", 1));

            // Generex.ToStringerex()
            var ch = new Generex<char>('M').Then('L');
            AssertMatches(ch.ToStringerex(), "MLP", True, False, True, False, True, new object[] { 0, 2 }, null, new object[] { 0, 2 }, 1, 1);
            AssertMatches(ch.Process(m => string.Join(",", m.Match)).ToStringerex(), "MLP", True, False, True, False, True, new object[] { 0, 2, "M,L" }, null, new object[] { 0, 2, "M,L" }, 1, 1);
        }
    }
}
