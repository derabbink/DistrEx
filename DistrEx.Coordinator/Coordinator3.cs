using System;
using System.Reactive.Linq;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Coordinator.InstructionSpecs.Parallel;
using DistrEx.Coordinator.InstructionSpecs.Sequential;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.TargetSpecs;
using DistrEx.Coordinator.TargetedInstructions;

namespace DistrEx.Coordinator
{
    public static class Coordinator3
    {
        public static CompletedStep<Tuple<TResult1, TResult2, TResult3>> Do<TArgument, TResult1, TResult2, TResult3>(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2,
            TargetedInstruction<TArgument, TResult3> targetedInstruction3,
            TArgument argument)
        {
            var wrappedInstruction = Do(targetedInstruction1, targetedInstruction2, targetedInstruction3);
            return Interface.Coordinator.Do(wrappedInstruction, argument);
        }

        public static CoordinatorInstruction<TArgument, Tuple<TResult1, TResult2, TResult3>> Do<TArgument, TResult1, TResult2, TResult3>(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2,
            TargetedInstruction<TArgument, TResult3> targetedInstruction3)
        {
            var monitored = MonitoredParallelInstructionSpec3<TArgument, TResult1, TResult2, TResult3>.Create(
                targetedInstruction1,
                targetedInstruction2,
                targetedInstruction3);
            return CoordinatorInstruction<TArgument, Tuple<TResult1, TResult2, TResult3>>.Create(monitored);
        }

        public static CoordinatorInstruction<TArgument, Tuple<TNextResult1, TNextResult2, TNextResult3>> ThenDo<TArgument, TIntermediateResult, TNextResult1, TNextResult2, TNextResult3>(
            this CoordinatorInstruction<TArgument, TIntermediateResult> instruction,
            TargetedInstruction<TIntermediateResult, TNextResult1> nextInstruction1,
            TargetedInstruction<TIntermediateResult, TNextResult2> nextInstruction2,
            TargetedInstruction<TIntermediateResult, TNextResult3> nextInstruction3)
        {
            var monitored = Do(nextInstruction1, nextInstruction2, nextInstruction3);
            return instruction.ThenDo(monitored);
        }
    }
}
