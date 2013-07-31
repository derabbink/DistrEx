using System;
using DistrEx.Communication.Contracts.Service;

namespace DistrEx.Communication.Contracts.Events
{
    public class ExecuteEventArgs : EventArgs
    {
        public ExecuteEventArgs(Guid operationId, string assemblyQualifiedName, string methodName,
                                string argumentTypeName, string serializedArgument, IExecutorCallback callbackChannel)
        {
            OperationId = operationId;
            AssemblyQualifiedName = assemblyQualifiedName;
            MethodName = methodName;
            ArgumentTypeName = argumentTypeName;
            SerializedArgument = serializedArgument;
            CallbackChannel = callbackChannel;
        }

        public Guid OperationId { get; private set; }

        public string AssemblyQualifiedName { get; private set; }

        public string MethodName { get; private set; }

        public string ArgumentTypeName { get; private set; }

        public string SerializedArgument { get; private set; }

        public IExecutorCallback CallbackChannel { get; private set; }
    }
}
