using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Coordinator.InstructionSpecs;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.Interface.TargetedInstructions;

namespace DistrEx.Coordinator.TargetSpecs
{
    /// <summary>
    ///     TargetSpec for execution on the coordinator
    /// </summary>
    public class OnCoordinator : TargetSpec
    {
        private static OnCoordinator _defaultInstance;

        private OnCoordinator()
        {
        }

        public static OnCoordinator Default
        {
            get
            {
                return _defaultInstance ?? (_defaultInstance = new OnCoordinator());
            }
        }

        public override void TransportAssemblies<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction)
        {
        }

        public override bool AssemblyIsTransported(AssemblyName assembly)
        {
            return true;
        }
        public override void TransportAssemblies<TArgument, TResult>(AsyncInstructionSpec<TArgument, TResult> instruction)
        {
            //no need to do anything
        }

        protected override void ClearAsyncResults()
        {
            //TODO
        }

        protected override void ClearAssemblies()
        {
            //no need to do anything
        }

        protected override InstructionSpec<TArgument, TResult> CreateInstructionSpec<TArgument, TResult>(Instruction<TArgument, TResult> instruction)
        {
            return NonTransferrableDelegateInstructionSpec<TArgument, TResult>.Create(instruction);
        }

        protected override AsyncInstructionSpec<TArgument, Guid> CreateAsyncInstructionSpec<TArgument, TResult>(TwoPartInstruction<TArgument, Guid> instruction)
        {
            throw new NotImplementedException();
        }

        public override Future<TResult> Invoke<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction, TArgument argument)
        {
            Instruction<TArgument, TResult> instr = instruction.GetDelegate();
            CancellationTokenSource cts = new CancellationTokenSource();

            var progress = Progress<TResult>.Default;
            IObservable<ProgressingResult<TResult>> observable = Observable.Create((IObserver<ProgressingResult<TResult>> obs) =>
                {
                    var result = instr(cts.Token, () => obs.OnNext(progress), argument);
                    obs.OnNext(new Result<TResult>(result));
                    obs.OnCompleted();
                    return Disposable.Empty;
                });

            return new Future<TResult>(observable, cts.Cancel);
        }

        public override Future<TResult> InvokeAsync<TArgument, TResult>(AsyncInstructionSpec<TArgument, TResult> asyncInstruction, TArgument argument)
        {
            throw new NotImplementedException();
        }

        public override Future<TResult> InvokeGetAsyncResult<TResult>(Guid asyncOperationId)
        {
            throw new NotImplementedException();
        }
    }
}
