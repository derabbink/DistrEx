using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using DistrEx.Communication.Contracts.Message;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Plugin;

namespace DistrEx.Communication.Service.AssemblyManager
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AssemblyManagerService : IAssemblyManager, IDisposable
    {
        private PluginManager _pluginManager;

        public AssemblyManagerService(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        public void AddAssembly(Assembly assembly)
        {
            _pluginManager.Load(assembly.AssemblyStream, assembly.Name);
        }

        public void Clear()
        {
            _pluginManager.Reset();
        }

        public void Dispose()
        {
            _pluginManager.Dispose();
        }
    }
}
