namespace DistrEx.Coordinator.Interface
{
    public class Coordinator
    {
        public static CompletedStep<TResult> Do<TArgument, TResult>(
            TargetedInstruction<TArgument, TResult> targetedInstruction, TArgument argument)
        {
            TResult result = targetedInstruction.Invoke(argument).GetResult();
            return new CompletedStep<TResult>(result);
        }
    }
}
