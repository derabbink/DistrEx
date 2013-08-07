using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.Interface.TargetedInstructions;
using DistrEx.Coordinator.TargetSpecs;
using NUnit.Framework;

namespace DistrEx.Coordinator.Test.Parallel
{
    [TestFixture]
    public class Parallel2Test
    {
        private const int ParallelCount = 2;
        private TargetSpec _local;
        private Instruction<Exception, Exception>[] _identities;
        private Instruction<Exception, Exception>[] _blockingIdentities;
        private Instruction<Exception, Exception>[] _throws;
        private ManualResetEventSlim[] _blockingIdentityNotifies;
        private ManualResetEventSlim[] _blockingIdentityHolds;

        private TargetedInstruction<Exception, Exception>[] _identityInstructions;
        private TargetedInstruction<Exception, Exception>[] _blockingIdentityInstructions;
        private TargetedInstruction<Exception, Exception>[] _throwInstructions;

        private Exception _identityArgument;
        private Exception _throwArgument;

        #region setup
        [SetUp]
        public void FixtureSetup()
        {
            _local = OnCoordinator.Default;
            _blockingIdentityNotifies = new ManualResetEventSlim[ParallelCount];
            _blockingIdentityHolds = new ManualResetEventSlim[ParallelCount];
            _identities = new Instruction<Exception, Exception>[ParallelCount];
            _blockingIdentities = new Instruction<Exception, Exception>[ParallelCount];
            _throws = new Instruction<Exception, Exception>[ParallelCount];
            
            for (int i = 0; i < ParallelCount; i++)
            {
                var iClosure = i;
                _blockingIdentityNotifies[i] = new ManualResetEventSlim(false);
                _blockingIdentityHolds[i] = new ManualResetEventSlim(false);
                _identities[i] = (ct, p, arg) => arg;
                _blockingIdentities[i] = (ct, p, arg) =>
                    {
                        _blockingIdentityNotifies[iClosure].Set();
                        _blockingIdentityHolds[iClosure].Wait(ct);
                        return arg;
                    };
                _throws[i] = (ct, p, e) => { throw e; };
            }
            
            _identityArgument = new Exception("Identity");
            _throwArgument = new Exception("Expected");

            _identityInstructions = new TargetedInstruction<Exception, Exception>[ParallelCount];
            _blockingIdentityInstructions = new TargetedInstruction<Exception, Exception>[ParallelCount];
            _throwInstructions = new TargetedInstruction<Exception, Exception>[ParallelCount];

            for (int i = 0; i < ParallelCount; i++)
            {
                _identityInstructions[i] = _local.Do(_identities[i]);
                _blockingIdentityInstructions[i] = _local.Do(_blockingIdentities[i]);
                _throwInstructions[i] = _local.Do(_throws[i]);
            }
        }

        #endregion

        [Test]
        public void ParallelAllSuccessful()
        {
            var expected = _identityArgument;
            var instructions = _identityInstructions;
            var result = GetResult(instructions, _identityArgument);
            Assert.That(result.Item1, Is.EqualTo(expected));
            Assert.That(result.Item2, Is.EqualTo(expected));
        }

        [Test]
        public void ParallelAllSimultaneous()
        {
            var expected = _identityArgument;
            var instructions = _blockingIdentityInstructions;
            Task<Tuple<Exception, Exception>> task = Task<Tuple<Exception, Exception>>.Factory.StartNew(() => GetResult(instructions, _identityArgument));
            WaitAll(_blockingIdentityNotifies);
            SetAll(_blockingIdentityHolds);
            task.Wait();
            var result = task.Result;
            Assert.That(result.Item1, Is.EqualTo(expected));
            Assert.That(result.Item2, Is.EqualTo(expected));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ParallelAllFail()
        {
            var instructions = _throwInstructions;
            var result = GetResult(instructions, _throwArgument);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ParallelFailOne([Values(0, 1)]int failIndex)
        {
            var instructions = _identityInstructions;
            instructions[failIndex] = _throwInstructions[failIndex];
            var result = GetResult(instructions, _throwArgument);
        }

        [Test]
        [ExpectedException(typeof(OperationCanceledException))]
        public void ParallelAllTimeout()
        {
            var instructions = _blockingIdentityInstructions;
            var result = GetResult(instructions, _identityArgument);
        }

        [Test]
        [ExpectedException(typeof(OperationCanceledException))]
        public void ParallelTimeoutOne([Values(0, 1)]int timeoutIndex)
        {
            var instructions = _identityInstructions;
            instructions[timeoutIndex] = _blockingIdentityInstructions[timeoutIndex];
            var result = GetResult(instructions, _identityArgument);
        }

        private Tuple<Exception, Exception> GetResult(TargetedInstruction<Exception, Exception>[] instructions, Exception argument)
        {
            return Coordinator.Do(instructions[0], instructions[1], argument).ResultValue;
        }

        private static void WaitAll(ManualResetEventSlim[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] != null)
                    blocks[i].Wait();
            }
        }

        private static void SetAll(ManualResetEventSlim[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] != null)
                    blocks[i].Set();
            }
        }
    }
}
