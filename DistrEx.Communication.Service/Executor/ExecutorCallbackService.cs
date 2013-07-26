using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Events;
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

        public void SubscribeProgress(EventHandler<ProgressCallbackEventArgs> handler)
        {
            throw new NotImplementedException();
        }
        public void UnsubscribeProgress(EventHandler<ProgressCallbackEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public void SubscribeComplete(EventHandler<CompleteCallbackEventArgs> handler)
        {
            throw new NotImplementedException();
        }
        public void UnsubscribeComplete(EventHandler<CompleteCallbackEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public void SubscribeError(EventHandler<ErrorCallbackEventArgs> handler)
        {
            throw new NotImplementedException();
        }
        public void UnsubscribeError(EventHandler<ErrorCallbackEventArgs> handler)
        {
            throw new NotImplementedException();
        }
    }
}
