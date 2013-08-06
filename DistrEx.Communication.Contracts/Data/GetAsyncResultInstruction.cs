using System;
using System.Runtime.Serialization;

namespace DistrEx.Communication.Contracts.Data
{
    [DataContract(Namespace = "http://schemas.fugro/distrex/data/getasyncresultinstruction")]
    public class GetAsyncResultInstruction
    {
        [DataMember]
        public Guid OperationId { get; set; }

        [DataMember]
        public Guid AsyncOperationId { get; set; }
    }
}
