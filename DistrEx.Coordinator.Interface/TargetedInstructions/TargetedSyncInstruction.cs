using DistrEx.Common;

namespace DistrEx.Coordinator.Interface.TargetedInstructions
{
    public class TargetedSyncInstruction<TArgument, TResult> : TargetedInstruction<TArgument, TResult>
    {
        protected TargetedSyncInstruction(TargetSpec target, InstructionSpec<TArgument, TResult> instruction) : base(target)
        {
            Instruction = instruction;
        }

        public static TargetedInstruction<TArgument, TResult> Create(TargetSpec target, InstructionSpec<TArgument, TResult> instruction)
        {
            return new TargetedSyncInstruction<TArgument, TResult>(target, instruction);
        }

        protected InstructionSpec<TArgument, TResult> Instruction
        {
            get;
            private set;
        }

        public override Future<TResult> Invoke(TArgument argument)
        {
            return Target.Invoke(Instruction, argument); 
        }

        public override void TransportAssemblies()
        {
            Target.TransportAssemblies(Instruction);
        }
    }
}
