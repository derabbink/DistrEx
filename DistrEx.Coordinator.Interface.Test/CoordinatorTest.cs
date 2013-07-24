using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DistrEx.Common;
using DistrEx.Coordinator.InstructionSpecs;
using DistrEx.Coordinator.TargetSpecs;
using NUnit.Framework;

namespace DistrEx.Coordinator.Interface.Test
{
    [TestFixture]
    public class CoordinatorTest
    {
        private InstructionSpec<object, object> _identity;
        private InstructionSpec<Exception, Exception> _throw;
        private TargetSpec _local;

        #region setup

        [SetUp]
        public void Setup()
        {
            _identity = NonTransferrableDelegateInstructionSpec<object, object>.Create(Wrapper.Wrap((object a)=>a));
            _throw = NonTransferrableDelegateInstructionSpec<Exception, Exception>.Create(Wrapper.Wrap<Exception, Exception>(e => { throw e; }));
            _local = OnCoordinator.Default;
        }

        #endregion

        #region tests

        [Test]
        public void DoSuccessful()
        {
            object expected = new object();
            object actual = Coordinator.Do(_local.Do(_identity), expected).ResultValue;
            Assert.That(actual, Is.SameAs(expected));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void DoError()
        {
            Exception expected = new Exception("Expected");
            Coordinator.Do(_local.Do(_throw), expected);
        }

        #endregion
    }
}
