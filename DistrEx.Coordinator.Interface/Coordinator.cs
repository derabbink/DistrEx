using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public class Coordinator
    {
        public static CompletedStep<TResult> Do<TArgument, TResult>(
            TargetedInstruction<TArgument, TResult> targetedInstruction, TArgument argument)
        {
            Future<TResult> future = InvokeAsync(targetedInstruction, argument).Last();
            var result = future.GetResult();
            return new CompletedStep<TResult>(result);
        }

        public static IObservable<Future<TResult>> InvokeAsync<TArgument, TResult>(
            TargetedInstruction<TArgument, TResult> targetedInstruction, TArgument argument)
        {
            var result = Observable.Create((IObserver<Future<TResult>> observer) =>
                {
                    targetedInstruction.TransportAssemblies();
                    Future<TResult> future = targetedInstruction.Invoke(argument);
                    TimeoutMonitor.MonitorTimeout(future);
                    observer.OnNext(future);
                    observer.OnCompleted();
                    return Disposable.Empty;
                })
                .SubscribeOn(Scheduler.Default);
            return result;
        }
    }
}
