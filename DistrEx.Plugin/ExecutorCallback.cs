using System;

namespace DistrEx.Plugin
{
    internal class ExecutorCallback : MarshalByRefObject
    {
        private readonly Action _progress;
        private readonly Action _reportCompleted1; 

        public ExecutorCallback(Action progress)
        {
            _progress = progress;
        }

        public ExecutorCallback(Action progress, Action reportCompleted1)
        {
            _progress = progress;
            _reportCompleted1 = reportCompleted1; 
        }

        internal void Progress()
        {
            _progress();
        }

        internal void ReportCompleted1()
        {
            _reportCompleted1(); 
        }
    }
}
