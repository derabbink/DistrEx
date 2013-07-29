using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public class Coordinator
    {
        public static CompletedStep<TResult> Do<TArgument, TResult>(
            TargetedInstruction<TArgument, TResult> targetedInstruction, TArgument argument)
        {
            var result = targetedInstruction.Invoke(argument).GetResult();
            return new CompletedStep<TResult>(result);
        }
    }
}
