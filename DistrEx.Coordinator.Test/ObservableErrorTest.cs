using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NUnit.Framework;

namespace DistrEx.Coordinator.Test
{
    [TestFixture]
    public class ObservableErrorTest
    {
        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ErrorInAmb1()
        {
            IObservable<Unit> left = Observable.Throw<Unit>(new Exception("Expected"));
            IObservable<Unit> right = Observable.Never<Unit>();

            IObservable<Unit> both = left.Amb(right);
            //tease out exceptions
            Unit last = both.Last();
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ErrorInAmb2()
        {
            IObservable<Unit> left = Observable.Never<Unit>();
            IObservable<Unit> right = Observable.Throw<Unit>(new Exception("Expected"));

            IObservable<Unit> both = left.Amb(right);
            //tease out exceptions
            Unit last = both.Last();
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ErrorInReplaySubject()
        {
            var expected = new Exception("Expected");
            IObservable<Unit> obs = Observable.Create((IObserver<Unit> observer) =>
            {
                observer.OnNext(Unit.Default);
                observer.OnError(expected);
                return Disposable.Empty;
            });
            var rps = new ReplaySubject<Unit>();
            obs.Subscribe(rps);
            //Tease out exception
            Unit last = rps.Last();
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ErrorInReplayedObservable()
        {
            IObservable<Unit> throws = Observable.Throw<Unit>(new Exception("Expected"));
            IConnectableObservable<Unit> replayed = throws.Replay();
            replayed.Connect();

            //tease out exception
            Unit last = replayed.Last();
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ErrorInReplayedObservableOnScheduler()
        {
            IObservable<Unit> throws = Observable.Throw<Unit>(new Exception("Expected"));
            IConnectableObservable<Unit> replayed = throws.Replay(Scheduler.Default);
            replayed.Connect();

            //tease out exception
            Unit last = replayed.Last();
        }
    }
}
