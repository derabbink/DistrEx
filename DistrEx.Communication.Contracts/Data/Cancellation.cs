using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DistrEx.Communication.Contracts.Data
{
    [DataContract(Namespace = "http://schemas.fugro/distrex/data/cancellation")]
    public class Cancellation
    {
        [DataMember]
        public Guid OperationId { get; set; }
    }
}
