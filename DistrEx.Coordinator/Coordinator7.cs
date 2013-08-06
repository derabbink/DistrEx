using System;
using DistrEx.Coordinator.InstructionSpecs.Parallel;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.TargetedInstructions;

namespace DistrEx.Coordinator
{
    public static partial class Coordinator
    {
        public static CompletedStep<Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> Do<TArgument, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2,
            TargetedInstruction<TArgument, TResult3> targetedInstruction3,
            TargetedInstruction<TArgument, TResult4> targetedInstruction4,
            TargetedInstruction<TArgument, TResult5> targetedInstruction5,
            TargetedInstruction<TArgument, TResult6> targetedInstruction6,
            TargetedInstruction<TArgument, TResult7> targetedInstruction7,
            TArgument argument)
        {
            var wrappedInstruction = Do(targetedInstruction1, targetedInstruction2, targetedInstruction3, targetedInstruction4, targetedInstruction5, targetedInstruction6, targetedInstruction7);
            return Interface.Coordinator.Do(wrappedInstruction, argument);
        }

        public static CoordinatorInstruction<TArgument, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> Do<TArgument, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2,
            TargetedInstruction<TArgument, TResult3> targetedInstruction3,
            TargetedInstruction<TArgument, TResult4> targetedInstruction4,
            TargetedInstruction<TArgument, TResult5> targetedInstruction5,
            TargetedInstruction<TArgument, TResult6> targetedInstruction6,
            TargetedInstruction<TArgument, TResult7> targetedInstruction7)
        {
            var monitored = MonitoredParallelInstructionSpec7<TArgument, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>.Create(
                targetedInstruction1,
                targetedInstruction2,
                targetedInstruction3,
                targetedInstruction4,
                targetedInstruction5,
                targetedInstruction6,
                targetedInstruction7);
            return CoordinatorInstruction<TArgument, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>>.Create(monitored);
        }

        public static CoordinatorInstruction<TArgument, Tuple<TNextResult1, TNextResult2, TNextResult3, TNextResult4, TNextResult5, TNextResult6, TNextResult7>> ThenDo<TArgument, TIntermediateResult, TNextResult1, TNextResult2, TNextResult3, TNextResult4, TNextResult5, TNextResult6, TNextResult7>(
            this CoordinatorInstruction<TArgument, TIntermediateResult> instruction,
            TargetedInstruction<TIntermediateResult, TNextResult1> nextInstruction1,
            TargetedInstruction<TIntermediateResult, TNextResult2> nextInstruction2,
            TargetedInstruction<TIntermediateResult, TNextResult3> nextInstruction3,
            TargetedInstruction<TIntermediateResult, TNextResult4> nextInstruction4,
            TargetedInstruction<TIntermediateResult, TNextResult5> nextInstruction5,
            TargetedInstruction<TIntermediateResult, TNextResult6> nextInstruction6,
            TargetedInstruction<TIntermediateResult, TNextResult7> nextInstruction7)
        {
            var monitored = Do(nextInstruction1, nextInstruction2, nextInstruction3, nextInstruction4, nextInstruction5, nextInstruction6, nextInstruction7);
            return instruction.ThenDo(monitored);
        }
    }
}
