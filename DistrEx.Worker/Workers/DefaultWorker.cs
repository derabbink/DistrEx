using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Communication.Service.AssemblyManager;
using DistrEx.Communication.Service.Executor;
using DistrEx.Plugin;
using DistrEx.Worker.Interface;

namespace DistrEx.Worker.Workers
{
    public class DefaultWorker : Interface.Worker
    {
        private PluginManager _pluginManager;
        private IAssemblyManager _assemblyManager;
        private ServiceHost _assemblyManagerServiceHost;
        private IExecutor _executor;
        private ServiceHost _executorServiceHost;

        public DefaultWorker()
        {
            _pluginManager = new PluginManager();
        }

        protected override void StartAssemblyManagerService()
        {
            _assemblyManager = new AssemblyManagerService(_pluginManager);
            _assemblyManagerServiceHost = new ServiceHost(_assemblyManager);
            _assemblyManagerServiceHost.Open();
        }

        protected override void StartExecutorService()
        {
            _executor = new ExecutorService(_pluginManager);
            _executorServiceHost = new ServiceHost(_executor);
            _executorServiceHost.Open();
        }

        protected override void StopAssemblyManagerService()
        {
            _assemblyManagerServiceHost.Close();
            _assemblyManager.Clear();
            _assemblyManager = null;
        }

        protected override void StopExecutorService()
        {
            _executorServiceHost.Close();
            _executor = null;
        }

        public override void Dispose()
        {
            base.Dispose();
            _pluginManager.Dispose();
        }
    }
}
