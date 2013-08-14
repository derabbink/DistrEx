using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Common.Serialization;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Events;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Communication.Service.Executor;
using DistrEx.Plugin;

namespace DistrEx.Worker
{
    public class Executor : IDisposable
    {
        private readonly PluginManager _pluginManager;
        private readonly ExecutorService _executor;
        private readonly IDictionary<Guid, Future<SerializedResult>> _asyncResults;

        private readonly IObservable<ClearAsyncResultsEventArgs> _clearAsyncResults;
        private readonly IObservable<ExecuteEventArgs> _executes;
        private readonly IObservable<ExecuteAsyncEventArgs> _executeAsyncs;
        private readonly IObservable<GetAsyncResultEventArgs> _getAsyncResults;
        private readonly IObservable<CancelEventArgs> _cancels;

        private readonly IDisposable _clearAsyncResultsSubscription;
        private readonly IDisposable _executeSubscription;
        private readonly IDisposable _executeAsyncSubscription;
        private readonly IDisposable _executeGetAsyncResultSubscription;
        private readonly IDisposable _cancelsConnection;

        public Executor(ExecutorService executor, PluginManager pluginManager)
        {
            _asyncResults = new ConcurrentDictionary<Guid, Future<SerializedResult>>();
            
            _executor = executor;
            _clearAsyncResults = Observable.FromEventPattern<ClearAsyncResultsEventArgs>(_executor.SubscribeClearAsyncResults, _executor.UnsubscribeClearAsyncResults).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);
            _executes = Observable.FromEventPattern<ExecuteEventArgs>(_executor.SubscribeExecute, _executor.UnsubscribeExecute).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);
            _executeAsyncs = Observable.FromEventPattern<ExecuteAsyncEventArgs>(_executor.SubscribeExecuteAsync, _executor.UnsubscribeExecuteAsync).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);
            _getAsyncResults = Observable.FromEventPattern<GetAsyncResultEventArgs>(_executor.SubscribeGetAsyncResult, _executor.UnsubscribeGetAsyncResult).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);
            var cancels = Observable.FromEventPattern<CancelEventArgs>(_executor.SubscribeCancel, _executor.UnsubscribeCancel).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs).Replay();
            _cancelsConnection = cancels.Connect();
            _cancels = cancels;
            // TODO Need to clean out processed cancel messages

            _pluginManager = pluginManager;

            _clearAsyncResultsSubscription = _clearAsyncResults.Subscribe(_ => ClearAsyncResults());
            _executeSubscription = _executes.Subscribe(Execute);
            _executeAsyncSubscription = _executeAsyncs.Subscribe(ExecuteAsync);
            _executeGetAsyncResultSubscription = _getAsyncResults.Subscribe(GetAsyncResult);
        }

        private void Execute(ExecuteEventArgs instruction)
        {
            Guid operationId = instruction.OperationId;
            IExecutorCallback callback = instruction.CallbackChannel;
            var cts = new CancellationTokenSource();
            
            var cancelObs = _cancels.Where(eArgs => eArgs.OperationId == operationId);
            var cancelSubscription = cancelObs.ObserveOn(Scheduler.Default).Subscribe(_ => cts.Cancel());
            
            var progressMsg = new Progress
            {
                OperationId = operationId
            };
            Action reportProgress = () => callback.Progress(progressMsg);
            try
            {
                SerializedResult serializedResult = _pluginManager.Execute(instruction.AssemblyQualifiedName,
                                                                           instruction.MethodName, cts.Token,
                                                                           reportProgress, instruction.ArgumentTypeName,
                                                                           instruction.SerializedArgument);
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

        private void ExecuteAsync(ExecuteAsyncEventArgs asyncInstruction)
        {
            Guid asyncResultId = Guid.NewGuid();
            Guid operationId = asyncInstruction.OperationId;
            IExecutorCallback callback = asyncInstruction.CallbackChannel;
            var cts = new CancellationTokenSource();

            var cancelObs = _cancels.Where(eArgs => eArgs.OperationId == operationId);
            var cancelSubscription = cancelObs.ObserveOn(Scheduler.Default).Subscribe(_ => cts.Cancel());

            var progressMsg = new Progress
            {
                OperationId = operationId
            };
            Action sendProgress = () => callback.Progress(progressMsg);
            
            IObservable<Unit> step1CompletedObs = Observable.Create((IObserver<Unit> observer) =>
                {
                    observer.OnNext(Unit.Default); //bogus element required later
                    Action reportStep1Completed = observer.OnCompleted;

                    IConnectableObservable<ProgressingResult<SerializedResult>> executionObs = Observable.Create(
                        (IObserver<ProgressingResult<SerializedResult>> executionObserver) =>
                            {
                                var progress = Progress<SerializedResult>.Default;
                                Action reportProgress = () => executionObserver.OnNext(progress);

                                SerializedResult serializedResult =
                                    _pluginManager.ExecuteTwoStep(asyncInstruction.AssemblyQualifiedName,
                                                                asyncInstruction.MethodName, cts.Token,
                                                                reportProgress, reportStep1Completed,
                                                                asyncInstruction.ArgumentTypeName,
                                                                asyncInstruction.SerializedArgument);
                                executionObserver.OnNext(new Result<SerializedResult>(serializedResult));
                                executionObserver.OnCompleted();
                                return Disposable.Empty;
                            }).Publish();
                    var future = new Future<SerializedResult>(executionObs, cts.Cancel, ()=>{});
                    var errorSubscription = future.Subscribe(_ => { }, observer.OnError, () => { });
                    var progressSubscription = future.Where(pRes => pRes.IsProgress).Subscribe(_ => sendProgress());
                    
                    //store future in dict
                    _asyncResults.Add(asyncResultId, future);
                    executionObs.Connect();
                    return Disposable.Create(() =>
                        {
                            errorSubscription.Dispose();
                            progressSubscription.Dispose();
                        });
                });

            try
            {
                step1CompletedObs.Wait();
                var resultMsg = new Result()
                    {
                        OperationId = operationId,
                        ResultTypeName = typeof (Guid).FullName,
                        SerializedResult = Serializer.Serialize(asyncResultId)
                    };
                callback.Complete(resultMsg);
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

        private void GetAsyncResult(GetAsyncResultEventArgs getAsyncResultInstruction)
        {
            var operationId = getAsyncResultInstruction.OperationId;
            var asyncResultId = getAsyncResultInstruction.AsyncOperationId;
            IExecutorCallback callback = getAsyncResultInstruction.CallbackChannel;

            CancellationTokenSource cts = new CancellationTokenSource();

            Future<SerializedResult> future;
            if (_asyncResults.TryGetValue(asyncResultId, out future))
            {
                _asyncResults.Remove(asyncResultId);
                var cancelObs = _cancels.Where(eArgs => eArgs.OperationId == operationId);
                var cancelSubscription = cancelObs.ObserveOn(Scheduler.Default).Subscribe(_ => cts.Cancel());

                var progressMsg = new Progress
                    {
                        OperationId = operationId
                    };
                Action reportProgress = () => callback.Progress(progressMsg);
                var progressObs = future.Where(pRes => pRes.IsProgress);
                var progressSubscription = progressObs.Subscribe(_ => reportProgress());

                try
                {
                    var serializedResult = future.GetResult();
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
                    progressSubscription.Dispose();
                }
            }
        }

        public void ClearAsyncResults()
        {
            foreach(Future<SerializedResult> future in _asyncResults.Values)
                try
                {
                    future.Cancel();
                }
                catch (AggregateException e)
                {
                    Debug.WriteLine(e.Message);  
                }
            _asyncResults.Clear();
        }

        public void Dispose()
        {
            _clearAsyncResultsSubscription.Dispose();
            _executeSubscription.Dispose();
            _executeAsyncSubscription.Dispose();
            _executeGetAsyncResultSubscription.Dispose();
            _cancelsConnection.Dispose();
        }
    }
}
