using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistrEx.Coordinator.Interface
{
    public class CompletedStep<TResult>
    {
        private IEnumerable<TargetSpec> _targetsUsed;
        

        public CompletedStep(TResult result, IEnumerable<TargetSpec> targetsUsed)
        {
            ResultValue = result;
            _targetsUsed = targetsUsed;
        }

        public TResult ResultValue { get; private set; }

        public CompletedStep<TNextResult> ThenDo<TNextResult>(TargetedInstruction<TResult, TNextResult> targetedInstruction)
        {
            return Coordinator.Do(targetedInstruction, ResultValue, _targetsUsed);
        }

        public void Cleanup()
        {
            foreach (TargetSpec target in _targetsUsed)
            {
                target.ClearAssemblies();
            }
        }
    }
}
