using System.Reflection;
using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public abstract class InstructionSpec<TArgument, TResult>
    {
        /// <summary>
        ///     Creates an actual (executable) Instruction from this specification
        /// </summary>
        /// <returns></returns>
        public abstract Instruction<TArgument, TResult> GetDelegate();

        public abstract Assembly GetAssembly();

        public abstract void TransportAssemblies(TargetSpec target);

        public abstract string GetAssemblyQualifiedName();

        public abstract string GetMethodName();
    }
}
