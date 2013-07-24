using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using DistrEx.Common.InstructionResult;

namespace DistrEx.Common
{
    public class Future<TResult> : IObservable<ProgressingResult<TResult>>
    {
        private readonly IObservable<ProgressingResult<TResult>> _observable;
        private readonly ReplaySubject<Result<TResult>> _replayResult;

        public Future(IObservable<ProgressingResult<TResult>> observable)
        {
            _observable = observable;
            _replayResult = new ReplaySubject<Result<TResult>>();
            _observable.Where(pr => pr.IsResult).Select(r => r as Result<TResult>).Subscribe(_replayResult);
        }

        public IDisposable Subscribe(IObserver<ProgressingResult<TResult>> observer)
        {
            return _observable.Subscribe(observer);
        }

        public TResult GetResult()
        {
            var result = _replayResult.Last();
            return result.ResultValue;
        }
    }
}
