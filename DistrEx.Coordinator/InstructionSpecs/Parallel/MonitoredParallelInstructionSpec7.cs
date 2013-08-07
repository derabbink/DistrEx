using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.Interface.TargetedInstructions;

namespace DistrEx.Coordinator.InstructionSpecs.Parallel
{
    internal abstract class MonitoredParallelInstructionSpec7<TArgument, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TCombinedResult> :
        MonitoredParallelInstructionSpec6<TArgument, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TCombinedResult>
    {
        protected MonitoredParallelInstructionSpec7(TargetedInstruction<TArgument, TResult1> targetedInstruction1,
                                                    TargetedInstruction<TArgument, TResult2> targetedInstruction2,
                                                    TargetedInstruction<TArgument, TResult3> targetedInstruction3,
                                                    TargetedInstruction<TArgument, TResult4> targetedInstruction4,
                                                    TargetedInstruction<TArgument, TResult5> targetedInstruction5,
                                                    TargetedInstruction<TArgument, TResult6> targetedInstruction6,
                                                    TargetedInstruction<TArgument, TResult7> targetedInstruction7,
                                                    Instruction<TArgument, TCombinedResult> monitoredInstruction)
            : base(targetedInstruction1, targetedInstruction2, targetedInstruction3, targetedInstruction4, targetedInstruction5, targetedInstruction6, monitoredInstruction)
        {
            TargetedInstruction7 = targetedInstruction7;
        }

        protected TargetedInstruction<TArgument, TResult7> TargetedInstruction7 { get; set; }
    }

    internal class MonitoredParallelInstructionSpec7<TArgument, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> :
        MonitoredParallelInstructionSpec7<TArgument, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>>
    {
        protected MonitoredParallelInstructionSpec7(TargetedInstruction<TArgument, TResult1> targetedInstruction1,
                                                    TargetedInstruction<TArgument, TResult2> targetedInstruction2,
                                                    TargetedInstruction<TArgument, TResult3> targetedInstruction3,
                                                    TargetedInstruction<TArgument, TResult4> targetedInstruction4,
                                                    TargetedInstruction<TArgument, TResult5> targetedInstruction5,
                                                    TargetedInstruction<TArgument, TResult6> targetedInstruction6,
                                                    TargetedInstruction<TArgument, TResult7> targetedInstruction7,
                                                    Instruction<TArgument, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> monitoredInstruction)
            : base(targetedInstruction1, targetedInstruction2, targetedInstruction3, targetedInstruction4, targetedInstruction5, targetedInstruction6, targetedInstruction7, monitoredInstruction)
        {
        }

        internal static MonitoredParallelInstructionSpec7<TArgument, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Create(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2,
            TargetedInstruction<TArgument, TResult3> targetedInstruction3,
            TargetedInstruction<TArgument, TResult4> targetedInstruction4,
            TargetedInstruction<TArgument, TResult5> targetedInstruction5,
            TargetedInstruction<TArgument, TResult6> targetedInstruction6,
            TargetedInstruction<TArgument, TResult7> targetedInstruction7)
        {
            Instruction<TArgument, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> monitoredInstruction =
                Parallelize(targetedInstruction1, targetedInstruction2, targetedInstruction3, targetedInstruction4, targetedInstruction5, targetedInstruction6, targetedInstruction7);
            return
                new MonitoredParallelInstructionSpec7<TArgument, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(
                    targetedInstruction1,
                    targetedInstruction2,
                    targetedInstruction3,
                    targetedInstruction4,
                    targetedInstruction5,
                    targetedInstruction6,
                    targetedInstruction7,
                    monitoredInstruction);
        }

        private static Instruction<TArgument, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> Parallelize(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1,
            TargetedInstruction<TArgument, TResult2> targetedInstruction2,
            TargetedInstruction<TArgument, TResult3> targetedInstruction3,
            TargetedInstruction<TArgument, TResult4> targetedInstruction4,
            TargetedInstruction<TArgument, TResult5> targetedInstruction5,
            TargetedInstruction<TArgument, TResult6> targetedInstruction6,
            TargetedInstruction<TArgument, TResult7> targetedInstruction7)
        {
            Instruction<TArgument, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> result = (ct, p, argument) =>
            {
                ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();

                var futureObs1 = Interface.Coordinator.GetInvocationFuture(targetedInstruction1, argument).Replay(Scheduler.Default);
                var futureObs2 = Interface.Coordinator.GetInvocationFuture(targetedInstruction2, argument).Replay(Scheduler.Default);
                var futureObs3 = Interface.Coordinator.GetInvocationFuture(targetedInstruction3, argument).Replay(Scheduler.Default);
                var futureObs4 = Interface.Coordinator.GetInvocationFuture(targetedInstruction4, argument).Replay(Scheduler.Default);
                var futureObs5 = Interface.Coordinator.GetInvocationFuture(targetedInstruction5, argument).Replay(Scheduler.Default);
                var futureObs6 = Interface.Coordinator.GetInvocationFuture(targetedInstruction6, argument).Replay(Scheduler.Default);
                var futureObs7 = Interface.Coordinator.GetInvocationFuture(targetedInstruction7, argument).Replay(Scheduler.Default);
                //triggers creating futures asynchronously
                futureObs1.Connect();
                futureObs2.Connect();
                futureObs3.Connect();
                futureObs4.Connect();
                futureObs5.Connect();
                futureObs6.Connect();
                futureObs7.Connect();
                //getting futures
                var future1 = futureObs1.Last();
                var future2 = futureObs2.Last();
                var future3 = futureObs3.Last();
                var future4 = futureObs4.Last();
                var future5 = futureObs5.Last();
                var future6 = futureObs6.Last();
                var future7 = futureObs7.Last();

                ct.Register(() =>
                    {
                        errors.Enqueue(new OperationCanceledException());
                        future1.Cancel();
                        future2.Cancel();
                        future3.Cancel();
                        future4.Cancel();
                        future5.Cancel();
                        future6.Cancel();
                        future7.Cancel();
                    });
                OnErrorCancelOthers(future1, future2, future3, future4, future5, future6, future7, errors.Enqueue);
                OnErrorCancelOthers(future2, future1, future3, future4, future5, future6, future7, errors.Enqueue);
                OnErrorCancelOthers(future3, future1, future2, future4, future5, future6, future7, errors.Enqueue);
                OnErrorCancelOthers(future4, future1, future2, future3, future5, future6, future7, errors.Enqueue);
                OnErrorCancelOthers(future5, future1, future2, future3, future4, future6, future7, errors.Enqueue);
                OnErrorCancelOthers(future6, future1, future2, future3, future4, future5, future7, errors.Enqueue);
                OnErrorCancelOthers(future6, future1, future2, future3, future4, future5, future6, errors.Enqueue);

                var progress = Progress<Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>>.Default;
                var progressObs1 = future1.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs2 = future2.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs3 = future3.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs4 = future4.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs5 = future5.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs6 = future6.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs7 = future7.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs = progressObs1.Merge(progressObs2).Merge(progressObs3).Merge(progressObs4).Merge(progressObs5).Merge(progressObs6).Merge(progressObs7);
                progressObs.Subscribe(_ => p());

                var getResult1 = GenerateGetResult(future1);
                var getResult2 = GenerateGetResult(future2);
                var getResult3 = GenerateGetResult(future3);
                var getResult4 = GenerateGetResult(future4);
                var getResult5 = GenerateGetResult(future5);
                var getResult6 = GenerateGetResult(future6);
                var getResult7 = GenerateGetResult(future7);
                
                if (errors.IsEmpty)
                    return new Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(getResult1(), getResult2(), getResult3(), getResult4(), getResult5(), getResult6(), getResult7());
                else
                {
                    Exception e = new Exception("There was an error during parallel execution, but the error is unknown.");
                    errors.TryDequeue(out e);
                    throw e;
                }
            };
            
            return result;
        }

        private static void OnErrorCancelOthers<T1, T2, T3, T4, T5, T6, T7>(Future<T1> errorSource,
                                                                            Future<T2> toCancel1,
                                                                            Future<T3> toCancel2,
                                                                            Future<T4> toCancel3,
                                                                            Future<T5> toCancel4,
                                                                            Future<T6> toCancel5,
                                                                            Future<T7> toCancel6,
                                                                            Action<Exception> processError)
        {
            errorSource.Subscribe(_ => { },
                                  error =>
                                      {
                                          processError(error);
                                          toCancel1.Cancel();
                                          toCancel2.Cancel();
                                          toCancel3.Cancel();
                                          toCancel4.Cancel();
                                          toCancel5.Cancel();
                                          toCancel6.Cancel();
                                      },
                                  () => { });
        }
    }
}
