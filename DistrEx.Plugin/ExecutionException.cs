using System;
using System.Runtime.Serialization;
using DistrEx.Common.Serialization;

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
        protected ExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // deserialize additional fields
            InnerExceptionTypeName = info.GetValue("InnerExceptionTypeName", typeof(string)) as string;
            SerializedInnerException = info.GetValue("SerializedInnerException", typeof(string)) as string;
        }

        public static ExecutionException FromException(Exception e)
        {
            string serializedExTypeName;
            string serializedEx;
            try
            {
                serializedEx = Serializer.Serialize(e);
                serializedExTypeName = e.GetType().FullName;
            }
            catch
            {
                serializedEx = null;
                serializedExTypeName = typeof(Exception).FullName;
            }
            return new ExecutionException(serializedExTypeName, serializedEx);
        }

        public string InnerExceptionTypeName
        {
            get;
            private set;
        }

        public string SerializedInnerException
        {
            get;
            private set;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // serialize additional fields
            info.AddValue("InnerExceptionTypeName", InnerExceptionTypeName);
            info.AddValue("SerializedInnerException", SerializedInnerException);
        }
    }
}
