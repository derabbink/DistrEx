using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DistrEx.Common.InstructionResult;

namespace DistrEx.Common
{
    public class Future<TResult> : IObservable<ProgressingResult<TResult>>
    {
        private readonly IConnectableObservable<ProgressingResult<TResult>> _observable;
        private readonly IConnectableObservable<Result<TResult>> _replayResult;
        private Action _cancelOperation;
        
        public Future(IObservable<ProgressingResult<TResult>> observable, Action cancelOperation)
        {
            _cancelOperation = cancelOperation;
            //subscribe triggers waiting for result
            _observable = observable.SubscribeOn(Scheduler.Default).Publish();
            IObservable<Result<TResult>> resultObs = _observable.Where(pr => pr.IsResult).Select(r => r as Result<TResult>);
            _replayResult = resultObs.Replay();
            _replayResult.Connect();

            _observable.Connect();
        }

        public IDisposable Subscribe(IObserver<ProgressingResult<TResult>> observer)
        {
            return _observable.Subscribe(observer);
        }

        public void Cancel()
        {
            _cancelOperation();
            //you can cancel just once
            _cancelOperation = () => {};
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">If there was an error in the operation</exception>
        public TResult GetResult()
        {
            //Last() will throw if there was an error
            Result<TResult> result = _replayResult.Last();
            return result.ResultValue;
        }
    }
}
