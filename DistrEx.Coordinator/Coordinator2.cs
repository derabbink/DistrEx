using System;
using System.Reactive.Linq;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Coordinator.InstructionSpecs.Parallel;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.TargetSpecs;

namespace DistrEx.Coordinator
{
    public static class Coordinator2
    {
        public static CompletedStep<Tuple<TResult1, TResult2>> Do<TArgument, TResult1, TResult2>(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2,
            TArgument argument)
        {
            var monitored = MonitoredParallelInstructionSpec2<TArgument, TResult1, TResult2>.Create(
                targetedInstruction1,
                targetedInstruction2);
            OnCoordinator local = OnCoordinator.Default;
            var wrappedInstruction = TargetedInstruction<TArgument, Tuple<TResult1, TResult2>>.Create(local, monitored);
            return Interface.Coordinator.Do(wrappedInstruction, argument);
        }

        public static TargetedInstruction<TArgument, Tuple<TResult1, TResult2>> Do<TArgument, TResult1, TResult2>(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2
            )
        {
            throw new NotImplementedException();
        }
    }
}
