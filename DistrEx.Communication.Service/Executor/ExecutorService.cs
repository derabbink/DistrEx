using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using DistrEx.Common.Serialization;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Plugin;

namespace DistrEx.Communication.Service.Executor
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ExecutorService : IExecutor
    {
        private readonly PluginManager _pluginManager;
        //TODO make the key a Tuple<Guid, session-id>
        private readonly IDictionary<Guid, CancellationTokenSource> _cancellationTokenSources;
        private readonly IExecutorCallback _callbackChannel;

        public ExecutorService(PluginManager pluginManager) : this(pluginManager, null)
        {   
        }

        [Obsolete("Only used for testing", false)]
        public ExecutorService(PluginManager pluginManager, IExecutorCallback callbackChannel)
        {
            _pluginManager = pluginManager;
            _cancellationTokenSources = new ConcurrentDictionary<Guid, CancellationTokenSource>();
            _callbackChannel = callbackChannel;
        }

        public void Execute(Instruction instruction)
        {
            var operationId = instruction.OperationId;
            CancellationTokenSource cts = new CancellationTokenSource();
            _cancellationTokenSources.Add(operationId, cts);
            Progress progressMsg = new Progress() {OperationId = operationId};
            Action reportProgress = () => Callback.Progress(progressMsg);
            try
            {
                SerializedResult serializedResult = _pluginManager.Execute(instruction.AssemblyQualifiedName, instruction.MethodName, cts.Token,
                                                              reportProgress, instruction.ArgumentTypeName, instruction.SerializedArgument);
                Callback.Complete(new Result() { OperationId = operationId, ResultTypeName = serializedResult.TypeName, SerializedResult = serializedResult.Value });
            }
            catch (ExecutionException e)
            {
                Callback.Error(new Error() {OperationId = operationId, ExceptionTypeName = e.InnerExceptionTypeName, SerializedException = e.SerializedInnerException});
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
                cts.Cancel();
        }

        private IExecutorCallback Callback
        {
            get { return _callbackChannel ?? OperationContext.Current.GetCallbackChannel<IExecutorCallback>(); }
        }
    }
}
