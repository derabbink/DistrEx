using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DistrEx.Communication.Contracts.Data
{
    [DataContract(Namespace = "http://schemas.fugro/distrex/data/instruction")]
    public class Instruction
    {
        [DataMember]
        public Guid OperationId { get; set; }

        [DataMember]
        public string AssemblyQualifiedName { get; set; }

        [DataMember]
        public string ActionName { get; set; }

        [DataMember]
        public object Argument { get; set; }
    }
}
