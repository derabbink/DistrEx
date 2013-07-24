using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DistrEx.Common;

namespace DistrEx.Coordinator.InstructionSpecs
{
    public class TransferrableDelegateInstructionSpec<TArgument, TResult> : DelegateInstructionSpec<TArgument, TResult>
    {
        private StaticMethodInstructionSpec<TArgument, TResult> _staticMethodSpec;

        private TransferrableDelegateInstructionSpec(StaticMethodInstructionSpec<TArgument, TResult> staticMethodSpec)
        {
            _staticMethodSpec = staticMethodSpec;
        }

        public static DelegateInstructionSpec<TArgument, TResult> Create(Instruction<TArgument, TResult> instruction)
        {
            //TODO throw exception if instruction contains closure
            
            MethodInfo methodInfo = instruction.Method;
            string methodName = methodInfo.Name;
            Type type = methodInfo.ReflectedType;
            string fqTypeName = type.FullName;
            string assemblyName = type.Assembly.FullName;
            
            var staticMethodSpec = StaticMethodInstructionSpec<TArgument, TResult>.Create(assemblyName, fqTypeName, methodName);
            return new TransferrableDelegateInstructionSpec<TArgument, TResult>(staticMethodSpec);
        }

        public override Instruction<TArgument, TResult> GetDelegate()
        {
            return _staticMethodSpec.GetDelegate();
        }
    }
}
