using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DistrEx.Common.TimelyBufferedObs;
using DistrEx.Communication.Contracts.Service;
using NUnit.Framework;
using DistrEx.Communication.Contracts.Events;
using TimelyBufferedObservable = DistrEx.Common.TimelyBufferedObs.TimelyBufferedObservable<DistrEx.Communication.Contracts.Events.CancelEventArgs>;
using TimelyBufferedNoRemoveObs = DistrEx.Common.Test.TimelyBufferedObsNoRemoveImpl<DistrEx.Communication.Contracts.Events.CancelEventArgs>;

namespace DistrEx.Common.Test
{
    [TestFixture]
    public class TimelyBufferedObsTest
    {
        private TimelyBufferedObservable observable;
        private TimelyBufferedNoRemoveObs observableNoRemove;
 
        private event EventHandler<CancelEventArgs> CancelRequest;

        private readonly Guid testGuid0 = new Guid("00000000-0000-0000-0000-000000000000");
        private readonly Guid testGuid1 = new Guid("00000000-0000-0000-0000-000000000001");
        private readonly Guid testGuid2 = new Guid("00000000-0000-0000-0000-000000000002");
        private readonly Guid testGuid3 = new Guid("00000000-0000-0000-0000-000000000003");

        private IDisposable subscription; 

        IExecutorCallback _callBack = null;

        protected virtual void OnCancelRequest(CancelEventArgs e)
        {
            CancelRequest.Raise(this, e);
        }

        [SetUp]
        public void Setup()
        {
            var cancels = Observable.FromEventPattern<CancelEventArgs>(SubscribeCancel, UnsubscribeCancel)
                          .ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);
            observable = new TimelyBufferedObservable(cancels);
            observableNoRemove = new TimelyBufferedNoRemoveObs(cancels);
            
            RegisterTheCancelRequest(); 
        }

        private void RegisterTheCancelRequest()
        {
            OnCancelRequest(new CancelEventArgs(testGuid0, _callBack));
            OnCancelRequest(new CancelEventArgs(testGuid1, _callBack));
            OnCancelRequest(new CancelEventArgs(testGuid2, _callBack));
            OnCancelRequest(new CancelEventArgs(testGuid3, _callBack));
        }

        private void UnsubscribeCancel(EventHandler<CancelEventArgs> handler)
        {
            CancelRequest -= handler; 
        }

        private void SubscribeCancel(EventHandler<CancelEventArgs> handler)
        {
            CancelRequest += handler;
        }

        [Test]
        public void SuccessfulRequest()
        {
            var guid = new Guid();

            IObservable<CancelEventArgs> cancelObs = observable.Where(args => args.OperationId == testGuid1);
            subscription = cancelObs.Subscribe(args =>
            {
                guid = args.OperationId;
            });
            
            Assert.AreEqual(testGuid1, guid);
            Assert.AreEqual(4, observable.ResultBuffer.Count);
        }

        [Test]
        public void TimeOutTest()
        {
            var guid = new Guid();
            observable.TimeOut = TimeSpan.FromSeconds(0);

            IObservable<CancelEventArgs> cancelObs = observable.Where(args => args.OperationId == testGuid2);
            subscription = cancelObs.Subscribe(args =>
            {
                guid = args.OperationId;
            });

            Assert.AreEqual(testGuid0, guid);
            Assert.AreEqual(0, observable.ResultBuffer.Count);
        }

        [Test]
        public void TimeOutWithNoRemoveTest()
        {
            var guid = new Guid();
            observableNoRemove.TimeOut = TimeSpan.FromSeconds(0);

            IObservable<CancelEventArgs> cancelObs = observableNoRemove.Where(args => args.OperationId == testGuid2);
            subscription = cancelObs.Subscribe(args =>
            {
                guid = args.OperationId;
            });

            Assert.AreEqual(testGuid0, guid);
            Assert.AreEqual(4, observableNoRemove.ResultBuffer.Count);
        }

        [Test]
        public void RemovedTimedOutMessageTest()
        {
            var testGuid4 = new Guid("00000000-0000-0000-0000-000000000004");
            var cancelEvt = new CancelEventArgs(testGuid4, _callBack);

            observable.TimeOut = TimeSpan.FromSeconds(10);
            observable.AddToBuffer(new TimeStampedTResult<CancelEventArgs>(cancelEvt, TimeSpan.FromTicks(DateTime.UtcNow.Ticks) - TimeSpan.FromSeconds(60)));
            
            Guid guid = new Guid();

            IObservable<CancelEventArgs> cancelObs = observable.Where(args => args.OperationId == testGuid2);
            subscription = cancelObs.Subscribe(args =>
            {
                guid = args.OperationId;
            });

            Assert.AreEqual(testGuid2, guid);
            Assert.AreEqual(4, observable.ResultBuffer.Count);
        }

        [TearDown]
        public void TearDown()
        {
            subscription.Dispose();
        }
    }
}
