using Inceptum.WebApi.Help.Common;
using NUnit.Framework;

namespace Inceptum.WebApi.Help.Tests.Common
{
    [TestFixture]
    public class ValueHolderTests
    {
        [Test]
        public void ShouldNotCreateNewValueHolderIfValueHolderPassed()
        {
            var expected = ValueHolder.Create(42);
            var actual = ValueHolder.Create(expected);

            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            Assert.IsTrue(ReferenceEquals(expected, actual));
        }

        [Test]
        public void ShouldReturnNullObjectIfNullPassed()
        {
            var actual = ValueHolder.Create(null);

            Assert.IsNotNull(actual);
            Assert.IsTrue(ReferenceEquals(ValueHolder.Null, actual));
        }

        [Test]
        public void ShouldReturnValueIfValidInstancePassed()
        {
            var holder = ValueHolder.Create(42);

            Assert.IsNotNull(holder);
            Assert.AreEqual(42, holder.Value);
        }

        [Test]
        public void ShouldReturnValueIfValidInstanceFactoryPassed()
        {
            var holder = ValueHolder.Create(() => 42);

            Assert.IsNotNull(holder);
            Assert.AreEqual(42, holder.Value);
        }

        [Test]
        public void ShouldReturnValueEachTimeFromFactoryIfValidInstanceFactoryPassed()
        {
            var counter = 0;
            var holder = ValueHolder.Create(() =>
            {
                counter++;
                return 42;
            });

            Assert.IsNotNull(holder);
            Assert.AreEqual(42, holder.Value);
            Assert.AreEqual(42, holder.Value);
            Assert.AreEqual(2, counter);
        }
    }
}