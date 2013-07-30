﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using DistrEx.Common;
using DistrEx.Common.Serialization;
using DistrEx.Communication.Contracts.Data;
using DistrEx.Communication.Contracts.Events;
using DistrEx.Communication.Contracts.Service;

namespace DistrEx.Communication.Service.Executor
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ExecutorCallbackService : IExecutorCallback
    {
        private event EventHandler<ProgressCallbackEventArgs> ProgressCallback;
        private event EventHandler<CompleteCallbackEventArgs> CompleteCallback;
        private event EventHandler<ErrorCallbackEventArgs> ErrorCallback;

        protected virtual void OnProgressCallback(ProgressCallbackEventArgs e)
        {
            ProgressCallback.Raise(this, e);
        }
        protected virtual void OnCompleteCallback(CompleteCallbackEventArgs e)
        {
            CompleteCallback.Raise(this, e);
        }
        protected virtual void OnErrorCallback(ErrorCallbackEventArgs e)
        {
            ErrorCallback.Raise(this, e);
        }

        public void Progress(Progress progress)
        {
            OnProgressCallback(new ProgressCallbackEventArgs(progress.OperationId));
        }

        public void Complete(Result result)
        {
            try
            {
                object value = Deserializer.Deserialize(result.ResultTypeName, result.SerializedResult);
                OnCompleteCallback(new CompleteCallbackEventArgs(result.OperationId, value));
            }
            catch (Exception e)
            {
                var exception = new Exception("Deserializing incoming result failed", e);
                OnErrorCallback(new ErrorCallbackEventArgs(result.OperationId, exception));
            }
        }

        public void Error(Error error)
        {
            Exception exception;
            try
            {
                object deserialized = Deserializer.Deserialize(error.ExceptionTypeName, error.SerializedException);
                try
                {
                    exception = (Exception) deserialized;
                }
                catch (Exception e)
                {
                    exception = new Exception("Could not cast incoming error to type Exception", e);
                }
            }
            catch (Exception e)
            {
                exception = new Exception("Deserializing incoming error failed", e);
            }
            OnErrorCallback(new ErrorCallbackEventArgs(error.OperationId, exception));
        }

        public void SubscribeProgress(EventHandler<ProgressCallbackEventArgs> handler)
        {
            ProgressCallback += handler;
        }
        public void UnsubscribeProgress(EventHandler<ProgressCallbackEventArgs> handler)
        {
            ProgressCallback -= handler;
        }

        public void SubscribeComplete(EventHandler<CompleteCallbackEventArgs> handler)
        {
            CompleteCallback += handler;
        }
        public void UnsubscribeComplete(EventHandler<CompleteCallbackEventArgs> handler)
        {
            CompleteCallback -= handler;
        }

        public void SubscribeError(EventHandler<ErrorCallbackEventArgs> handler)
        {
            ErrorCallback += handler;
        }
        public void UnsubscribeError(EventHandler<ErrorCallbackEventArgs> handler)
        {
            ErrorCallback -= handler;
        }
    }
}
