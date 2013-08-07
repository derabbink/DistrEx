using System.Diagnostics.Contracts;
using System.Reflection;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;

namespace DistrEx.Coordinator.InstructionSpecs
{
    public class TransferrableDelegateInstructionSpec<TArgument, TResult> : DelegateInstructionSpec<TArgument, TResult>
    {
        private readonly StaticMethodInstructionSpec<TArgument, TResult> _staticMethodSpec;

        private TransferrableDelegateInstructionSpec(StaticMethodInstructionSpec<TArgument, TResult> staticMethodSpec)
        {
            _staticMethodSpec = staticMethodSpec;
        }

        public static DelegateInstructionSpec<TArgument, TResult> Create(Instruction<TArgument, TResult> instruction)
        {
            Contract.Requires(instruction.Method.IsStatic);
            MethodInfo methodInfo = instruction.Method;
            string methodName = methodInfo.Name;
            string assemblyQualifiedName = methodInfo.ReflectedType.AssemblyQualifiedName;

            StaticMethodInstructionSpec<TArgument, TResult> staticMethodSpec = StaticMethodInstructionSpec<TArgument, TResult>.Create(assemblyQualifiedName, methodName);
            return new TransferrableDelegateInstructionSpec<TArgument, TResult>(staticMethodSpec);
        }

        public override Instruction<TArgument, TResult> GetDelegate()
        {
            return _staticMethodSpec.GetDelegate();
        }

        public override Assembly GetAssembly()
        {
            return _staticMethodSpec.GetAssembly();
        }

        public override string GetAssemblyQualifiedName()
        {
            return _staticMethodSpec.GetAssemblyQualifiedName();
        }

        public override string GetMethodName()
        {
            return _staticMethodSpec.GetMethodName();
        }
    }
}
