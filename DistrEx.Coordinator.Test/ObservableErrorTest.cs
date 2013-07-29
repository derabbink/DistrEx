using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using NUnit.Framework;

namespace DistrEx.Coordinator.Test
{
    [TestFixture]
    public class ObservableErrorTest
    {

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ErrorInReplaySubject()
        {
            Exception expected = new Exception("Expected");
            IObservable<Unit> obs = Observable.Create((IObserver<Unit> observer) =>
                {
                    observer.OnNext(Unit.Default);
                    observer.OnError(expected);
                    return Disposable.Empty;
                });
            ReplaySubject<Unit> rps = new ReplaySubject<Unit>();
            obs.Subscribe(rps);
            //Tease out exception
            var last = rps.Last();
        }

        [Test]
        [ExpectedException(typeof (Exception), ExpectedMessage = "Expected")]
        public void ErrorInAmb1()
        {
            var left = Observable.Throw<Unit>(new Exception("Expected"));
            var right = Observable.Never<Unit>();

            var both = left.Amb(right);
            //tease out exceptions
            var last = both.Last();
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ErrorInAmb2()
        {
            var left = Observable.Never<Unit>();
            var right = Observable.Throw<Unit>(new Exception("Expected"));

            var both = left.Amb(right);
            //tease out exceptions
            var last = both.Last();
        }

    }
}
