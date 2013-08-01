using System.Reflection;
using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public abstract class AsyncInstructionSpec<TArgument, TResult>
    {
        /// <summary>
        /// Creates an actual (executable) AsyncInstruction from this specification
        /// </summary>
        /// <returns></returns>
        public abstract TwoPartInstruction<TArgument, TResult> GetDelegate();

        public abstract Assembly GetAssembly();

        public abstract string GetAssemblyQualifiedName();

        public abstract string GetMethodName();
    }
}
