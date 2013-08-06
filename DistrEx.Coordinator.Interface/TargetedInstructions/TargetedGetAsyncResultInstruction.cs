using System;
using DistrEx.Common;

namespace DistrEx.Coordinator.Interface.TargetedInstructions
{
    internal class TargetedGetAsyncResultInstruction<TResult> : TargetedInstruction<Guid, TResult>
    {
        public TargetedGetAsyncResultInstruction(TargetSpec target) : base(target)
        {
        }

        public static TargetedInstruction<Guid, TResult> Create(TargetSpec target)
        {
            return new TargetedGetAsyncResultInstruction<TResult>(target);
        }

        public override Future<TResult> Invoke(Guid asyncOperationId)
        {
            return Target.InvokeGetAsyncResult<TResult>(asyncOperationId);
        }
    }
}
