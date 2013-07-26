using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistrEx.Communication.Contracts.Events
{
    public class ProgressCallbackEventArgs : EventArgs
    {
        public ProgressCallbackEventArgs(Guid operationId)
        {
            OperationId = operationId;
        }

        public Guid OperationId { get; private set; }
    }
}
