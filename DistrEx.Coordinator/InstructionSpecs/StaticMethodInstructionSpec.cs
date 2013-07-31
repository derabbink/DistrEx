using System;
using System.Reflection;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;

namespace DistrEx.Coordinator.InstructionSpecs
{
    internal class StaticMethodInstructionSpec<TArgument, TResult> : InstructionSpec<TArgument, TResult>
    {
        private StaticMethodInstructionSpec(string assemblyQualifiedName, string methodName)
        {
            AssemblyQualifiedName = assemblyQualifiedName;
            MethodName = methodName;
        }

        protected string AssemblyQualifiedName
        {
            get;
            private set;
        }

        protected string MethodName
        {
            get;
            private set;
        }

        internal static StaticMethodInstructionSpec<TArgument, TResult> Create(string assemblyQualifiedName,
                                                                               string methodName)
        {
            return new StaticMethodInstructionSpec<TArgument, TResult>(assemblyQualifiedName, methodName);
        }

        public override Instruction<TArgument, TResult> GetDelegate()
        {
            throw new NotImplementedException("This method should only be used on non-transferrable instruction specs that contain actual delegates");
        }

        public override Assembly GetAssembly()
        {
            return Type.GetType(AssemblyQualifiedName).Assembly;
        }

        public override string GetAssemblyQualifiedName()
        {
            return AssemblyQualifiedName;
        }

        public override string GetMethodName()
        {
            return MethodName;
        }
    }
}
