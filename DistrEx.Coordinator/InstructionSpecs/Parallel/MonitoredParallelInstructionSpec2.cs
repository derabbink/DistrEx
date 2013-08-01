using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Coordinator.Interface;

namespace DistrEx.Coordinator.InstructionSpecs.Parallel
{
    internal class MonitoredParallelInstructionSpec2<TArgument, TResult1, TResult2> :
        MonitoredParallelInstructionSpec<TArgument, TResult1, Tuple<TResult1, TResult2>>
    {
        protected MonitoredParallelInstructionSpec2(TargetedInstruction<TArgument, TResult1> targetedInstruction1,
                                                    TargetedInstruction<TArgument, TResult2> targetedInstruction2,
                                                    Instruction<TArgument, Tuple<TResult1, TResult2>> monitoredInstruction)
            : base(targetedInstruction1, monitoredInstruction)
        {
            TargetedInstruction2 = targetedInstruction2;
        }

        internal static MonitoredParallelInstructionSpec2<TArgument, TResult1, TResult2> Create(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2)
        {
            Instruction<TArgument, Tuple<TResult1, TResult2>> monitoredInstruction = Parallelize(targetedInstruction1,
                                                                                                 targetedInstruction2);
            return new MonitoredParallelInstructionSpec2<TArgument, TResult1, TResult2>(targetedInstruction1,
                                                                                        targetedInstruction2,
                                                                                        monitoredInstruction);
        }

        protected TargetedInstruction<TArgument, TResult2> TargetedInstruction2 { get; set; }

        public override void TransportAssemblies(TargetSpec target)
        {
            base.TransportAssemblies(target);
            TargetedInstruction2.TransportAssemblies();
        }

        private static Instruction<TArgument, Tuple<TResult1, TResult2>> Parallelize(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2)
        {
            Instruction<TArgument, Tuple<TResult1, TResult2>> result = (ct, p, argument) =>
            {
                ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();

                var futureObs1 = Interface.Coordinator.GetInvocationFuture(targetedInstruction1, argument).Replay(Scheduler.Default);
                var futureObs2 = Interface.Coordinator.GetInvocationFuture(targetedInstruction2, argument).Replay(Scheduler.Default);
                //triggers creating futures asynchronously
                futureObs1.Connect();
                futureObs2.Connect();
                //getting futures
                var future1 = futureObs1.Last();
                var future2 = futureObs2.Last();

                ct.Register(() =>
                    {
                        errors.Enqueue(new OperationCanceledException());
                        future1.Cancel();
                        future2.Cancel();
                    });
                OnErrorCancelOthers(future1, future2, errors.Enqueue);
                OnErrorCancelOthers(future2, future1, errors.Enqueue);
                
                var progress = Progress<Tuple<TResult1, TResult2>>.Default;
                var progressObs1 = future1.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs2 = future2.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs = progressObs1.Merge(progressObs2);
                progressObs.Subscribe(_ => p());

                var getResult1 = GenerateGetResult(future1);
                var getResult2 = GenerateGetResult(future2);
                
                if (errors.IsEmpty)
                    return new Tuple<TResult1, TResult2>(getResult1(), getResult2());
                else
                {
                    Exception e = new Exception("There was an error during parallel execution, but the error is unknown.");
                    errors.TryDequeue(out e);
                    throw e;
                }
            };
            
            return result;
        }

        private static void OnErrorCancelOthers<T1, T2>(Future<T1> errorSource,
                                                        Future<T2> toCancel,
                                                        Action<Exception> processError)
        {
            errorSource.Subscribe(_ => { },
                                  error =>
                                      {
                                          processError(error);
                                          toCancel.Cancel();
                                      },
                                  () => { });
        }
    }
}
