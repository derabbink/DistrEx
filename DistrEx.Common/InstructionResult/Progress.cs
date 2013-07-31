namespace DistrEx.Common.InstructionResult
{
    public class Progress<TResult> : ProgressingResult<TResult>
    {
        private static Progress<TResult> _defaultInstance;

        private Progress()
        {
        }

        public static Progress<TResult> Default
        {
            get
            {
                return _defaultInstance ?? (_defaultInstance = new Progress<TResult>());
            }
        }

        public override bool IsResult
        {
            get
            {
                return false;
            }
        }
    }
}
