using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace DistrEx.Common.TimelyBufferedObs
{
    public class TimelyBufferedObservable<TResult> : IObservable<TResult>, IDisposable
    {
        private readonly IObservable<TResult> _observable;
        private readonly IList<TimeStampedTResult<TResult>> _buffer;
        private readonly IDisposable _subscription;

        private TimeSpan _timeOut = TimeSpan.FromSeconds(10);
        
        public TimelyBufferedObservable(IObservable<TResult> observable)
        {
            _buffer = new List<TimeStampedTResult<TResult>>();
            _observable = observable;
            _subscription = _observable.ObserveOn(Scheduler.Default).Subscribe(x => AddToBuffer(new TimeStampedTResult<TResult>(x, TimeSpanNow()))); 
        }
        
        public virtual void AddToBuffer(TimeStampedTResult<TResult> arg)
        {
            _buffer.Add(arg);
        }

        protected virtual void RemoveFromBuffer(TimeStampedTResult<TResult> arg)
        {
            _buffer.Remove(arg); 
        }

        private static TimeSpan TimeSpanNow()
        {
            return TimeSpan.FromTicks(DateTime.UtcNow.Ticks);
        }

        public virtual IList<TimeStampedTResult<TResult>> ResultBuffer
        {
            get
            {
                return _buffer;
            }
        }


        public TimeSpan TimeOut
        {
            get
            {
                return _timeOut; 
            }
            set
            {
                _timeOut = value; 
            }
        }

        public virtual IDisposable Subscribe(IObserver<TResult> observer)
        {
            var subscription = _observable.Subscribe(observer);

            foreach (TimeStampedTResult<TResult> timeStampedT in _buffer.ToArray())
            {
                TimeSpan difference = TimeSpanNow() - timeStampedT.TimeStamp;
                if (difference >= _timeOut)
                {
                    RemoveFromBuffer(timeStampedT);
                }
                else
                {
                    observer.OnNext(timeStampedT.CancelEvent);
                }
            }

            return Disposable.Create(subscription.Dispose);
        }
        
        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}
