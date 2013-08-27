using System;
using DistrEx.Coordinator.InstructionSpecs.Parallel;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.Interface.TargetedInstructions;
using DistrEx.Coordinator.TargetedInstructions;

namespace DistrEx.Coordinator
{
    public static partial class Coordinator
    {
        public static CompletedStep<Tuple<TResult1, TResult2>> Do<TArgument, TResult1, TResult2>(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2,
            TArgument argument)
        {
            var wrappedInstruction = Do(targetedInstruction1, targetedInstruction2);
            return Interface.Coordinator.Do(wrappedInstruction, argument);
        }

        public static CoordinatorInstruction<TArgument, Tuple<TResult1, TResult2>> Do<TArgument, TResult1, TResult2>(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2)
        {
            var monitored = MonitoredParallelInstructionSpec2<TArgument, TResult1, TResult2>.Create(
                targetedInstruction1,
                targetedInstruction2);
            Action transportAssemblies = () =>
                {
                    targetedInstruction1.TransportAssemblies();
                    targetedInstruction2.TransportAssemblies();
                };
            return CoordinatorInstruction<TArgument, Tuple<TResult1, TResult2>>.Create(monitored, transportAssemblies);
        }

        public static CoordinatorInstruction<TArgument, Tuple<TNextResult1, TNextResult2>> ThenDo<TArgument, TIntermediateResult, TNextResult1, TNextResult2>(
            this CoordinatorInstruction<TArgument, TIntermediateResult> instruction,
            TargetedInstruction<TIntermediateResult, TNextResult1> nextInstruction1,
            TargetedInstruction<TIntermediateResult, TNextResult2> nextInstruction2)
        {
            var monitored = Do(nextInstruction1, nextInstruction2);
            return instruction.ThenDo(monitored);
        }
    }
}
