using System;
using System.ServiceModel;
using DistrEx.Communication.Contracts.Message;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Plugin;

namespace DistrEx.Communication.Service.AssemblyManager
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AssemblyManagerService : IAssemblyManager, IDisposable
    {
        private readonly PluginManager _pluginManager;

        public AssemblyManagerService(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        #region IAssemblyManager Members

        public void AddAssembly(Assembly assembly)
        {
            _pluginManager.Load(assembly.AssemblyStream, assembly.Name);
        }

        public void Clear()
        {
            _pluginManager.Reset();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _pluginManager.Dispose();
        }

        #endregion
    }
}
