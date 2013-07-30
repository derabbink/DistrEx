using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using DistrEx.Common;
using DistrEx.Common.Serialization;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Events;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Communication.Service.Executor;
using DistrEx.Plugin;
using NUnit.Framework;
using DependencyResolver;

namespace DistrEx.Communication.Service.Test.Executor
{
    [TestFixture]
    public class ExecutorServiceTest
    {
        private PluginManager _pluginManager;
        private IExecutor _executor;
        private IExecutorCallback _executorCallback;

        private IObservable<ProgressCallbackEventArgs> _progresses;
        private IObservable<CompleteCallbackEventArgs> _completes;
        private IObservable<ErrorCallbackEventArgs> _errors;

        private Instruction<int, int> _delegateIdentity;
        private int _argumentIdentity;
        private string _serializedArgumentIdentity;
        private Guid _identityOperationId;
        private Instruction _instructionIdentity;

        private Instruction<Exception, Exception> _delegateThrow;
        private Exception _argumentThrow;
        private string _serializedArgumentThrow;
        private Guid _throwOperationId;
        private Instruction _instructionThrow;

        #region setup
        [SetUp]
        public void Setup()
        {
            _executorCallback = new ExecutorCallbackService();
            _pluginManager = new PluginManager();
            _executor = new ExecutorService(_pluginManager, _executorCallback);

            TransportThisAssembly();
            SetupCallbackObservables();
            SetupInstructions();
        }

        private void TransportThisAssembly()
        {
            Assembly thisAssembly = GetType().Assembly;
            IObservable<AssemblyName> dependencies = Resolver.GetAllDependencies(thisAssembly.GetName());
            dependencies.Subscribe(aName =>
            {
                String path = new Uri(aName.CodeBase).LocalPath;
                using (Stream assyFileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    _pluginManager.Load(assyFileStream, aName.Name);
                }
            });
        }

        private void SetupCallbackObservables()
        {
            _progresses = Observable.FromEventPattern<ProgressCallbackEventArgs>(_executorCallback.SubscribeProgress, _executorCallback.UnsubscribeProgress).Select(ePattern => ePattern.EventArgs);
            _completes = Observable.FromEventPattern<CompleteCallbackEventArgs>(_executorCallback.SubscribeComplete, _executorCallback.UnsubscribeComplete).Select(ePattern => ePattern.EventArgs);
            _errors = Observable.FromEventPattern<ErrorCallbackEventArgs>(_executorCallback.SubscribeError, _executorCallback.UnsubscribeError).Select(ePattern => ePattern.EventArgs);
        }

        private void SetupInstructions()
        {
            _argumentIdentity = 1;
            _serializedArgumentIdentity = Serializer.Serialize(_argumentIdentity);
            _identityOperationId = Guid.NewGuid();
            _delegateIdentity = (ct, p, i) => i;
            MethodInfo mi = _delegateIdentity.Method;
            _instructionIdentity = new Instruction()
                {
                    ArgumentTypeName = _argumentIdentity.GetType().FullName,
                    SerializedArgument = _serializedArgumentIdentity,
                    AssemblyQualifiedName = mi.ReflectedType.AssemblyQualifiedName,
                    MethodName = mi.Name,
                    OperationId = _identityOperationId
                };

            _argumentThrow = new Exception("Expected");
            _serializedArgumentThrow = Serializer.Serialize(_argumentThrow);
            _throwOperationId = Guid.NewGuid();
            _delegateThrow = (ct, p, e) => { throw e; };
            mi = _delegateThrow.Method;
            _instructionThrow = new Instruction()
            {
                ArgumentTypeName = _argumentThrow.GetType().FullName,
                SerializedArgument = _serializedArgumentThrow,
                AssemblyQualifiedName = mi.ReflectedType.AssemblyQualifiedName,
                MethodName = mi.Name,
                OperationId = _throwOperationId
            };
        }
        #endregion

        #region tests
        [Test]
        public void ExecuteSuccess()
        {
            var expected = _argumentIdentity;
            IObservable<int> observable = Observable.Create((IObserver<int> observer) =>
                {
                    var resultObs = _completes
                        .Where(eArgs => eArgs.OperationId == _identityOperationId)
                        .Replay(Scheduler.Default);
                    resultObs.Connect();

                    _executor.Execute(_instructionIdentity);

                    int result = (int)resultObs.First().Result;
                    observer.OnNext(result);
                    observer.OnCompleted();

                    return Disposable.Empty;
                });
            var actual = observable.Last();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ExecuteError()
        {
            IObservable<Exception> observable = Observable.Create((IObserver<Exception> observer) =>
            {
                var errorObs = _errors
                    .Where(eArgs => eArgs.OperationId == _throwOperationId)
                    .Replay(Scheduler.Default);
                errorObs.Connect();

                _executor.Execute(_instructionThrow);

                Exception error = (Exception) errorObs.First().Error;
                observer.OnError(error);
                
                return Disposable.Empty;
            });
            var actual = observable.Last();
        }
        #endregion
    }
}
