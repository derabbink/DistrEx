using System;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.TargetSpecs;
using NUnit.Framework;

namespace DistrEx.Coordinator.Test.Parallel
{
    [TestFixture]
    public class Parallel2Test
    {
        private TargetSpec _local;
        private Instruction<int, int> _identity;
        private Instruction<Exception, Exception> _throw;
        private int _identityArgument;
        private Exception _throwArgument;

        #region setup
        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _local = OnCoordinator.Default;
            _identity = (ct, p, i) => i;
            _identityArgument = 1;

            _throw = (ct, p, e) => { throw e; };
            _throwArgument = new Exception("Expected");
        }
        #endregion

        [Test]
        public void ParallelAllSuccessful()
        {
            var expected = _identityArgument;
            var result = Coordinator2.Do(_local.Do(_identity), _local.Do(_identity), _identityArgument).ResultValue;
            Assert.That(result.Item1, Is.EqualTo(expected));
            Assert.That(result.Item2, Is.EqualTo(expected));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ParallelAllFail()
        {
            Tuple<Exception, Exception> result = Coordinator2.Do(_local.Do(_throw), _local.Do(_throw), _throwArgument).ResultValue;
        }
    }
}
