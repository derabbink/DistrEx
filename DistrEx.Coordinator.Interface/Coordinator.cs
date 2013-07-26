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
        public static CompletedStep<TResult> Do<TArgument, TResult>(TargetedInstruction<TArgument, TResult> targetedInstruction, TArgument argument)
        {
            ICollection<TargetSpec> targets = new List<TargetSpec>();
            return Do(targetedInstruction, argument, targets);
        }

        protected internal static CompletedStep<TResult> Do<TArgument, TResult>(
            TargetedInstruction<TArgument, TResult> targetedInstruction, TArgument argument,
            IEnumerable<TargetSpec> usedTargets)
        {
            ICollection<TargetSpec> targets = new List<TargetSpec>(usedTargets);
            targets.Add(targetedInstruction.Target);

            try
            {
                var result = targetedInstruction.Invoke(argument).GetResult();
                return new CompletedStep<TResult>(result, targets.Distinct());
            }
            catch (Exception e)
            {
                foreach (TargetSpec target in targets.Distinct())
                {
                    target.ClearAssemblies();
                }
                throw;
            }
        }
    }
}
