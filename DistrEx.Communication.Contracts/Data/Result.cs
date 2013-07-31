using System;
using System.Runtime.Serialization;

namespace DistrEx.Communication.Contracts.Data
{
    [DataContract(Namespace = "http://schemas.fugro/distrex/data/result")]
    public class Result
    {
        [DataMember]
        public Guid OperationId
        {
            get;
            set;
        }

        [DataMember]
        public string ResultTypeName
        {
            get;
            set;
        }

        [DataMember]
        public string SerializedResult
        {
            get;
            set;
        }
    }
}
