﻿using System;
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
    public static class Coordinator2
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
            return CoordinatorInstruction<TArgument, Tuple<TResult1, TResult2>>.Create(monitored);
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