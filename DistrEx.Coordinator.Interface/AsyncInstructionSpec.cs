using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public abstract class AsyncInstructionSpec<TArgument, TResult> : Spec
    {
        /// <summary>
        /// Creates an actual (executable) AsyncInstruction from this specification
        /// </summary>
        /// <returns></returns>
        public abstract TwoPartInstruction<TArgument, TResult> GetDelegate();
    }
}
