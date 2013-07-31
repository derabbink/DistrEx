using System;
using DistrEx.Common;
using DistrEx.Coordinator.TargetSpecs;
using NUnit.Framework;

namespace DistrEx.Coordinator.Interface.Test
{
    [TestFixture]
    public class CoordinatorTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _identity = Wrapper.Wrap((object a) => a);
            _throw = Wrapper.Wrap<Exception, Exception>(e =>
            {
                throw e;
            });
            _local = OnCoordinator.Default;
        }

        #endregion

        private Instruction<object, object> _identity;
        private Instruction<Exception, Exception> _throw;
        private TargetSpec _local;

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void DoError()
        {
            var expected = new Exception("Expected");
            Coordinator.Do(_local.Do(_throw), expected);
        }

        [Test]
        public void DoSuccessful()
        {
            var expected = new object();
            object actual = Coordinator.Do(_local.Do(_identity), expected).ResultValue;
            Assert.That(actual, Is.SameAs(expected));
        }
    }
}
