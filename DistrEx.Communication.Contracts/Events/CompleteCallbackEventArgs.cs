using System;

namespace DistrEx.Communication.Contracts.Events
{
    public class CompleteCallbackEventArgs : EventArgs
    {
        public CompleteCallbackEventArgs(Guid operationId, object result)
        {
            OperationId = operationId;
            Result = result;
        }

        public Guid OperationId
        {
            get;
            private set;
        }

        public object Result
        {
            get;
            private set;
        }
    }
}
