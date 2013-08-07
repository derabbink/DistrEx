using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DistrEx.Common;
using DistrEx.Common.InstructionResult;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.Interface.TargetedInstructions;

namespace DistrEx.Coordinator.InstructionSpecs.Parallel
{
    internal class MonitoredParallelInstructionSpec<TArgument, TResult1, TResult> :
        NonTransferrableDelegateInstructionSpec<TArgument, TResult>
    {
        protected MonitoredParallelInstructionSpec(TargetedInstruction<TArgument, TResult1> targetedInstruction1,
                                                   Instruction<TArgument, TResult> monitoredInstruction)
            : base(monitoredInstruction)
        {
            TargetedInstruction1 = targetedInstruction1;
        }

        internal static MonitoredParallelInstructionSpec<TArgument, TResult1, Tuple<TResult1>> Create(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1)
        {
            Instruction<TArgument, Tuple<TResult1>> monitoredInstruction = Parallelize(targetedInstruction1);
            return new MonitoredParallelInstructionSpec<TArgument, TResult1, Tuple<TResult1>>(targetedInstruction1,
                                                                                              monitoredInstruction);
        }

        protected TargetedInstruction<TArgument, TResult1> TargetedInstruction1 { get; set; }

        public override void TransportAssemblies(TargetSpec target)
        {
            base.TransportAssemblies(target);
            TargetedInstruction1.TransportAssemblies();
        }

        private static Instruction<TArgument, Tuple<TResult1>> Parallelize(
            TargetedInstruction<TArgument, TResult1> targetedInstruction1)
        {
            Instruction<TArgument, Tuple<TResult1>> result = (ct, p, argument) =>
            {
                ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();

                var futureObs1 = Interface.Coordinator.GetInvocationFuture(targetedInstruction1, argument).Replay(Scheduler.Default);
                //triggers creating futures asynchronously
                futureObs1.Connect();
                //getting futures
                var future1 = futureObs1.Last();

                ct.Register(() =>
                {
                    errors.Enqueue(new OperationCanceledException());
                    future1.Cancel();
                });
                OnErrorCancelOthers(future1, errors.Enqueue);

                var progress = Progress<Tuple<TResult1>>.Default;
                var progressObs1 = future1.Where(pr => pr.IsProgress).Select(_ => progress);
                var progressObs = progressObs1;
                progressObs.Subscribe(_ => p());

                var getResult1 = GenerateGetResult(future1);

                if (errors.IsEmpty)
                    return new Tuple<TResult1>(getResult1());
                else
                {
                    throw errors.First();
                }
            };

            return result;
        }

        private static void OnErrorCancelOthers<T1>(Future<T1> errorSource,
                                                    Action<Exception> processError)
        {
            errorSource.Subscribe(_ => { },
                                  processError,
                                  () => { });
        }



        protected static Func<T> GenerateGetResult<T>(Future<T> future)
        {
            try
            {
                var result = future.GetResult();
                return () => result;
            }
            catch (Exception e)
            {
                return () => { throw e; };
            }
        }
    }
}
