using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DistrEx.Communication.Contracts.Data
{
    [DataContract(Namespace = "http://schemas.fugro/distrex/data/result")]
    public class Result
    {
        [DataMember]
        public Guid OperationId { get; set; }

        [DataMember]
        public string ResultTypeName { get; set; }

        [DataMember]
        public string SerializedResult { get; set; }
    }
}
