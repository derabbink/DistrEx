using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.Interface.TargetedInstructions;

namespace DistrEx.Coordinator.InstructionSpecs.Sequential
{
    internal class MonitoredSequentialInstructionSpec<TArgument, TNextResult> :
        NonTransferrableDelegateInstructionSpec<TArgument, TNextResult>
    {
        protected MonitoredSequentialInstructionSpec(Instruction<TArgument, TNextResult> monitoredInstruction) : base(monitoredInstruction)
        {
        }

        internal static MonitoredSequentialInstructionSpec<TArgument, TNextResult> Create<TIntermediateResult>(
            TargetedInstruction<TArgument, TIntermediateResult> firstInstruction,
            TargetedInstruction<TIntermediateResult, TNextResult> seconInstruction)
        {
            Instruction<TArgument, TNextResult> monitoredInstruction = Sequentialize(firstInstruction, seconInstruction);
            return new MonitoredSequentialInstructionSpec<TArgument, TNextResult>(monitoredInstruction);
        }

        private static Instruction<TArgument, TNextResult> Sequentialize<TIntermediateResult>(
            TargetedInstruction<TArgument, TIntermediateResult> firstInstruction,
            TargetedInstruction<TIntermediateResult, TNextResult> secondInstruction)
        {
            Instruction<TArgument, TNextResult> result = (ct, p, argument) =>
            {
                var progress = Progress<TNextResult>.Default;
                //part 1
                var future1 = Interface.Coordinator.GetInvocationFuture(firstInstruction, argument).Last();
                ct.Register(future1.Cancel);
                
                var progress1Obs = future1.Where(pr => pr.IsProgress).Select(_ => progress);
                progress1Obs.Subscribe(_ => p());

                var intermediateResult = future1.GetResult();

                //part 2
                var future2 = Interface.Coordinator.GetInvocationFuture(secondInstruction, intermediateResult).Last();
                ct.Register(future2.Cancel);

                var progress2Obs = future2.Where(pr => pr.IsProgress).Select(_ => progress);
                progress2Obs.Subscribe(_ => p());

                return future2.GetResult();
            };

            return result;
        }
    }
}
