using System;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.Interface.TargetedInstructions;

namespace DistrEx.Coordinator
{
    public static partial class CompletedStep
    {
        public static CompletedStep<Tuple<TNextResult1, TNextResult2, TNextResult3>>
                ThenDo<TResult, TNextResult1, TNextResult2, TNextResult3>(
            this CompletedStep<TResult> step,
            TargetedInstruction<TResult, TNextResult1> targetedInstruction1,
            TargetedInstruction<TResult, TNextResult2> targetedInstruction2,
            TargetedInstruction<TResult, TNextResult3> targetedInstruction3)
        {
            return Coordinator.Do(targetedInstruction1, targetedInstruction2, targetedInstruction3, step.ResultValue);
        }
    }
}
