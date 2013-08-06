using DistrEx.Common;
using DistrEx.Coordinator.Interface.TargetedInstructions;

namespace DistrEx.Coordinator.Interface
{
    public abstract class TargetSpec
    {
        public TargetedInstruction<TArgument, TResult> Do<TArgument, TResult>(
            Instruction<TArgument, TResult> instruction)
        {
            return TargetedSyncInstruction<TArgument, TResult>.Create(this, CreateInstructionSpec(instruction));
        }

        public TargetedInstruction<TArgument, Guid> Do<TArgument, TResult>(
            TwoPartInstruction<TArgument, Guid> asyncInstruction)
        {
            return TargetedAsyncInstruction<TArgument, Guid>.Create(this, CreateAsyncInstructionSpec<TArgument, TResult>(asyncInstruction)); 
        }

        public TargetedInstruction<Guid, TResult> GetAsyncResult<TResult>()
        {
            return TargetedGetAsyncResultInstruction<TResult>.Create(this);
        }

        public abstract void TransportAssemblies<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction);
        public abstract bool AssemblyIsTransported(AssemblyName assembly);

        public void ClearEverything()
        {
            ClearAsyncResults();
            ClearAssemblies();
        }

        protected abstract void ClearAssemblies();
        protected abstract void ClearAsyncResults();

        protected abstract InstructionSpec<TArgument, TResult> CreateInstructionSpec<TArgument, TResult>(Instruction<TArgument, TResult> instruction);
        protected abstract AsyncInstructionSpec<TArgument, Guid> CreateAsyncInstructionSpec<TArgument, TResult>(TwoPartInstruction<TArgument, Guid> instruction);


        public abstract Future<TResult> Invoke<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction,TArgument argument);
        public abstract Future<TResult> InvokeAsync<TArgument, TResult>(AsyncInstructionSpec<TArgument, TResult> asyncInstruction, TArgument argument);
        public abstract Future<TResult> InvokeGetAsyncResult<TResult>(Guid asyncOperationId);


    }
}
