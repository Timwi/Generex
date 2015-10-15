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
    sealed class TestThrow : TestBase
    {
        [Test]
        public void TestGenerexThrow()
        {
            Assert.Throws<ArgumentNullException>(() => { _g.Throw(null); });
            Assert.Throws<ArgumentNullException>(() => { _g.Throw<string>(null); });
            Assert.Throws<ArgumentNullException>(() => { _gr.Throw(null); });

            Assert.Throws<ExpectedException>(() => { _g.Throw(m => new ExpectedException()).Match(_input); });
            Assert.Throws<ExpectedException>(() => { _g.Throw<string>(m => new ExpectedException()).Match(_input); });
            Assert.Throws<ExpectedException>(() => { _gr.Throw(m => new ExpectedException()).Match(_input); });
        }
    }
}
