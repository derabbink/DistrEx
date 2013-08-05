using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public abstract class TargetSpec
    {
        public TargetedInstruction<TArgument, TResult> Do<TArgument, TResult>(
            Instruction<TArgument, TResult> instruction)
        {
            return Do(CreateInstructionSpec(instruction));
        }

        protected TargetedInstruction<TArgument, TResult> Do<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction)
        {
            return TargetedInstruction<TArgument, TResult>.Create(this, instruction);
        }

        public TargetedInstruction<TArgument, TResult> Do<TArgument, TResult>(
            TwoPartInstruction<TArgument, TResult> twoPartInstruction)
        {
            return DoAsync(CreateAsyncInstructionSpec(twoPartInstruction));
        }

        public TargetedInstruction<TArgument, TResult> DoAsync<TArgument, TResult>
            (AsyncInstructionSpec<TArgument, TResult> asyncInstruction)
        {
            return TargetedInstruction<TArgument, TResult>.Create(this, asyncInstruction);
        }

        public abstract void TransportAssembly(AssemblyName assembly);
        public abstract void TransportAssemblies<TArgument, TResult>(AsyncInstructionSpec<TArgument, TResult> instruction);
        public abstract bool AssemblyIsTransported(AssemblyName assembly);

        public abstract void ClearAssemblies();

        protected abstract InstructionSpec<TArgument, TResult> CreateInstructionSpec<TArgument, TResult>(Instruction<TArgument, TResult> instruction);
        protected abstract AsyncInstructionSpec<TArgument, TResult> CreateAsyncInstructionSpec<TArgument, TResult>(TwoPartInstruction<TArgument, TResult> instruction);


        public abstract Future<TResult> Invoke<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction,TArgument argument);
        public abstract Future<TResult> InvokeAsync<TArgument, TResult>(AsyncInstructionSpec<TArgument, TResult> asyncInstruction, TArgument argument);

        public abstract Future<TResult> GetAsyncResult<TResult>(Guid resultId);

    }
}
