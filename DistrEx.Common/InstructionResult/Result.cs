namespace DistrEx.Common.InstructionResult
{
    public class Result<TResult> : ProgressingResult<TResult>
    {
        private readonly TResult _resultValue;

        public Result(TResult resultValue)
        {
            _resultValue = resultValue;
        }

        public override bool IsResult
        {
            get
            {
                return true;
            }
        }

        public override TResult ResultValue
        {
            get
            {
                return _resultValue;
            }
        }
    }
}
