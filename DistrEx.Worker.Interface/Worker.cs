using System;

namespace DistrEx.Worker.Interface
{
    public abstract class Worker : IDisposable
    {
        #region IDisposable Members

        public virtual void Dispose()
        {
            StopServices();
        }

        #endregion

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
    }
}
