using System;
using System.Reactive.Linq;

namespace DistrEx.Common
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
            MonitorTimeout(unmonitoredFuture, unmonitoredFuture.Cancel, timeoutMs);
        }

        /// <summary>
        /// monitors an observable for a timeout, and takes action if it appears.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"></param>
        /// <param name="doUponTimeout">action to take upon timeout</param>
        /// <param name="timeoutMs"></param>
        public static void MonitorTimeout<T>(IObservable<T> observable, Action doUponTimeout, int timeoutMs)
        {
            observable.Timeout(TimeSpan.FromMilliseconds(timeoutMs))
                      .Subscribe(_ => { }, exception =>
                          {
                              if (exception is TimeoutException)
                                  doUponTimeout();
                          }, () => { });
        }
    }
}
