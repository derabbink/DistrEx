using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Events;

namespace DistrEx.Communication.Contracts.Service
{
    [ServiceContract(Name = "Executor", Namespace = "http://schemas.fugro/distrex/service/executor", CallbackContract = typeof(IExecutorCallback))]
    public interface IExecutor
    {
        /// <summary>
        /// Requesting execution of an instruction
        /// </summary>
        /// <param name="instruction"></param>
        [OperationContract(IsOneWay = true)]
        void Execute(Instruction instruction);

        [OperationContract(IsOneWay = true)]
        void Cancel(Cancellation cancellation);
    }

    //[ServiceContract(Name = "ExecutorCallback", Namespace = "http://schemas.fugro/distrex/service/executorcallback")]
    public interface IExecutorCallback
    {
        /// <summary>
        /// Reporting progress for an insruction's execution
        /// </summary>
        /// <param name="progress"></param>
        [OperationContract(IsOneWay = true)]
        void Progress(Progress progress);

        /// <summary>
        /// Returning a result for an completed instruction
        /// </summary>
        /// <param name="result"></param>
        [OperationContract(IsOneWay = true)]
        void Complete(Result result);

        /// <summary>
        /// Reporting an error for an instruction
        /// </summary>
        /// <param name="error"></param>
        [OperationContract(IsOneWay = true)]
        void Error(Error error);

        void SubscribeProgress(EventHandler<ProgressCallbackEventArgs> handler);
        void UnsubscribeProgress(EventHandler<ProgressCallbackEventArgs> handler);

        void SubscribeComplete(EventHandler<CompleteCallbackEventArgs> handler);
        void UnsubscribeComplete(EventHandler<CompleteCallbackEventArgs> handler);

        void SubscribeError(EventHandler<ErrorCallbackEventArgs> handler);
        void UnsubscribeError(EventHandler<ErrorCallbackEventArgs> handler);
    }
}
