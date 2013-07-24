using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Coordinator.Interface;

namespace DistrEx.Coordinator.TargetSpecs
{
    /// <summary>
    /// TargetSpec for execution on the coordinator
    /// </summary>
    public class OnCoordinator : TargetSpec
    {
        private static OnCoordinator _defaultInstance = null;

        public static OnCoordinator Default
        {
            get { return _defaultInstance ?? (_defaultInstance = new OnCoordinator()); }
        }

        private OnCoordinator() { }

        public override Future<TResult> Invoke<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction, CancellationToken cancellationToken, TArgument argument)
        {
            Instruction<TArgument, TResult> instr = instruction.GetDelegate();

            ISubject<ProgressingResult<TResult>> progressObs = new Subject<ProgressingResult<TResult>>();
            Action reportProgress = () => progressObs.OnNext(Progress<TResult>.Default);

            IObservable<Result<TResult>> resultObs = Observable.Return(new Result<TResult>(instr(cancellationToken, reportProgress, argument)));

            IObservable<IObservable<ProgressingResult<TResult>>> obsObs = Observable.Return(progressObs);
            IObservable<ProgressingResult<TResult>> futureObs = obsObs.Concat(Observable.Return(resultObs)).Switch();
            return new Future<TResult>(futureObs);
        }
    }
}
