using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistrEx.Coordinator.InstructionSpecs.Sequential;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.TargetSpecs;

namespace DistrEx.Coordinator.TargetedInstructions
{
    public class CoordinatorInstruction<TArgument, TResult> : TargetedInstruction<TArgument, TResult>
    {
        protected CoordinatorInstruction(InstructionSpec<TArgument, TResult> instruction)
            : base(OnCoordinator.Default, instruction)
        {
        }

        public static CoordinatorInstruction<TArgument, TResult> Create(InstructionSpec<TArgument, TResult> instruction)
        {
            return new CoordinatorInstruction<TArgument, TResult>(instruction);
        }

        public CoordinatorInstruction<TArgument, TNextResult> ThenDo<TNextResult>(TargetedInstruction<TResult, TNextResult> nextInstruction)
        {
            return
                CoordinatorInstruction<TArgument, TNextResult>.Create(
                    MonitoredSequentialInstructionSpec<TArgument, TNextResult>.Create(this, nextInstruction));
        }
    }
}
