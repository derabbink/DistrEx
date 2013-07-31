using System;
using System.Threading;
using System.Threading.Tasks;
using DistrEx.Common;
using NUnit.Framework;

namespace DistrEx.Plugin.Test
{
    [TestFixture]
    public class HaltingInstructionTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            //need explicit types: http://stackoverflow.com/questions/4466859
            _haltingIdentity = (CancellationToken ct, Action p, int i) =>
            {
                p();
                var block = new ManualResetEventSlim(false);
                block.Wait(ct);
                p();
                return i;
            };
        }

        #endregion

        private Instruction<int, int> _haltingIdentity;

        [Test]
        public void Test()
        {
            int argument = 1;
            var cts = new CancellationTokenSource();
            var progressBlock = new ManualResetEventSlim(false);
            Action progress = progressBlock.Set;
            Task<int> t = Task<int>.Factory.StartNew(() =>
            {
                int result = _haltingIdentity(cts.Token, progress, argument);
                return result;
            });
            progressBlock.Wait();
            progressBlock.Reset();
            cts.Cancel();
            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                Assert.That(e.InnerException, Is.TypeOf<OperationCanceledException>());
            }
            Assert.That(progressBlock.IsSet, Is.False);
        }
    }
}
