using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using DistrEx.Common;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Events;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Plugin;

namespace DistrEx.Communication.Service.Executor
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ExecutorService : IExecutor
    {
        private readonly IExecutorCallback _callbackChannel;

        private event EventHandler<ExecuteEventArgs> ExecuteRequest;
        private event EventHandler<CancelEventArgs> CancelRequest;

        public ExecutorService() : this(null)
        {
        }

        [Obsolete("Only used for testing", false)]
        public ExecutorService(IExecutorCallback callbackChannel)
        {
            _callbackChannel = callbackChannel;
        }

        protected virtual void OnExecuteRequest(ExecuteEventArgs e)
        {
            ExecuteRequest.Raise(this, e);
        }

        protected virtual void OnCancelRequest(CancelEventArgs e)
        {
            CancelRequest.Raise(this, e);
        }

        public void Execute(Instruction instruction)
        {
            var args = new ExecuteEventArgs(instruction.OperationId, instruction.AssemblyQualifiedName,
                                            instruction.MethodName, instruction.ArgumentTypeName,
                                            instruction.SerializedArgument, Callback);
            OnExecuteRequest(args);
        }

        public void Cancel(Cancellation cancellation)
        {
            var args = new CancelEventArgs(cancellation.OperationId, Callback);
            OnCancelRequest(args);
        }

        public void SubscribeExecute(EventHandler<ExecuteEventArgs> handler)
        {
            ExecuteRequest += handler;
        }
        public void UnsubscribeExecute(EventHandler<ExecuteEventArgs> handler)
        {
            ExecuteRequest -= handler;
        }

        public void SubscribeCancel(EventHandler<CancelEventArgs> handler)
        {
            CancelRequest += handler;
        }
        public void UnsubscribeCancel(EventHandler<CancelEventArgs> handler)
        {
            CancelRequest -= handler;
        }

        private IExecutorCallback Callback
        {
            get
            {
                return _callbackChannel ?? OperationContext.Current.GetCallbackChannel<IExecutorCallback>();
            }
        }
    }
}
