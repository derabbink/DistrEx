namespace DistrEx.Coordinator.Interface
{
    public class CompletedStep<TResult>
    {
        public CompletedStep(TResult result)
        {
            ResultValue = result;
        }

        public TResult ResultValue
        {
            get;
            private set;
        }

        public CompletedStep<TNextResult> ThenDo<TNextResult>(TargetedInstruction<TResult, TNextResult> targetedInstruction)
        {
            return Coordinator.Do(targetedInstruction, ResultValue);
        }
    }
}
