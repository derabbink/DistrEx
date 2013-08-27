using System;
using DistrEx.Coordinator.InstructionSpecs.Parallel;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.Interface.TargetedInstructions;
using DistrEx.Coordinator.TargetedInstructions;

namespace DistrEx.Coordinator
{
    public static partial class Coordinator
    {
        public static CompletedStep<Tuple<TResult1, TResult2, TResult3, TResult4>> Do<TArgument, TResult1, TResult2, TResult3, TResult4>(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2,
            TargetedInstruction<TArgument, TResult3> targetedInstruction3,
            TargetedInstruction<TArgument, TResult4> targetedInstruction4,
            TArgument argument)
        {
            var wrappedInstruction = Do(targetedInstruction1, targetedInstruction2, targetedInstruction3, targetedInstruction4);
            return Interface.Coordinator.Do(wrappedInstruction, argument);
        }

        public static CoordinatorInstruction<TArgument, Tuple<TResult1, TResult2, TResult3, TResult4>> Do<TArgument, TResult1, TResult2, TResult3, TResult4>(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2,
            TargetedInstruction<TArgument, TResult3> targetedInstruction3,
            TargetedInstruction<TArgument, TResult4> targetedInstruction4)
        {
            var monitored = MonitoredParallelInstructionSpec4<TArgument, TResult1, TResult2, TResult3, TResult4>.Create(
                targetedInstruction1,
                targetedInstruction2,
                targetedInstruction3,
                targetedInstruction4);
            Action transportAssemblies = () =>
            {
                targetedInstruction1.TransportAssemblies();
                targetedInstruction2.TransportAssemblies();
                targetedInstruction3.TransportAssemblies();
                targetedInstruction4.TransportAssemblies();
            };
            return CoordinatorInstruction<TArgument, Tuple<TResult1, TResult2, TResult3, TResult4>>.Create(monitored, transportAssemblies);
        }

        public static CoordinatorInstruction<TArgument, Tuple<TNextResult1, TNextResult2, TNextResult3, TNextResult4>> ThenDo<TArgument, TIntermediateResult, TNextResult1, TNextResult2, TNextResult3, TNextResult4>(
            this CoordinatorInstruction<TArgument, TIntermediateResult> instruction,
            TargetedInstruction<TIntermediateResult, TNextResult1> nextInstruction1,
            TargetedInstruction<TIntermediateResult, TNextResult2> nextInstruction2,
            TargetedInstruction<TIntermediateResult, TNextResult3> nextInstruction3,
            TargetedInstruction<TIntermediateResult, TNextResult4> nextInstruction4)
        {
            var monitored = Do(nextInstruction1, nextInstruction2, nextInstruction3, nextInstruction4);
            return instruction.ThenDo(monitored);
        }
    }
}
