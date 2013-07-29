using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using DistrEx.Common.InstructionResult;
using NUnit.Framework;

namespace DistrEx.Common.Test
{
    [TestFixture]
    public class ObservableProgressingResultTest
    {
        private Func<Action, int, int> _operationSuccess;
        private Func<Action, Exception, Exception> _operationFail;

        private int _argumentSuccess;
        private Exception _argumentFail;

        #region setup
        [SetUp]
        public void Setup()
        {
            _operationSuccess = (sendHeartbeat, i) =>
                {
                    sendHeartbeat();
                    return i;
                };
            _operationFail = (sendHeartbeat, e) =>
                {
                    sendHeartbeat();
                    throw e;
                };
            _argumentSuccess = 1;
            _argumentFail = new Exception("Expected");
        }
        #endregion

        #region tests
        [Test]
        public void SuccessSequence()
        {
            IConnectableObservable<ProgressingResult<int>> observable = Observable.Create((IObserver<ProgressingResult<int>> observer) =>
                {
                    var result = _operationSuccess(() => observer.OnNext(Progress<int>.Default), _argumentSuccess);
                    observer.OnNext(new Result<int>(result));
                    observer.OnCompleted();
                    return Disposable.Empty;
                }).Publish();
            int progresscount = 0;
            int resultcount = 0;
            var progresses = observable.Where(res => res.IsProgress);
            progresses.Subscribe(_ => progresscount++);
            var results = observable.Where(res => res.IsResult).Replay();
            results.Subscribe(_ => resultcount++);
            results.Connect();
            observable.Connect();
            var finalResult = results.Last().ResultValue;

            var expected = _argumentSuccess;
            Assert.That(progresscount, Is.EqualTo(1));
            Assert.That(resultcount, Is.EqualTo(1));
            Assert.That(finalResult, Is.EqualTo(expected));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void FailSequence()
        {
            IConnectableObservable<ProgressingResult<Exception>> observable = Observable.Create((IObserver<ProgressingResult<Exception>> observer) =>
            {
                var result = _operationFail(() => observer.OnNext(Progress<Exception>.Default), _argumentFail);
                observer.OnNext(new Result<Exception>(result));
                observer.OnCompleted();
                return Disposable.Empty;
            }).Publish();
            int progresscount = 0;
            int resultcount = 0;
            var progresses = observable.Where(res => res.IsProgress);
            progresses.Subscribe(_ => progresscount++);
            var results = observable.Where(res => res.IsResult).Replay();
            results.Subscribe(_ => resultcount++);
            results.Connect();
            observable.Connect();
            
            var expected = _argumentFail;
            Assert.That(progresscount, Is.EqualTo(1));
            Assert.That(resultcount, Is.EqualTo(1));
        }

        #endregion
    }
}
