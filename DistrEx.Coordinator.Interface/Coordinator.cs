using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DistrEx.Common;
using DistrEx.Coordinator.Interface.TargetedInstructions;

namespace DistrEx.Coordinator.Interface
{
    public static class Coordinator
    {
        public static CompletedStep<TResult> Do<TArgument, TResult>(
            TargetedInstruction<TArgument, TResult> targetedInstruction, TArgument argument)
        {
            targetedInstruction.TransportAssemblies();
            Future<TResult> future = GetInvocationFuture(targetedInstruction, argument).Last();
            TimeoutMonitor.MonitorTimeout(future);
            var result = future.GetResult();
            return new CompletedStep<TResult>(result);
        }

        public static IObservable<Future<TResult>> GetInvocationFuture<TArgument, TResult>(
            TargetedInstruction<TArgument, TResult> targetedInstruction, TArgument argument)
        {
            var result = Observable.Create((IObserver<Future<TResult>> observer) =>
                {
                    Future<TResult> future = targetedInstruction.Invoke(argument);
                    TimeoutMonitor.MonitorTimeout(future);
                    observer.OnNext(future);
                    observer.OnCompleted();
                    return Disposable.Empty;
                });
            return result;
        }
    }
}
