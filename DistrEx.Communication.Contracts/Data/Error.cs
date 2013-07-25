using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DistrEx.Communication.Contracts.Data
{
    [DataContract(Namespace = "http://schemas.fugro/distrex/data/error")]
    public class Error
    {
        [DataMember]
        public Guid OperationId { get; set; }

        [DataMember]
        public Exception Exception { get; set; }
    }
}
