using System.Reflection;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;

namespace DistrEx.Coordinator.InstructionSpecs
{
    public class TwoPartInstructionSpec<TArgument, TResult> : AsyncInstructionSpec<TArgument, TResult>
    {
        private readonly TwoPartInstruction<TArgument, TResult> _instruction;

        public TwoPartInstructionSpec(TwoPartInstruction<TArgument, TResult> instruction)
        {
            _instruction = instruction;
        }

        public override TwoPartInstruction<TArgument, TResult> GetDelegate()
        {
            return _instruction; 
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
