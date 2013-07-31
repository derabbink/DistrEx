using System;
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
        public string MethodName { get; set; }

        [DataMember]
        public string ArgumentTypeName { get; set; }

        [DataMember]
        public string SerializedArgument { get; set; }
    }
}
