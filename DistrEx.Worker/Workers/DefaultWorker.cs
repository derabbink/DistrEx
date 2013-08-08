using System.ServiceModel;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Communication.Service.AssemblyManager;
using DistrEx.Communication.Service.Executor;
using DistrEx.Plugin;

namespace DistrEx.Worker.Workers
{
    public class DefaultWorker : Interface.Worker
    {
        private readonly PluginManager _pluginManager;
        private Executor _executor;
        private IAssemblyManager _assemblyManager;
        private ServiceHost _assemblyManagerServiceHost;
        private ExecutorService _executorService;
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
            _executorService = new ExecutorService();
            _executor = new Executor(_executorService, _pluginManager);
            _executorServiceHost = new ServiceHost(_executorService);
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
            _executor.Dispose();
            _executorServiceHost.Close();
            _executorService = null;
        }

        public override void Dispose()
        {
            base.Dispose();
            _pluginManager.Dispose();
        }
    }
}
