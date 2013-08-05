
using DistrEx.Common;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.TargetSpecs;
using NUnit.Framework;

namespace DistrEx.Coordinator.Test.Sequential
{
    [TestFixture]
    public class NestedSequentialTest
    {
        private TargetSpec _local;
        private Instruction<int, int> _inc;
        private int _incArgument;

        #region setup
        [SetUp]
        public void Setup()
        {
            _local = OnCoordinator.Default;
            _inc = (ct, p, i) => i + 1;
            _incArgument = 0;
        }
        #endregion

        [Test]
        public void Chain([Values(0, 1, 2, 5, 10)] int chainLength)
        {
            var chain = Coordinator.Do(_local.Do<int, int>((ct, p, a) => a));
            for (int i = 0; i < chainLength; i++)
            {
                chain = chain.ThenDo(_local.Do(_inc));
            }
            var res = Coordinator.Do(chain, _incArgument);
            
            Assert.That(res.ResultValue, Is.EqualTo(chainLength));
        }
    }
}
