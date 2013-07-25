using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using DistrEx.Communication.Contracts.Message;

namespace DistrEx.Communication.Contracts.Service
{
    [ServiceContract(Name = "AssemblyManager", Namespace = "http://fugro.schemas/distrex/service/assemblymanager")]
    public interface IAssemblyManager
    {
        [OperationContract]
        void AddAssembly(Assembly assembly);

        [OperationContract]
        void Clear();
    }
}
