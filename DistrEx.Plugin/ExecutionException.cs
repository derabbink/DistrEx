using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DistrEx.Plugin
{
    [Serializable]
    public class ExecutionException : Exception
    {
        public ExecutionException(string innerExceptionTypeName, string serializedInnerException)
        {
            InnerExceptionTypeName = innerExceptionTypeName;
            SerializedInnerException = serializedInnerException;
        }

        // Constructor needed for serialization 
        protected ExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // deserialize additional fields
            this.InnerExceptionTypeName = info.GetValue("InnerExceptionTypeName", typeof(string)) as string;
            this.SerializedInnerException = info.GetValue("SerializedInnerException", typeof(string)) as string;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // serialize additional fields
            info.AddValue("InnerExceptionTypeName", this.InnerExceptionTypeName);
            info.AddValue("SerializedInnerException", this.SerializedInnerException);
        }

        public string InnerExceptionTypeName { get; private set; }

        public string SerializedInnerException { get; private set; }
    }
}
