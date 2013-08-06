using DistrEx.Common;

namespace DistrEx.Coordinator.Interface.TargetedInstructions
{
    public abstract class TargetedInstruction<TArgument, TResult>
    {
        protected TargetedInstruction(TargetSpec target)
        {
            Target = target;
        }

        protected internal TargetSpec Target
        {
            get;
            private set;
        }

        public abstract Future<TResult> Invoke(TArgument argument);
    }
}
