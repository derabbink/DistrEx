using System;
using DistrEx.Common;
using DistrEx.Coordinator.TargetSpecs;
using NUnit.Framework;

namespace DistrEx.Coordinator.Interface.Test
{
    [TestFixture]
    public class CompletedStepTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _identity = Wrapper.Wrap((object a) => a);
            _identityEx = Wrapper.Wrap((Exception e) => e);
            _throw = Wrapper.Wrap<Exception, Exception>(e =>
            {
                throw e;
            });
            _local = OnCoordinator.Default;
        }

        #endregion

        private Instruction<object, object> _identity;
        private Instruction<Exception, Exception> _identityEx;
        private Instruction<Exception, Exception> _throw;
        private TargetSpec _local;

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void DoChainedError()
        {
            var expected = new Exception("Expected");
            Coordinator.Do(_local.Do(_identityEx), expected).ThenDo(_local.Do(_throw));
        }

        [Test]
        public void DoChainedSuccessful()
        {
            var expected = new object();
            object actual = Coordinator.Do(_local.Do(_identity), expected).ThenDo(_local.Do(_identity)).ResultValue;
            Assert.That(actual, Is.SameAs(expected));
        }
    }
}
