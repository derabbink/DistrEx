using System;

namespace DistrEx.Common.TimelyBufferedObs
{
    public class TimeStampedTResult<T>
    {
        private readonly T _T;
        private readonly TimeSpan _timeStamp;

        public TimeStampedTResult(T cancelEvent, TimeSpan timestamp)
        {
            _T = cancelEvent;
            _timeStamp = timestamp;
        }

        public T CancelEvent
        {
            get
            {
                return _T;
            }
        }

        public TimeSpan TimeStamp
        {
            get
            {
                return _timeStamp;
            }
        }
    }
}