using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
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
    }
}
