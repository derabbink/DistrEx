using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public class TargetedInstruction<TArgument, TResult>
    {
        protected TargetedInstruction(TargetSpec target, InstructionSpec<TArgument, TResult> instruction)
        {
            Target = target;
            Instruction = instruction;
        }

        protected TargetedInstruction(TargetSpec target, AsyncInstructionSpec<TArgument, TResult> instruction)
        {
            Target = target;
            AsyncInstruction = instruction;
        }

        protected internal TargetSpec Target
        {
            get;
            private set;
        }

        protected InstructionSpec<TArgument, TResult> Instruction
        {
            get;
            private set;
        }

        protected AsyncInstructionSpec<TArgument, TResult> AsyncInstruction
        {
            get;
            private set; 
        }

        public static TargetedInstruction<TArgument, TResult> Create(TargetSpec target, InstructionSpec<TArgument, TResult> instruction)
        {
            return new TargetedInstruction<TArgument, TResult>(target, instruction);
        }

        public void TransportAssemblies()
        {
            Instruction.TransportAssemblies(Target);
        }
		
        public static TargetedInstruction<TArgument, TResult> Create(TargetSpec target, AsyncInstructionSpec<TArgument, TResult> instruction)
        {
            return new TargetedInstruction<TArgument, TResult>(target, instruction);
        }

        public Future<TResult> Invoke(TArgument argument)
        {
            if (Instruction == null)
            {
                Target.TransportAssemblies(AsyncInstruction);
                return Target.InvokeAsync(AsyncInstruction, argument);
            }
            return Target.Invoke(Instruction, argument);
        }

        //public Future<TResult> InvokeAsync(TArgument argument)
        //{
           
        //}
    }
}
