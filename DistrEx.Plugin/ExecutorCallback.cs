using System;

namespace DistrEx.Plugin
{
    internal class ExecutorCallback : MarshalByRefObject
    {
        private readonly Action _progress;

        public ExecutorCallback(Action progress)
        {
            _progress = progress;
        }

        internal void Progress()
        {
            _progress();
        }
    }
}
