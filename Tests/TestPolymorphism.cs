using System;
using NUnit.Framework;

namespace RT.Generexes.Tests
{
    [TestFixture]
    sealed class TestPolymorphism : TestBase
    {
        [Test]
        public void TestAnyOfType()
        {
            var input = new Exception[] { new ArgumentException(), new ArgumentNullException(), new InvalidOperationException() };
            var generex = Generex<Exception>.AnyOfType<ArgumentException>();
            AssertMatches(generex, input, True, True, True, False, True, new object[] { 0, 1, input[0] }, null, new object[] { 1, 1, input[1] }, 2, 2);
        }

        [Test]
        public void TestOfType()
        {
            var input = new Exception[] { new ArgumentException(), new ArgumentNullException(), new InvalidOperationException() };
            var generex = Generex<Exception>.Any.Process(m => m.Match[0]).OfType<ArgumentException>().RepeatGreedy();
            AssertMatches(generex, input, True, True, True, False, True, new object[] { 0, 2, new Exception[] { input[0], input[1] } }, null, new object[] { 3, 0, new Exception[] { } }, 3, 3);
        }

        [Test]
        public void TestCast()
        {
            var input = new Exception[] { new ArgumentException(), new ArgumentNullException(), new InvalidOperationException() };
            var generex = Generex<Exception>.Any.Process(m => m.Match[0]).Cast<ArgumentException>();
            AssertMatches(generex, input, True, True, True, False, InvCast, new object[] { 0, 1, input[0] }, null, _expectInvCast, _expInvCast, _expInvCast);
        }
    }
}
