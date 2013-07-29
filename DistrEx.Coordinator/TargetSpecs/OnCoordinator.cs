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
using DistrEx.Coordinator.InstructionSpecs;
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

        public override void TransportAssemblies<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction)
        {
            //no need to do anything
        }

        public override void ClearAssemblies()
        {
            //no need to do anything
        }

        protected override InstructionSpec<TArgument, TResult> CreateInstructionSpec<TArgument, TResult>(Instruction<TArgument, TResult> instruction)
        {
            return NonTransferrableDelegateInstructionSpec<TArgument, TResult>.Create(instruction);
        }

        public override Future<TResult> Invoke<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction, CancellationToken cancellationToken, TArgument argument)
        {
            Instruction<TArgument, TResult> instr = instruction.GetDelegate();

            IObservable<ProgressingResult<TResult>> observable = Observable.Create((IObserver<ProgressingResult<TResult>> obs) =>
                {
                    var result = instr(cancellationToken, () => obs.OnNext(Progress<TResult>.Default), argument);
                    obs.OnNext(new Result<TResult>(result));
                    obs.OnCompleted();
                    return Disposable.Empty;
                });

            return new Future<TResult>(observable);
        }
    }
}
