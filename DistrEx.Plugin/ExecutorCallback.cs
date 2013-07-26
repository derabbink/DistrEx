using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistrEx.Plugin
{
    internal class ExecutorCallback : MarshalByRefObject
    {
        private Action _progress;

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
