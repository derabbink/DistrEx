using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.TargetSpecs;
using NUnit.Framework;

namespace DistrEx.Coordinator.Test.Integration
{
    [TestFixture]
    public class Parallel2IntegrationTests
    {
        private TargetSpec _local;
        private Instruction<int, int> _identity;
        private int _identityArgument;

        #region setup
        [SetUp]
        public void Setup()
        {
            _local = OnCoordinator.Default;
            _identity = (ct, p, i) => i;
            _identityArgument = 1;
        }
        #endregion

        [Test]
        public void Test()
        {
            var expected = _identityArgument;
            Tuple<int, Tuple<int, int>> result =
                Coordinator2.Do(
                    _local.Do(_identity),
                    Coordinator2.Do(
                        _local.Do(_identity),
                        _local.Do(_identity)),
                    _identityArgument)
                    .ResultValue;
            Assert.That(result.Item1, Is.EqualTo(expected));
            Assert.That(result.Item2.Item1, Is.EqualTo(expected));
            Assert.That(result.Item2.Item2, Is.EqualTo(expected));
        }
    }
}
