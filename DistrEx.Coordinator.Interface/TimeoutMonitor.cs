using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public static class TimeoutMonitor
    {
        public static int DefaultTimeout = 10000;

        public static void MonitorTimeout<TResult>(Future<TResult> unmonitoredFuture)
        {
            MonitorTimeout(unmonitoredFuture, DefaultTimeout);
        }

        /// <summary>
        /// Cancels an operation if the corresponding future takes too long
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="unmonitoredFuture"></param>
        /// <param name="timeoutMs">number of milliseconds used for timeout</param>
        public static void MonitorTimeout<TResult>(Future<TResult> unmonitoredFuture, int timeoutMs)
        {
            unmonitoredFuture
                .Timeout(TimeSpan.FromMilliseconds(timeoutMs))
                .Subscribe(_=>{}, exception =>
                    {
                        if (exception is TimeoutException)
                            unmonitoredFuture.Cancel();
                    }, ()=>{});
        }
    }
}
