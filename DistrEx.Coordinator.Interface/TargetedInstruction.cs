using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public class TargetedInstruction<TArgument, TResult>
    {
        protected TargetedInstruction(TargetSpec target, InstructionSpec<TArgument, TResult> instruction)
        {
            Target = target;
            Instruction = instruction;
        }

        protected internal TargetSpec Target
        {
            get;
            private set;
        }

        protected InstructionSpec<TArgument, TResult> Instruction
        {
            get;
            private set;
        }

        public static TargetedInstruction<TArgument, TResult> Create(TargetSpec target, InstructionSpec<TArgument, TResult> instruction)
        {
            return new TargetedInstruction<TArgument, TResult>(target, instruction);
        }

        public Future<TResult> Invoke(TArgument argument)
        {
            Target.TransportAssemblies(Instruction);
            return Target.Invoke(Instruction, argument);
        }
    }
}
