using System;

namespace DistrEx.Plugin
{
    internal class ExecutorCallback : MarshalByRefObject
    {
        private readonly Action _callback;

        public ExecutorCallback(Action callback)
        {
            _callback = callback;
        }

        internal void Callback()
        {
            _callback();
        }
    }
}
