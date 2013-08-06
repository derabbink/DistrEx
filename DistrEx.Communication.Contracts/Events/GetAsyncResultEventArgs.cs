using System;
using DistrEx.Communication.Contracts.Service;

namespace DistrEx.Communication.Contracts.Events
{
    public class GetAsyncResultEventArgs : EventArgs
    {
        public GetAsyncResultEventArgs(Guid operationId, Guid asyncOperationId, IExecutorCallback callbackChannel)
        {
            OperationId = operationId;
            AsyncOperationId = asyncOperationId;
            CallbackChannel = callbackChannel;
        }

        public Guid OperationId { get; private set; }

        public Guid AsyncOperationId { get; private set; }

        public IExecutorCallback CallbackChannel { get; private set; }
    }
}
