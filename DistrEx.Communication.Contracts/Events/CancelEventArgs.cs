using System;
using DistrEx.Communication.Contracts.Service;

namespace DistrEx.Communication.Contracts.Events
{
    public class CancelEventArgs : EventArgs
    {
        public CancelEventArgs(Guid operationId, IExecutorCallback callbackChannel)
        {
            OperationId = operationId;
            CallbackChannel = callbackChannel;
        }

        public Guid OperationId { get; private set; }

        public IExecutorCallback CallbackChannel { get; private set; }
    }
}
