using System.Reflection;
using DistrEx.Common;

namespace DistrEx.Coordinator.InstructionSpecs
{
    public class NonTransferrableDelegateInstructionSpec<TArgument, TResult> : DelegateInstructionSpec<TArgument, TResult>
    {
        private readonly Instruction<TArgument, TResult> _instruction;

        protected NonTransferrableDelegateInstructionSpec(Instruction<TArgument, TResult> instruction)
        {
            _instruction = instruction;
        }

        public static DelegateInstructionSpec<TArgument, TResult> Create(Instruction<TArgument, TResult> instruction)
        {
            return new NonTransferrableDelegateInstructionSpec<TArgument, TResult>(instruction);
        }

        public override Instruction<TArgument, TResult> GetDelegate()
        {
            return _instruction;
        }

        public override void TransportAssemblies(Interface.TargetSpec target)
        {
            //do nothing here
        }

        public override Assembly GetAssembly()
        {
            return _instruction.Method.ReflectedType.Assembly;
        }

        public override string GetAssemblyQualifiedName()
        {
            return _instruction.Method.ReflectedType.AssemblyQualifiedName;
        }

        public override string GetMethodName()
        {
            return _instruction.Method.Name;
        }
    }
}
