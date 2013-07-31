using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Events;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Plugin;

namespace DistrEx.Worker
{
    public class Executor : IDisposable
    {
        private readonly PluginManager _pluginManager;
        private readonly IExecutor _executor;

        private readonly IObservable<ExecuteEventArgs> _executes;
        private readonly IObservable<CancelEventArgs> _cancels;

        private readonly IDisposable _executeSubscription;

        public Executor(IExecutor executor, PluginManager pluginManager)
        {
            _executor = executor;
            _executes = Observable.FromEventPattern<ExecuteEventArgs>(_executor.SubscribeExecute, _executor.UnsubscribeExecute).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);
            _cancels = Observable.FromEventPattern<CancelEventArgs>(_executor.SubscribeCancel, _executor.UnsubscribeCancel).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);

            _pluginManager = pluginManager;

            _executeSubscription = _executes.Subscribe(Execute);
        }

        private void Execute(ExecuteEventArgs instruction)
        {
            Guid operationId = instruction.OperationId;
            IExecutorCallback callback = instruction.CallbackChannel;
            var cts = new CancellationTokenSource();
            
            var cancelObs = _cancels.Where(eArgs => eArgs.OperationId == operationId);
            var cancelSubscription = cancelObs.Subscribe(_ => cts.Cancel());
            
            var progressMsg = new Progress
            {
                OperationId = operationId
            };
            Action reportProgress = () => callback.Progress(progressMsg);
            try
            {
                SerializedResult serializedResult = _pluginManager.Execute(instruction.AssemblyQualifiedName, instruction.MethodName, cts.Token,
                                                                           reportProgress, instruction.ArgumentTypeName, instruction.SerializedArgument);
                callback.Complete(new Result
                {
                    OperationId = operationId,
                    ResultTypeName = serializedResult.TypeName,
                    SerializedResult = serializedResult.Value
                });
            }
            catch (ExecutionException e)
            {
                var msg = new Error
                {
                    OperationId = operationId,
                    ExceptionTypeName = e.InnerExceptionTypeName,
                    SerializedException = e.SerializedInnerException
                };
                callback.Error(msg);
            }
            finally
            {
                cancelSubscription.Dispose();
            }
        }

        public void Dispose()
        {
            _executeSubscription.Dispose();
        }
    }
}
