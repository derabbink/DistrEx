using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public abstract class InstructionSpec<TArgument, TResult>
    {
        /// <summary>
        /// Creates an actual (executable) Instruction from this specification
        /// </summary>
        /// <returns></returns>
        public abstract Instruction<TArgument, TResult> GetDelegate();
    }
}
