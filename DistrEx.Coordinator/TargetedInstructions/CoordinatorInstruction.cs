using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistrEx.Common;
using DistrEx.Coordinator.InstructionSpecs.Sequential;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.Interface.TargetedInstructions;
using DistrEx.Coordinator.TargetSpecs;

namespace DistrEx.Coordinator.TargetedInstructions
{
    public class CoordinatorInstruction<TArgument, TResult> : TargetedInstruction<TArgument, TResult>
    {
        protected CoordinatorInstruction(InstructionSpec<TArgument, TResult> instruction): this(instruction, null)
        {
        }

        protected CoordinatorInstruction(InstructionSpec<TArgument, TResult> instruction, Action extraTransportAssemblies)
            : base(OnCoordinator.Default)
        {
            Instruction = instruction;
            ExtraTransportAssemblies = extraTransportAssemblies;
        }

        protected InstructionSpec<TArgument, TResult> Instruction
        {
            get;
            private set;
        }

        public static CoordinatorInstruction<TArgument, TResult> Create(InstructionSpec<TArgument, TResult> instruction)
        {
            return Create(instruction, null);
        }

        public static CoordinatorInstruction<TArgument, TResult> Create(InstructionSpec<TArgument, TResult> instruction, Action extraTransportAssemblies)
        {
            return new CoordinatorInstruction<TArgument, TResult>(instruction, extraTransportAssemblies);
        }

        protected internal Action ExtraTransportAssemblies { get; private set; }

        public CoordinatorInstruction<TArgument, TNextResult> ThenDo<TNextResult>(TargetedInstruction<TResult, TNextResult> nextInstruction)
        {
            return
                CoordinatorInstruction<TArgument, TNextResult>.Create(
                    MonitoredSequentialInstructionSpec<TArgument, TNextResult>.Create(this, nextInstruction));
        }

        public override Future<TResult> Invoke(TArgument argument)
        {
            return Target.Invoke(Instruction, argument);
        }

        public override void TransportAssemblies()
        {
            Target.TransportAssemblies(Instruction);
            if (ExtraTransportAssemblies != null)
                ExtraTransportAssemblies();
        }
    }
}
