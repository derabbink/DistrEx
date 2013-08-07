using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.Interface.TargetedInstructions;

namespace DistrEx.Coordinator.InstructionSpecs.Sequential
{
    internal class MonitoredSingleInstructionSpec<TArgument, TResult> :
        NonTransferrableDelegateInstructionSpec<TArgument, TResult>
    {
        protected MonitoredSingleInstructionSpec(Instruction<TArgument, TResult> monitoredInstruction)
            : base(monitoredInstruction)
        {
        }

        internal static MonitoredSingleInstructionSpec<TArgument, TResult> Create(
            TargetedInstruction<TArgument, TResult> instruction)
        {
            Instruction<TArgument, TResult> monitoredInstruction = Sequentialize(instruction);
            return new MonitoredSingleInstructionSpec<TArgument, TResult>(monitoredInstruction);
        }

        private static Instruction<TArgument, TResult> Sequentialize(
            TargetedInstruction<TArgument, TResult> instruction)
        {
            Instruction<TArgument, TResult> result = (ct, p, argument) =>
            {
                var progress = Progress<TResult>.Default;
                var future1 = Interface.Coordinator.GetInvocationFuture(instruction, argument).Last();
                ct.Register(future1.Cancel);
                
                var progress1Obs = future1.Where(pr => pr.IsProgress).Select(_ => progress);
                progress1Obs.Subscribe(_ => p());

                return future1.GetResult();
            };

            return result;
        }
    }
}
