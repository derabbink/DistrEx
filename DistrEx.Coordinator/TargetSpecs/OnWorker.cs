using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using DependencyResolver;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Common.Serialization;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Events;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Communication.Proxy;
using DistrEx.Coordinator.InstructionSpecs;
using DistrEx.Coordinator.Interface;

namespace DistrEx.Coordinator.TargetSpecs
{
    public class OnWorker : TargetSpec
    {
        private readonly Client<IAssemblyManager> _assemblyManagerClient;
        private readonly IExecutorCallback _callbackHandler;
        private readonly IObservable<CompleteCallbackEventArgs> _completes;
        private readonly IObservable<ErrorCallbackEventArgs> _errors;

        private readonly DuplexClient<IExecutor> _executorClient;
        private readonly IObservable<ProgressCallbackEventArgs> _progresses;
        private readonly ISet<AssemblyName> _transportedAssemblies;

        private OnWorker(string assemblyManagerEndpointConfigName, string executorEndpointConfigName, IExecutorCallback callbackHandler)
        {
            _callbackHandler = callbackHandler;
            _progresses = Observable.FromEventPattern<ProgressCallbackEventArgs>(_callbackHandler.SubscribeProgress, _callbackHandler.UnsubscribeProgress).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);
            _completes = Observable.FromEventPattern<CompleteCallbackEventArgs>(_callbackHandler.SubscribeComplete, _callbackHandler.UnsubscribeComplete).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);
            _errors = Observable.FromEventPattern<ErrorCallbackEventArgs>(_callbackHandler.SubscribeError, _callbackHandler.UnsubscribeError).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);

            var assemblyManagerFactory = new ClientFactory<IAssemblyManager>(assemblyManagerEndpointConfigName);
            _assemblyManagerClient = assemblyManagerFactory.GetClient();

            var executorFactory = new DuplexClientFactory<IExecutor, IExecutorCallback>(executorEndpointConfigName, _callbackHandler);
            _executorClient = executorFactory.GetClient();

            _transportedAssemblies = new HashSet<AssemblyName>();
        }

        private IAssemblyManager AssemblyManager
        {
            get
            {
                return _assemblyManagerClient.Channel;
            }
        }

        private IExecutor Executor
        {
            get
            {
                return _executorClient.Channel;
            }
        }

        public static OnWorker FromEndpointConfigNames(string assemblyManagerEndpointConfigName, string executorEndpointConfigName, IExecutorCallback callbackHandler)
        {
            return new OnWorker(assemblyManagerEndpointConfigName, executorEndpointConfigName, callbackHandler);
        }

        public override void TransportAssemblies(Spec instructionSpec)
        {
            Assembly assy = instructionSpec.GetAssembly();
            IObservable<AssemblyName> dependencies = Resolver.GetAllDependencies(assy.GetName())
                                                             .Where(aName => !_transportedAssemblies.Contains(aName));
            dependencies.Subscribe(TransportAssembly);
        }

        public override void TransportAssembly(AssemblyName assemblyName)
        {
            String path = new Uri(assemblyName.CodeBase).LocalPath;
            using (Stream assyFileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var msg = new Communication.Contracts.Message.Assembly
                {
                    AssemblyStream = assyFileStream,
                    Name = assemblyName.Name,
                    FullName = assemblyName.FullName
                };
                AssemblyManager.AddAssembly(msg);
            }
            _transportedAssemblies.Add(assemblyName);
        }

        public override bool AssemblyIsTransported(AssemblyName assembly)
        {
            return _transportedAssemblies.Contains(assembly);
        }

        protected override void ClearAsyncResults()
        {
            Executor.ClearAsyncResults();
        }

        protected override void ClearAssemblies()
        {
            AssemblyManager.Clear();
        }

        protected override InstructionSpec<TArgument, TResult> CreateInstructionSpec<TArgument, TResult>(Instruction<TArgument, TResult> instruction)
        {
            return TransferrableDelegateInstructionSpec<TArgument, TResult>.Create(instruction);
        }

        protected override AsyncInstructionSpec<TArgument, TResult> CreateAsyncInstructionSpec<TArgument, TResult>(TwoPartInstruction<TArgument, TResult> instruction)
        {
            return new TwoPartInstructionSpec<TArgument, TResult>(instruction); 
        }

        public override Future<TResult> Invoke<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction, TArgument argument)
        {
            string methodName = instruction.GetMethodName();
            string assemblyQualifiedName = instruction.GetAssemblyQualifiedName();

            Guid operationId = Guid.NewGuid();
            IObservable<Progress<TResult>> progressObs = _progresses
                .Where(eArgs => eArgs.OperationId == operationId)
                .Select(_ => Progress<TResult>.Default);
            IObservable<ProgressingResult<TResult>> resultObs = _completes
                .Where(eArgs => eArgs.OperationId == operationId)
                .Select(eArgs =>
                {
                    object result = eArgs.Result;
                    try
                    {
                        var castRes = (TResult) result;
                        return new Result<TResult>(castRes);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(
                            String.Format(
                                "Casting result (of type {0}) from method {1} in type {2} to type {3} failed.",
                                result.GetType().FullName, methodName, assemblyQualifiedName,
                                typeof(TResult).FullName),
                            e);
                    }
                });
            IObservable<ProgressingResult<TResult>> errorObs = _errors
                .Where(eArgs => eArgs.OperationId == operationId)
                .Select<ErrorCallbackEventArgs, ProgressingResult<TResult>>(eArgs =>
                {
                    throw eArgs.Error;
                });

            //send out instruction...
            IConnectableObservable<ProgressingResult<TResult>> resultOrErrorObs =
                resultObs.Amb(errorObs).Replay(Scheduler.Default);
            resultOrErrorObs.Connect();
            string serializedArgument = Serializer.Serialize(argument);
            var msg = new Instruction
            {
                OperationId = operationId,
                AssemblyQualifiedName = assemblyQualifiedName,
                MethodName = methodName,
                ArgumentTypeName = argument.GetType().FullName,
                SerializedArgument = serializedArgument
            };
            Executor.Execute(msg);

            //this collects the instruction result
            IObservable<IObservable<ProgressingResult<TResult>>> resultMetaObs = Observable.Create((
                IObserver<IObservable<ProgressingResult<TResult>>> obs) =>
            {
                IObservable<ProgressingResult<TResult>> combinedObs;
                //wait here: (First() blocks)
                try
                {
                    //might throw
                    ProgressingResult<TResult> resultOrError = resultOrErrorObs.First();
                    combinedObs = Observable.Return(resultOrError);
                }
                catch (Exception e)
                {
                    combinedObs = Observable.Throw<ProgressingResult<TResult>>(e);
                }
                obs.OnNext(combinedObs);
                obs.OnCompleted();
                return Disposable.Empty;
            });

            IObservable<IObservable<ProgressingResult<TResult>>> metaObs = Observable.Return(progressObs);
            IObservable<ProgressingResult<TResult>> futureObs = metaObs.Concat(resultMetaObs).Switch();
            return new Future<TResult>(futureObs, () => Cancel(operationId));
        }

        private void Cancel(Guid operationId)
        {
            Cancellation msg = new Cancellation(){OperationId = operationId};
            Executor.Cancel(msg);
        }

        public override Future<Guid> InvokeAsync<TArgument, TResult>(AsyncInstructionSpec<TArgument, TResult> asyncInstruction, TArgument argument)
        {
            string methodName = asyncInstruction.GetMethodName();
            string assemblyQualifiedName = asyncInstruction.GetAssemblyQualifiedName();

            Guid operationId = Guid.NewGuid();
            IObservable<Progress<Guid>> progressObs = _progresses
                .Where(eArgs => eArgs.OperationId == operationId)
                .Select(_ => Progress<Guid>.Default);
            IObservable<ProgressingResult<Guid>> resultObs = _completes
                .Where(eArgs => eArgs.OperationId == operationId)
                .Select(eArgs =>
                {
                    object result = eArgs.Result;
                    try
                    {
                        var castRes = (Guid)result;
                        return new Result<Guid>(castRes);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(
                            String.Format(
                                "Casting result (of type {0}) from method {1} in type {2} to type {3} failed.",
                                result.GetType().FullName, methodName, assemblyQualifiedName,
                                typeof(Guid).FullName),
                            e);
                    }
                });
            IObservable<ProgressingResult<Guid>> errorObs = _errors
                .Where(eArgs => eArgs.OperationId == operationId)
                .Select<ErrorCallbackEventArgs, ProgressingResult<Guid>>(eArgs =>
                {
                    throw eArgs.Error;
                });

            //send out instruction...
            IConnectableObservable<ProgressingResult<Guid>> resultOrErrorObs =
                resultObs.Amb(errorObs).Replay(Scheduler.Default);
            resultOrErrorObs.Connect();
            string serializedArgument = Serializer.Serialize(argument);

            var msg = new AsyncInstruction
            {
                OperationId = operationId,
                AssemblyQualifiedName = assemblyQualifiedName,
                MethodName = methodName,
                ArgumentTypeName = argument.GetType().FullName,
                SerializedArgument = serializedArgument
            };

            Executor.ExecuteAsync(msg);

            //this collects the instruction result
            IObservable<IObservable<ProgressingResult<Guid>>> resultMetaObs = Observable.Create((
                IObserver<IObservable<ProgressingResult<Guid>>> obs) =>
            {
                IObservable<ProgressingResult<Guid>> combinedObs;
                //wait here: (First() blocks)
                try
                {
                    //might throw
                    ProgressingResult<Guid> resultOrError = resultOrErrorObs.First();
                    combinedObs = Observable.Return(resultOrError);
                }
                catch (Exception e)
                {
                    combinedObs = Observable.Throw<ProgressingResult<Guid>>(e);
                }
                obs.OnNext(combinedObs);
                obs.OnCompleted();
                return Disposable.Empty;
            });

            IObservable<IObservable<ProgressingResult<Guid>>> metaObs = Observable.Return(progressObs);
            IObservable<ProgressingResult<Guid>> futureObs = metaObs.Concat(resultMetaObs).Switch();
            return new Future<Guid>(futureObs, () => Cancel(operationId));
        }

        public override Future<TResult> InvokeGetAsyncResult<TResult>(Guid asyncOperationId)
        {
            Guid operationId = Guid.NewGuid();
            IObservable<Progress<TResult>> progressObs = _progresses
                .Where(eArgs => eArgs.OperationId == operationId)
                .Select(_ => Progress<TResult>.Default);
            IObservable<ProgressingResult<TResult>> resultObs = _completes
                .Where(eArgs => eArgs.OperationId == operationId)
                .Select(eArgs =>
                {
                    object result = eArgs.Result;
                    try
                    {
                        var castRes = (TResult)result;
                        return new Result<TResult>(castRes);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(
                            String.Format(
                                "Casting result (of type {0}) from getting async result {1} to type {2} failed.",
                                result.GetType().FullName, asyncOperationId, typeof(TResult).FullName),
                            e);
                    }
                });
            IObservable<ProgressingResult<TResult>> errorObs = _errors
                .Where(eArgs => eArgs.OperationId == operationId)
                .Select<ErrorCallbackEventArgs, ProgressingResult<TResult>>(eArgs =>
                {
                    throw eArgs.Error;
                });

            //send out instruction...
            IConnectableObservable<ProgressingResult<TResult>> resultOrErrorObs =
                resultObs.Amb(errorObs).Replay(Scheduler.Default);
            resultOrErrorObs.Connect();
            
            var msg = new GetAsyncResultInstruction
            {
                OperationId = operationId,
                AsyncOperationId = asyncOperationId
            };

            Executor.GetAsyncResult(msg);

            //this collects the instruction result
            IObservable<IObservable<ProgressingResult<TResult>>> resultMetaObs = Observable.Create((
                IObserver<IObservable<ProgressingResult<TResult>>> obs) =>
            {
                IObservable<ProgressingResult<TResult>> combinedObs;
                //wait here: (First() blocks)
                try
                {
                    //might throw
                    ProgressingResult<TResult> resultOrError = resultOrErrorObs.First();
                    combinedObs = Observable.Return(resultOrError);
                }
                catch (Exception e)
                {
                    combinedObs = Observable.Throw<ProgressingResult<TResult>>(e);
                }
                obs.OnNext(combinedObs);
                obs.OnCompleted();
                return Disposable.Empty;
            });

            IObservable<IObservable<ProgressingResult<TResult>>> metaObs = Observable.Return(progressObs);
            IObservable<ProgressingResult<TResult>> futureObs = metaObs.Concat(resultMetaObs).Switch();
            return new Future<TResult>(futureObs, () => Cancel(operationId));
        }
    }
}
