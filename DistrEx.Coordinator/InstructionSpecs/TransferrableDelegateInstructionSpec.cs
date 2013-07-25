using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using DistrEx.Common;

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

            var staticMethodSpec = StaticMethodInstructionSpec<TArgument, TResult>.Create(assemblyQualifiedName, methodName);
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
    }
}
