using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public abstract class InstructionSpec<TArgument, TResult> : Spec
    {
        /// <summary>
        ///     Creates an actual (executable) Instruction from this specification
        /// </summary>
        /// <returns></returns>
        public abstract Instruction<TArgument, TResult> GetDelegate();
    }
}
