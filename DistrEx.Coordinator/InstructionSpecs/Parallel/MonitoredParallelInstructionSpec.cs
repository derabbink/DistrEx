using System;
using System.Reactive.Linq;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;

namespace DistrEx.Coordinator.InstructionSpecs.Parallel
{
    internal abstract class MonitoredParallelInstructionSpec<TArgument, TResult1, TResult> :
        NonTransferrableDelegateInstructionSpec<TArgument, TResult>
    {
        protected MonitoredParallelInstructionSpec(TargetedInstruction<TArgument, TResult1> targetedInstruction1,
                                                   Instruction<TArgument, TResult> monitoredInstruction)
            : base(monitoredInstruction)
        {
            TargetedInstruction1 = targetedInstruction1;
        }

        protected TargetedInstruction<TArgument, TResult1> TargetedInstruction1 { get; set; }

        public override void TransportAssemblies(TargetSpec target)
        {
            base.TransportAssemblies(target);
            TargetedInstruction1.TransportAssemblies();
        }

        protected static Func<T> GenerateGetResult<T>(Future<T> future)
        {
            try
            {
                var result = future.Where(pr => pr.IsResult).Last().ResultValue;
                return () => result;
            }
            catch (Exception e)
            {
                return () => { throw e; };
            }
        }
    }
}
