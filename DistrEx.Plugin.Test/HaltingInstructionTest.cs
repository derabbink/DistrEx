using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DistrEx.Common;
using NUnit.Framework;

namespace DistrEx.Plugin.Test
{
    [TestFixture]
    public class HaltingInstructionTest
    {
        private Instruction<int, int> _haltingIdentity;

        #region setup
        [SetUp]
        public void Setup()
        {
            _haltingIdentity = (ct, p, i) =>
                {
                    p();
                    ManualResetEventSlim block = new ManualResetEventSlim(false);
                    block.Wait(ct);
                    p();
                    return i;
                };
        }
        #endregion


        #region tests
        [Test]
        public void Test()
        {
            var argument = 1;
            CancellationTokenSource cts = new CancellationTokenSource();
            ManualResetEventSlim progressBlock = new ManualResetEventSlim(false);
            Action progress = progressBlock.Set;
            Task<int> t = Task<int>.Factory.StartNew(() =>
                {
                    var result = _haltingIdentity(cts.Token, progress, argument);
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
        #endregion
    }
}
