using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Threading;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Common.Serialization;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Events;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Communication.Proxy;
using DistrEx.Coordinator.InstructionSpecs;
using DistrEx.Coordinator.Interface;
using DependencyResolver;

namespace DistrEx.Coordinator.TargetSpecs
{
    public class OnWorker : TargetSpec
    {
        private readonly ISet<AssemblyName> _transportedAssemblies;

        private readonly IExecutorCallback _callbackHandler;
        private readonly IObservable<ProgressCallbackEventArgs> _progresses;
        private readonly IObservable<CompleteCallbackEventArgs> _completes;
        private readonly IObservable<ErrorCallbackEventArgs> _errors;

        private readonly Client<IAssemblyManager> _assemblyManagerClient;
        private IAssemblyManager AssemblyManager { get { return _assemblyManagerClient.Channel; } }
        private readonly DuplexClient<IExecutor> _executorClient;
        private IExecutor Executor { get { return _executorClient.Channel; } }

        private OnWorker(string assemblyManagerEndpointConfigName, string executorEndpointConfigName, IExecutorCallback callbackHandler)
        {
            _callbackHandler = callbackHandler;
            _progresses = Observable.FromEventPattern<ProgressCallbackEventArgs>(_callbackHandler.SubscribeProgress, _callbackHandler.UnsubscribeProgress).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);
            _completes = Observable.FromEventPattern<CompleteCallbackEventArgs>(_callbackHandler.SubscribeComplete, _callbackHandler.UnsubscribeComplete).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);
            _errors = Observable.FromEventPattern<ErrorCallbackEventArgs>(_callbackHandler.SubscribeError, _callbackHandler.UnsubscribeError).ObserveOn(Scheduler.Default).Select(ePattern => ePattern.EventArgs);

            ClientFactory<IAssemblyManager> assemblyManagerFactory = new ClientFactory<IAssemblyManager>(assemblyManagerEndpointConfigName);
            _assemblyManagerClient = assemblyManagerFactory.GetClient();

            DuplexClientFactory<IExecutor, IExecutorCallback> executorFactory = new DuplexClientFactory<IExecutor, IExecutorCallback>(executorEndpointConfigName, _callbackHandler);
            _executorClient = executorFactory.GetClient();
            
            _transportedAssemblies = new HashSet<AssemblyName>();
        }

        public static OnWorker FromEndpointConfigNames(string assemblyManagerEndpointConfigName, string executorEndpointConfigName, IExecutorCallback callbackHandler)
        {
            return new OnWorker(assemblyManagerEndpointConfigName, executorEndpointConfigName, callbackHandler);
        }

        public override void TransportAssemblies<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction)
        {
            Assembly assy = instruction.GetAssembly();
            IObservable<AssemblyName> dependencies = Resolver.GetAllDependencies(assy.GetName())
                    .Where(aName => !_transportedAssemblies.Contains(aName));
            dependencies.Subscribe(TransportAssembly);
        }

        private void TransportAssembly(AssemblyName assemblyName)
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

        public override void ClearAssemblies()
        {
            AssemblyManager.Clear();
        }

        protected override InstructionSpec<TArgument, TResult> CreateInstructionSpec<TArgument, TResult>(Instruction<TArgument, TResult> instruction)
        {
            return TransferrableDelegateInstructionSpec<TArgument, TResult>.Create(instruction);
        }

        public override Future<TResult> Invoke<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction, CancellationToken cancellationToken, TArgument argument)
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
                            TResult castRes = (TResult) result;
                            return new Result<TResult>(castRes);
                        }
                        catch (Exception e)
                        {
                            throw new Exception(
                                String.Format(
                                    "Casting result (of type {0}) from method {1} in type {2} to type {3} failed.",
                                    result.GetType().FullName, methodName, assemblyQualifiedName,
                                    typeof (TResult).FullName),
                                e);
                        }
                    });
            IObservable<ProgressingResult<TResult>> errorObs = _errors
                .Where(eArgs => eArgs.OperationId == operationId)
                .Select<ErrorCallbackEventArgs, ProgressingResult<TResult>>(eArgs => { throw eArgs.Error; });
            
            IObservable<IObservable<ProgressingResult<TResult>>> resultMetaObs = Observable.Create((
                IObserver<IObservable<ProgressingResult<TResult>>> obs) =>
                {
                    var resultOrErrorObs = resultObs.Amb(errorObs)
                                                    .Replay(Scheduler.Default);
                    resultOrErrorObs.Connect();

                    string serializedArgument = Serializer.Serialize(argument);
                    Instruction msg = new Instruction()
                        {
                            OperationId = operationId,
                            AssemblyQualifiedName = assemblyQualifiedName,
                            MethodName = methodName,
                            ArgumentTypeName = argument.GetType().FullName,
                            SerializedArgument = serializedArgument
                        };
                    Executor.Execute(msg);

                    IObservable<ProgressingResult<TResult>> combinedObs;
                    //wait here: (First() blocks)
                    try
                    {
                        //might throw
                        var resultOrError = resultOrErrorObs.First();
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
            return new Future<TResult>(futureObs);
        }
    }
}
