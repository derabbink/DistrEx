using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DistrEx.Communication.Contracts.Data
{
    [DataContract(Namespace = "http://fugro.schemas/distrex/data/instruction")]
    public class Instruction
    {
        [DataMember]
        public string AssemblyName { get; set; }

        [DataMember]
        public string FqTypeName { get; set; }

        [DataMember]
        public string ActionName { get; set; }
    }
}
