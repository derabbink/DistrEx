using System;
using System.Runtime.Serialization;

namespace DistrEx.Communication.Contracts.Data
{
    [DataContract(Namespace = "http://schemas.fugro/distrex/data/cancellation")]
    public class Cancellation
    {
        [DataMember]
        public Guid OperationId
        {
            get;
            set;
        }
    }
}
