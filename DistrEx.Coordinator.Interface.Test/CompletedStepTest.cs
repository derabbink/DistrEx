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
        private InstructionSpec<object, object> _identity;
        private InstructionSpec<Exception, Exception> _identityEx;
        private InstructionSpec<Exception, Exception> _throw;
        private TargetSpec _local;

        #region setup

        [SetUp]
        public void Setup()
        {
            _identity = NonTransferrableDelegateInstructionSpec<object, object>.Create(Wrapper.Wrap((object a) => a));
            _identityEx = NonTransferrableDelegateInstructionSpec<Exception, Exception>.Create(Wrapper.Wrap((Exception e) => e));
            _throw = NonTransferrableDelegateInstructionSpec<Exception, Exception>.Create(Wrapper.Wrap<Exception, Exception>(e => { throw e; }));
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
