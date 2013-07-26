using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DistrEx.Coordinator.Test
{
    [TestFixture]
    public class ObservableSwitchTest
    {
        private ISubject<int> _heartbeats;
        private IObservable<IObservable<int>> _resultMeta;
        private Action<int> _sendHeartbeat;
        private Func<int, int> _operation;
        private int _argument;
        private IObservable<int> _final;

        #region setup

        [SetUp]
        public void Setup()
        {
            _argument = 1;
            _heartbeats = new Subject<int>();
            _sendHeartbeat = _heartbeats.OnNext;
            _operation = i =>
                {
                    _sendHeartbeat(-i);
                    return i;
                };
            _resultMeta = Observable.Create((IObserver<IObservable<int>> obs) =>
                {
                    var result = _operation(_argument);
                    obs.OnNext(Observable.Return(result));
                    obs.OnCompleted();
                    return Disposable.Empty;
                });
            _final = Observable.Return(_heartbeats).Concat(_resultMeta).Switch();
        }

        #endregion

        #region tests

        [Test]
        public void CorrectSequence()
        {
            int expected = _argument;
            var actual = _final.ToEnumerable().ToList();
            Assert.That(actual.ElementAt(0), Is.EqualTo(-expected));
            Assert.That(actual.ElementAt(1), Is.EqualTo(expected));
        }

        [Test]
        public void SequenceCompletes()
        {
            bool completed = false;
            _final.Subscribe(_ => { }, () => { completed = true; });
            Assert.That(completed, Is.True);
        }

        [Test]
        public void OneExecutionMultipleSubscribers()
        {
            var executions = 0;
            Action inc = () => executions++;
            ManualResetEventSlim block = new ManualResetEventSlim(false);
            IObservable<Unit> heartbeats = Observable.Create((IObserver<Unit> obs) =>
                {
                    obs.OnNext(Unit.Default);
                    block.Wait();
                    inc();
                    return Disposable.Empty;
                });
            IObservable<IObservable<Unit>> finalMetaObs = Observable.Create((IObserver<IObservable<Unit>> obs) =>
                {
                    var res = Observable.Return(Unit.Default);
                    obs.OnNext(res);
                    obs.OnCompleted();
                    return Disposable.Empty;
                });
            //Publish() and Connect() is the key to multiple subscribers
            var final = Observable.Return(heartbeats).Concat(finalMetaObs).Switch().Publish();
            int units1 = 0;
            int units2 = 0;
            final.Subscribe(u => units1++);
            final.Subscribe(u => units2++);
            
            block.Set();
            final.Connect();
            
            Assert.That(units1, Is.EqualTo(2));
            Assert.That(units2, Is.EqualTo(2));
            Assert.That(executions, Is.EqualTo(1));
        }

        #endregion
    }
}
