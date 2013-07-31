using System.IO;
using System.ServiceModel;

namespace DistrEx.Communication.Contracts.Message
{
    [MessageContract]
    public class Assembly
    {
        //additional data must be [MessageHeader]s

        //With a stream, there can be only ONE MessageBodyMember, or else transfer reverts to a buffered strategy
        /// <summary>
        ///     stream producing the assembly/dll file's bytes
        /// </summary>
        [MessageBodyMember]
        public Stream AssemblyStream;

        /// <summary>
        ///     the full (long) name of the assembly
        /// </summary>
        [MessageHeader]
        public string FullName;

        /// <summary>
        ///     the (short) name of the assembly
        /// </summary>
        [MessageHeader]
        public string Name;
    }
}
