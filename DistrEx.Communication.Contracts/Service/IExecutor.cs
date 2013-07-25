using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using DistrEx.Communication.Contracts.Data;

namespace DistrEx.Communication.Contracts.Service
{
    [ServiceContract(Name = "Executor", Namespace = "http://fugro.schemas/distrex/service/executor")]
    public interface IExecutor
    {
        [OperationContract]
        object Execute(Instruction instruction);
    }
}
