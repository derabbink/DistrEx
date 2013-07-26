using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
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
        private IDictionary<Guid, CancellationTokenSource> _cancellationTokenSources;

        public ExecutorService(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
            _cancellationTokenSources = new ConcurrentDictionary<Guid, CancellationTokenSource>();
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
                var result = _pluginManager.Execute(instruction.AssemblyQualifiedName, instruction.ActionName, cts.Token,
                                                    reportProgress, instruction.Argument);
                Callback.Complete(new Result() {OperationId = operationId, Value = result});
            }
            catch (Exception e)
            {
                Callback.Error(new Error() {OperationId = operationId, Exception = e});
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

        IExecutorCallback Callback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IExecutorCallback>();
            }
        }
    }
}
