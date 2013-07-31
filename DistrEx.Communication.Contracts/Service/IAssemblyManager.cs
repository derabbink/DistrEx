using System.ServiceModel;
using DistrEx.Communication.Contracts.Message;

namespace DistrEx.Communication.Contracts.Service
{
    [ServiceContract(Name = "AssemblyManager", Namespace = "http://schemas.fugro/distrex/service/assemblymanager")]
    public interface IAssemblyManager
    {
        [OperationContract]
        void AddAssembly(Assembly assembly);

        [OperationContract]
        void Clear();
    }
}
