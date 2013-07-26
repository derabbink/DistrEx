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

    }
}
