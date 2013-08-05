using System;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.TargetSpecs;
using NUnit.Framework;

namespace DistrEx.Coordinator.Test.Integration
{
    [TestFixture]
    public class Parallel7IntegrationTests
    {
        private TargetSpec _local;
        private Instruction<int, int> _identity;
        private Instruction<Tuple<int, int, int, int, int, int, int>, Tuple<int, int, int, int, int, int, int>> _identityTpl;
        private int _identityArgument;

        #region setup
        [SetUp]
        public void Setup()
        {
            _local = OnCoordinator.Default;
            _identity = (ct, p, i) => i;
            _identityTpl = (ct, p, tpl) => tpl;
            _identityArgument = 1;
        }
        #endregion

        [Test]
        public void Test()
        {
            var expected = _identityArgument;
            Tuple<Tuple<int, int, int, int, int, int, int>, Tuple<int, int, int, int, int, int, int>, int, int, int, int, int> result =
                Coordinator7.Do(
                    Coordinator.Do(_local.Do(_identity))
                               .ThenDo(_local.Do(_identity),
                                       _local.Do(_identity),
                                       _local.Do(_identity),
                                       _local.Do(_identity),
                                       _local.Do(_identity),
                                       _local.Do(_identity),
                                       _local.Do(_identity)),
                    Coordinator7.Do(_local.Do(_identity),
                                    _local.Do(_identity),
                                    _local.Do(_identity),
                                    _local.Do(_identity),
                                    _local.Do(_identity),
                                    _local.Do(_identity),
                                    _local.Do(_identity))
                                .ThenDo(_local.Do(_identityTpl)),
                    _local.Do(_identity),
                    _local.Do(_identity),
                    _local.Do(_identity),
                    _local.Do(_identity),
                    _local.Do(_identity),
                    _identityArgument)
                            .ResultValue;
            Assert.That(result.Item1.Item1, Is.EqualTo(expected));
            Assert.That(result.Item1.Item2, Is.EqualTo(expected));
            Assert.That(result.Item1.Item3, Is.EqualTo(expected));
            Assert.That(result.Item1.Item4, Is.EqualTo(expected));
            Assert.That(result.Item1.Item5, Is.EqualTo(expected));
            Assert.That(result.Item1.Item6, Is.EqualTo(expected));
            Assert.That(result.Item1.Item7, Is.EqualTo(expected));
            Assert.That(result.Item2.Item1, Is.EqualTo(expected));
            Assert.That(result.Item2.Item2, Is.EqualTo(expected));
            Assert.That(result.Item2.Item3, Is.EqualTo(expected));
            Assert.That(result.Item2.Item4, Is.EqualTo(expected));
            Assert.That(result.Item2.Item5, Is.EqualTo(expected));
            Assert.That(result.Item2.Item6, Is.EqualTo(expected));
            Assert.That(result.Item2.Item7, Is.EqualTo(expected));
            Assert.That(result.Item3, Is.EqualTo(expected));
            Assert.That(result.Item4, Is.EqualTo(expected));
            Assert.That(result.Item5, Is.EqualTo(expected));
            Assert.That(result.Item6, Is.EqualTo(expected));
            Assert.That(result.Item7, Is.EqualTo(expected));
        }
    }
}
