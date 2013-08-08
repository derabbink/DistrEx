using System;
using DistrEx.Common;

namespace DistrEx.Coordinator.Interface.TargetedInstructions
{
    internal class TargetedAsyncInstruction<TArgument, TResult> : TargetedInstruction<TArgument, Guid>
    {
        protected TargetedAsyncInstruction(TargetSpec target, AsyncInstructionSpec<TArgument, TResult> instruction)
            : base(target)
        {
            Instruction = instruction;
        }

        public static TargetedInstruction<TArgument, Guid> Create(TargetSpec target, AsyncInstructionSpec<TArgument, TResult> instruction)
        {
            return new TargetedAsyncInstruction<TArgument, TResult>(target, instruction);
        }

        protected AsyncInstructionSpec<TArgument, TResult> Instruction
        {
            get;
            private set;
        }

        public override Future<Guid> Invoke(TArgument argument)
        {
            return Target.InvokeAsync(Instruction, argument);
        }

        public override void TransportAssemblies()
        {
            Target.TransportAssemblies(Instruction);
        }
    }
}
