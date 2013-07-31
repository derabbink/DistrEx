﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Plugin;

namespace DistrEx.Communication.Service.Executor
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ExecutorService : IExecutor
    {
        private readonly IExecutorCallback _callbackChannel;
        private readonly IDictionary<Guid, CancellationTokenSource> _cancellationTokenSources;
        private readonly PluginManager _pluginManager;

        public ExecutorService(PluginManager pluginManager)
            : this(pluginManager, null)
        {
        }

        [Obsolete("Only used for testing", false)]
        public ExecutorService(PluginManager pluginManager, IExecutorCallback callbackChannel)
        {
            _pluginManager = pluginManager;
            _cancellationTokenSources = new ConcurrentDictionary<Guid, CancellationTokenSource>();
            _callbackChannel = callbackChannel;
        }

        private IExecutorCallback Callback
        {
            get
            {
                return _callbackChannel ?? OperationContext.Current.GetCallbackChannel<IExecutorCallback>();
            }
        }

        #region IExecutor Members

        public void Execute(Instruction instruction)
        {
            Guid operationId = instruction.OperationId;
            var cts = new CancellationTokenSource();
            _cancellationTokenSources.Add(operationId, cts);
            var progressMsg = new Progress
            {
                OperationId = operationId
            };
            Action reportProgress = () => Callback.Progress(progressMsg);
            try
            {
                SerializedResult serializedResult = _pluginManager.Execute(instruction.AssemblyQualifiedName, instruction.MethodName, cts.Token,
                                                                           reportProgress, instruction.ArgumentTypeName, instruction.SerializedArgument);
                Callback.Complete(new Result
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
                Callback.Error(msg);
            }
            finally
            {
                _cancellationTokenSources.Remove(operationId);
            }
        }

        public void Cancel(Cancellation cancellation)
        {
            CancellationTokenSource cts;
            if (_cancellationTokenSources.TryGetValue(cancellation.OperationId, out cts))
            {
                cts.Cancel();
            }
        }

        #endregion
    }
}
