using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public class Coordinator
    {
        public static CompletedStep<TResult> Do<TArgument, TResult>(
            TargetedInstruction<TArgument, TResult> targetedInstruction, TArgument argument)
        {
            Future<TResult> future = targetedInstruction.Invoke(argument);
            var result = future.GetResult();
            return new CompletedStep<TResult>(result);
        }
    }
}
