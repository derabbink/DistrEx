using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistrEx.Coordinator.Interface
{
    public abstract class TargetSpec
    {

        public TargetedInstruction<TArgument, TResult> Do<TArgument, TResult>(
            InstructionSpec<TArgument, TResult> instruction)
        {
            return TargetedInstruction<TArgument, TResult>.Create(this, instruction);
        }

        public abstract TResult Invoke<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction,
                                                           TArgument argument);
    }
}
