using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;

namespace DistrEx.Coordinator.TargetSpecs
{
    /// <summary>
    /// TargetSpec for execution on the coordinator
    /// </summary>
    public class OnCoordinator : TargetSpec
    {
        public override TResult Invoke<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction, TArgument argument)
        {
            Instruction<TArgument, TResult> instr = instruction.Materialize();
            throw new NotImplementedException();
        }
    }
}
