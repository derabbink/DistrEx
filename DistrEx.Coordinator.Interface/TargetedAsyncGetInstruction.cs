using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistrEx.Coordinator.Interface
{
    public class TargetedAsyncGetInstruction<TResult> : TargetedInstruction<Guid, TResult>
    {
        protected TargetedAsyncGetInstruction(TargetSpec target, Guid asyncInstructionId)
            : base(target, ToGetInstructionSpec(asyncInstructionId))
        {
        }

        private static InstructionSpec<Guid, TResult> ToGetInstructionSpec(Guid asyncInstructionId)
        {
            throw new NotImplementedException();
        }
    }
}
