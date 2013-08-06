using DistrEx.Common;

namespace DistrEx.Coordinator.Interface.TargetedInstructions
{
    internal class TargetedAsyncInstruction<TArgument, TResult> : TargetedInstruction<TArgument, TResult>
    {
        protected TargetedAsyncInstruction(TargetSpec target, AsyncInstructionSpec<TArgument, TResult> instruction)
            : base(target)
        {
            Instruction = instruction;
        }

        public static TargetedInstruction<TArgument, TResult> Create(TargetSpec target, AsyncInstructionSpec<TArgument, TResult> instruction)
        {
            return new TargetedAsyncInstruction<TArgument, TResult>(target, instruction);
        }

        protected AsyncInstructionSpec<TArgument, TResult> Instruction
        {
            get;
            private set;
        }

        public override Future<TResult> Invoke(TArgument argument)
        {
            Target.TransportAssemblies(Instruction);
            return Target.InvokeAsync(Instruction, argument);
        }
    }
}
