using System;
using System.Threading;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.TargetSpecs;
using NUnit.Framework;

namespace DistrEx.Coordinator.Test.Parallel
{
    [TestFixture]
    public class Parallel1Test
    {
        private TargetSpec _local;
        private Instruction<int, int> _identity;
        private Instruction<int, int> _blockingIdentity;
        private Instruction<Exception, Exception> _throw;
        private int _identityArgument;
        private Exception _throwArgument;
        private ManualResetEventSlim _blockingIdentityNotify;
        private ManualResetEventSlim _blockingIdentityHold;

        #region setup
        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _blockingIdentityNotify = new ManualResetEventSlim(false);
            _blockingIdentityHold = new ManualResetEventSlim(false);

            _local = OnCoordinator.Default;
            _identity = (ct, p, i) => i;
            _blockingIdentity = (ct, p, i) =>
                {
                    _blockingIdentityNotify.Set();
                    _blockingIdentityHold.Wait(ct);
                    return i;
                };
            _identityArgument = 1;

            _throw = (ct, p, e) => { throw e; };
            _throwArgument = new Exception("Expected");
        }
        #endregion

        [Test]
        public void ParallelAllSuccessful()
        {
            var expected = _identityArgument;
            var monitored = InstructionSpecs.Parallel.MonitoredParallelInstructionSpec<int, int, Tuple<int>>.Create(
                _local.Do(_identity));
            var wrappedInstruction = TargetedInstruction<int, Tuple<int>>.Create(_local, monitored);
            Tuple<int> result = Interface.Coordinator.Do(wrappedInstruction, _identityArgument).ResultValue;
            Assert.That(result.Item1, Is.EqualTo(expected));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ParallelAllFail()
        {
            var monitored = InstructionSpecs.Parallel.MonitoredParallelInstructionSpec<Exception, Exception, Tuple<Exception>>.Create(
                _local.Do(_throw));
            var wrappedInstruction = TargetedInstruction<Exception, Tuple<Exception>>.Create(_local, monitored);
            Tuple<Exception> result = Interface.Coordinator.Do(wrappedInstruction, _throwArgument).ResultValue;
        }

        [Test]
        [ExpectedException(typeof(OperationCanceledException))]
        public void ParallelAllTimeout()
        {
            var monitored = InstructionSpecs.Parallel.MonitoredParallelInstructionSpec<int, int, Tuple<int>>.Create(
                _local.Do(_blockingIdentity));
            var wrappedInstruction = TargetedInstruction<int, Tuple<int>>.Create(_local, monitored);
            Tuple<int> result = Interface.Coordinator.Do(wrappedInstruction, _identityArgument).ResultValue;
        }
    }
}
