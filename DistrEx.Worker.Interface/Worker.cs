using System;

namespace DistrEx.Worker.Interface
{
    public abstract class Worker : IDisposable
    {
        public void StartServices()
        {
            StartAssemblyManagerService();
            StartExecutorService();
        }

        protected abstract void StartAssemblyManagerService();
        protected abstract void StartExecutorService();

        public void StopServices()
        {
            StopAssemblyManagerService();
            StopExecutorService();
        }

        protected abstract void StopAssemblyManagerService();
        protected abstract void StopExecutorService();
        
        public virtual void Dispose()
        {
            StopServices();
        }
    }
}
