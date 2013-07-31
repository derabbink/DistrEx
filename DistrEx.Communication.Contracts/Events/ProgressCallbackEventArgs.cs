using System;

namespace DistrEx.Communication.Contracts.Events
{
    public class ProgressCallbackEventArgs : EventArgs
    {
        public ProgressCallbackEventArgs(Guid operationId)
        {
            OperationId = operationId;
        }

        public Guid OperationId
        {
            get;
            private set;
        }
    }
}
