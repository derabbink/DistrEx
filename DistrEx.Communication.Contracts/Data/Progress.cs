using System;
using System.Runtime.Serialization;

namespace DistrEx.Communication.Contracts.Data
{
    [DataContract(Namespace = "http://schemas.fugro/distrex/data/progress")]
    public class Progress
    {
        [DataMember]
        public Guid OperationId { get; set; }
    }
}
