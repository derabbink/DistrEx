using System;
using System.Runtime.Serialization;

namespace DistrEx.Communication.Contracts.Data
{
    [DataContract(Namespace = "http://schemas.fugro/distrex/data/error")]
    public class Error
    {
        [DataMember]
        public Guid OperationId
        {
            get;
            set;
        }

        [DataMember]
        public string ExceptionTypeName
        {
            get;
            set;
        }

        [DataMember]
        public string SerializedException
        {
            get;
            set;
        }
    }
}
