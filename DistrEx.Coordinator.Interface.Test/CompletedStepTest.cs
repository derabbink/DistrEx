using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistrEx.Common;
using DistrEx.Coordinator.InstructionSpecs;
using DistrEx.Coordinator.TargetSpecs;
using NUnit.Framework;

namespace DistrEx.Coordinator.Interface.Test
{
    [TestFixture]
    public class CompletedStepTest
    {
        private Instruction<object, object> _identity;
        private Instruction<Exception, Exception> _identityEx;
        private Instruction<Exception, Exception> _throw;
        private TargetSpec _local;

        #region setup

        [SetUp]
        public void Setup()
        {
            _identity = Wrapper.Wrap((object a) => a);
            _identityEx = Wrapper.Wrap((Exception e) => e);
            _throw = Wrapper.Wrap<Exception, Exception>(e => { throw e; });
            _local = OnCoordinator.Default;
        }

        #endregion

        #region tests

        [Test]
        public void DoChainedSuccessful()
        {
            object expected = new object();
            object actual = Coordinator.Do(_local.Do(_identity), expected).ThenDo(_local.Do(_identity)).ResultValue;
            Assert.That(actual, Is.SameAs(expected));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void DoChainedError()
        {
            Exception expected = new Exception("Expected");
            Coordinator.Do(_local.Do(_identityEx), expected).ThenDo(_local.Do(_throw));
        }

        #endregion
    }
}
