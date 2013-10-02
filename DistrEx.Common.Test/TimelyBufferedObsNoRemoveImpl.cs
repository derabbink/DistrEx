using System;
using DistrEx.Common.TimelyBufferedObs;

namespace DistrEx.Common.Test
{
    public class TimelyBufferedObsNoRemoveImpl<TResult> : TimelyBufferedObservable<TResult>
    {
        public TimelyBufferedObsNoRemoveImpl(IObservable<TResult> observable)
            : base(observable)
        {
        }

        protected override void RemoveFromBuffer(TimeStampedTResult<TResult> arg)
        {
        }
    }
}
