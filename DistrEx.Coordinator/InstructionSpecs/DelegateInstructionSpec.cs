using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;

namespace DistrEx.Coordinator.InstructionSpecs
{
    public abstract class DelegateInstructionSpec<TArgument, TResult> : InstructionSpec<TArgument, TResult>
    {
    }
}
