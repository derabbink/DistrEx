using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistrEx.Coordinator.Interface
{
    public class TargetedAsyncSendInstruction<TArgument, TResult> : TargetedInstruction<TArgument, Guid>
    {
        protected TargetedAsyncSendInstruction(TargetSpec target, AsyncInstructionSpec<TArgument, TResult> asyncInstruction)
            : base(target, ToSendInstructionSpec(asyncInstruction))
        {
        }

        private static InstructionSpec<TArgument, Guid> ToSendInstructionSpec(
            AsyncInstructionSpec<TArgument, TResult> asyncInstruction)
        {
            throw new NotImplementedException();
        }
    }
}
