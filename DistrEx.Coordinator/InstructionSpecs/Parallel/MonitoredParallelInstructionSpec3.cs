using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Coordinator.Interface;

namespace DistrEx.Coordinator.InstructionSpecs.Parallel
{
    internal abstract class MonitoredParallelInstructionSpec3<TArgument, TResult1, TResult2, TResult3, TCombinedResult> :
        MonitoredParallelInstructionSpec2<TArgument, TResult1, TResult2, TCombinedResult>
    {
        protected MonitoredParallelInstructionSpec3(TargetedInstruction<TArgument, TResult1> targetedInstruction1,
                                                    TargetedInstruction<TArgument, TResult2> targetedInstruction2,
                                                    TargetedInstruction<TArgument, TResult3> targetedInstruction3,
                                                    Instruction<TArgument, TCombinedResult> monitoredInstruction)
            : base(targetedInstruction1, targetedInstruction2, monitoredInstruction)
        {
            TargetedInstruction3 = targetedInstruction3;
        }

        protected TargetedInstruction<TArgument, TResult3> TargetedInstruction3 { get; set; }
    }

    internal class MonitoredParallelInstructionSpec3<TArgument, TResult1, TResult2, TResult3> :
        MonitoredParallelInstructionSpec3<TArgument, TResult1, TResult2, TResult3, Tuple<TResult1, TResult2, TResult3>>
    {
        protected MonitoredParallelInstructionSpec3(TargetedInstruction<TArgument, TResult1> targetedInstruction1,
                                                    TargetedInstruction<TArgument, TResult2> targetedInstruction2,
                                                    TargetedInstruction<TArgument, TResult3> targetedInstruction3,
                                                    Instruction<TArgument, Tuple<TResult1, TResult2, TResult3>> monitoredInstruction)
            : base(targetedInstruction1, targetedInstruction2, targetedInstruction3, monitoredInstruction)
        {
        }

        internal static MonitoredParallelInstructionSpec3<TArgument, TResult1, TResult2, TResult3> Create(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2,
            TargetedInstruction<TArgument, TResult3> targetedInstruction3)
        {
            Instruction<TArgument, Tuple<TResult1, TResult2, TResult3>> monitoredInstruction =
                Parallelize(targetedInstruction1, targetedInstruction2, targetedInstruction3);
            return new MonitoredParallelInstructionSpec3<TArgument, TResult1, TResult2, TResult3>(targetedInstruction1,
                                                                                        targetedInstruction2,
                                                                                        targetedInstruction3,
                                                                                        monitoredInstruction);
        }

        public override void TransportAssemblies(TargetSpec target)
        {
            base.TransportAssemblies(target);
            TargetedInstruction2.TransportAssemblies();
        }

        private static Instruction<TArgument, Tuple<TResult1, TResult2, TResult3>> Parallelize(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2,
            TargetedInstruction<TArgument, TResult3> targetedInstruction3)
        {
            Instruction<TArgument, Tuple<TResult1, TResult2, TResult3>> result = (ct, p, argument) =>
            {
                ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();

                var futureObs1 = Interface.Coordinator.GetInvocationFuture(targetedInstruction1, argument).Replay(Scheduler.Default);
                var futureObs2 = Interface.Coordinator.GetInvocationFuture(targetedInstruction2, argument).Replay(Scheduler.Default);
                var futureObs3 = Interface.Coordinator.GetInvocationFuture(targetedInstruction3, argument).Replay(Scheduler.Default);
                //triggers creating futures asynchronously
                futureObs1.Connect();
                futureObs2.Connect();
                futureObs3.Connect();
                //getting futures
                var future1 = futureObs1.Last();
                var future2 = futureObs2.Last();
                var future3 = futureObs3.Last();

                ct.Register(() =>
                    {
                        errors.Enqueue(new OperationCanceledException());
                        future1.Cancel();
                        future2.Cancel();
                        future3.Cancel();
                    });
                OnErrorCancelOthers(future1, future2, future3, errors.Enqueue);
                OnErrorCancelOthers(future2, future1, future3, errors.Enqueue);
                OnErrorCancelOthers(future3, future1, future2, errors.Enqueue);
                
                var progress = Progress<Tuple<TResult1, TResult2, TResult3>>.Default;
                var progressObs1 = future1.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs2 = future2.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs3 = future3.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs = progressObs1.Merge(progressObs2).Merge(progressObs3);
                progressObs.Subscribe(_ => p());

                var getResult1 = GenerateGetResult(future1);
                var getResult2 = GenerateGetResult(future2);
                var getResult3 = GenerateGetResult(future3);
                
                if (errors.IsEmpty)
                    return new Tuple<TResult1, TResult2, TResult3>(getResult1(), getResult2(), getResult3());
                else
                {
                    Exception e = new Exception("There was an error during parallel execution, but the error is unknown.");
                    errors.TryDequeue(out e);
                    throw e;
                }
            };
            
            return result;
        }

        private static void OnErrorCancelOthers<T1, T2, T3>(Future<T1> errorSource,
                                                            Future<T2> toCancel1,
                                                            Future<T3> toCancel2,
                                                            Action<Exception> processError)
        {
            errorSource.Subscribe(_ => { },
                                  error =>
                                      {
                                          processError(error);
                                          toCancel1.Cancel();
                                          toCancel2.Cancel();
                                      },
                                  () => { });
        }
    }
}
