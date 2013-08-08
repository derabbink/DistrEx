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

        private event EventHandler<ClearAsyncResultsEventArgs> ClearAsyncResultsRequest;
        private event EventHandler<ExecuteEventArgs> ExecuteRequest;
        private event EventHandler<ExecuteAsyncEventArgs> ExecuteAsyncRequest;
        private event EventHandler<GetAsyncResultEventArgs> GetAsyncResultRequest;
        private event EventHandler<CancelEventArgs> CancelRequest;

        public ExecutorService() : this(null)
        {
        }

        [Obsolete("Only used for testing", false)]
        public ExecutorService(IExecutorCallback callbackChannel)
        {
            _callbackChannel = callbackChannel;
        }

        public virtual void OnClearAsyncResultsRequest(ClearAsyncResultsEventArgs e)
        {
            ClearAsyncResultsRequest.Raise(this, e);
        }
        public virtual void OnExecuteRequest(ExecuteEventArgs e)
        {
            ExecuteRequest.Raise(this, e);
        }
        public virtual void OnExecuteAsyncRequest(ExecuteAsyncEventArgs e)
        {
            ExecuteAsyncRequest.Raise(this, e);
        }
        public virtual void OnGetAsyncResultRequest(GetAsyncResultEventArgs e)
        {
            GetAsyncResultRequest.Raise(this, e);
        }
        public virtual void OnCancelRequest(CancelEventArgs e)
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

        public void ExecuteAsync(AsyncInstruction instruction)
        {
            var args = new ExecuteAsyncEventArgs(instruction.OperationId, instruction.AssemblyQualifiedName,
                                            instruction.MethodName, instruction.ArgumentTypeName,
                                            instruction.SerializedArgument, Callback);
            OnExecuteAsyncRequest(args);
        }

        public void GetAsyncResult(GetAsyncResultInstruction getAsyncResultInstruction)
        {
            var args = new GetAsyncResultEventArgs(getAsyncResultInstruction.OperationId,
                                                   getAsyncResultInstruction.AsyncOperationId,
                                                   Callback);
            OnGetAsyncResultRequest(args);
        }

        public void ClearAsyncResults()
        {
            var args = new ClearAsyncResultsEventArgs();
            OnClearAsyncResultsRequest(args);
        }

        public void Cancel(Cancellation cancellation)
        {
            var args = new CancelEventArgs(cancellation.OperationId, Callback);
            OnCancelRequest(args);
        }

        public void SubscribeClearAsyncResults(EventHandler<ClearAsyncResultsEventArgs> handler)
        {
            ClearAsyncResultsRequest += handler;
        }
        public void UnsubscribeClearAsyncResults(EventHandler<ClearAsyncResultsEventArgs> handler)
        {
            ClearAsyncResultsRequest -= handler;
        }

        public void SubscribeExecute(EventHandler<ExecuteEventArgs> handler)
        {
            ExecuteRequest += handler;
        }
        public void UnsubscribeExecute(EventHandler<ExecuteEventArgs> handler)
        {
            ExecuteRequest -= handler;
        }

        public void SubscribeExecuteAsync(EventHandler<ExecuteAsyncEventArgs> handler)
        {
            ExecuteAsyncRequest += handler;
        }
        public void UnsubscribeExecuteAsync(EventHandler<ExecuteAsyncEventArgs> handler)
        {
            ExecuteAsyncRequest -= handler;
        }

        public void SubscribeGetAsyncResult(EventHandler<GetAsyncResultEventArgs> handler)
        {
            GetAsyncResultRequest += handler;
        }
        public void UnsubscribeGetAsyncResult(EventHandler<GetAsyncResultEventArgs> handler)
        {
            GetAsyncResultRequest -= handler;
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
