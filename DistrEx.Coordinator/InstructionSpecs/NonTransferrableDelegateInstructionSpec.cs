using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistrEx.Common;

namespace DistrEx.Coordinator.InstructionSpecs
{
    public class NonTransferrableDelegateInstructionSpec<TArgument, TResult> : DelegateInstructionSpec<TArgument, TResult>
    {
        private Instruction<TArgument, TResult> _instruction;

        private NonTransferrableDelegateInstructionSpec(Instruction<TArgument, TResult> instruction)
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
    }
}
