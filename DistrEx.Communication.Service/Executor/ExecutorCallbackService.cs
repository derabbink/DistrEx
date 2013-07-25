using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Service;

namespace DistrEx.Communication.Service.Executor
{
    public class ExecutorCallbackService : IExecutorCallback
    {
        public void Progress(Progress progress)
        {
            throw new NotImplementedException();
        }

        public void Complete(Result result)
        {
            throw new NotImplementedException();
        }

        public void Complete(Error error)
        {
            throw new NotImplementedException();
        }

        public void Error(Error error)
        {
            throw new NotImplementedException();
        }
    }
}
