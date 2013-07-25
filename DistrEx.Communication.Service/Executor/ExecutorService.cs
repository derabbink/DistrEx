using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Plugin;

namespace DistrEx.Communication.Service.Executor
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ExecutorService : IExecutor
    {
        private readonly PluginManager _pluginManager;

        public ExecutorService(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        public void Execute(Instruction instruction)
        {
            //TODO
            var operationId = instruction.OperationId;
            Progress progressMsg = new Progress() {OperationId = operationId};
            Action reportProgress = () => Callback.Progress(progressMsg);
            try
            {
                //var result = _pluginManager.Execute(instruction.AssemblyQualifiedName, instruction.ActionName);
                //Callback.Complete(new Result() {OperationId = operationId, Value = result});
            }
            catch (Exception e)
            {
                Callback.Error(new Error() {OperationId = operationId, Exception = e});
            }
        }

        public void Cancel(Cancellation cancellation)
        {
            //TODO
            throw new NotImplementedException();
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
