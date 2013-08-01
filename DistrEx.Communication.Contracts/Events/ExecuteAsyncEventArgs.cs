using System;
using DistrEx.Communication.Contracts.Service;

namespace DistrEx.Communication.Contracts.Events
{
    public class ExecuteAsyncEventArgs : ExecuteEventArgs
    {
        public ExecuteAsyncEventArgs(Guid operationId, string assemblyQualifiedName, string methodName,
                                     string argumentTypeName, string serializedArgument, IExecutorCallback callbackChannel)
            :base(operationId, assemblyQualifiedName, methodName, argumentTypeName, serializedArgument, callbackChannel)
        {}
    }
}
