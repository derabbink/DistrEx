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
        private PluginManager _pluginManager;

        public ExecutorService(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        public object Execute(Instruction instruction)
        {
            return _pluginManager.Execute(instruction.AssemblyName, instruction.FqTypeName, instruction.ActionName);
        }
    }
}
