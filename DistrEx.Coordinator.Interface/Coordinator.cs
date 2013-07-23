using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public class Coordinator
    {
        public static TResult Do<TArgument, TResult>(TargetedInstruction<TArgument, TResult> targetedInstruction, TArgument argument)
        {
            return targetedInstruction.Invoke(argument);
        }
    }
}
