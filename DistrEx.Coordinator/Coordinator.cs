using DistrEx.Coordinator.InstructionSpecs.Sequential;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.TargetedInstructions;

namespace DistrEx.Coordinator
{
    public static partial class Coordinator
    {
        public static CompletedStep<TResult> Do<TArgument, TResult>(
            TargetedInstruction<TArgument, TResult> targetedInstruction, TArgument argument)
        {
            return Interface.Coordinator.Do(targetedInstruction, argument);
        }

        public static CoordinatorInstruction<TArgument, TResult> Do<TArgument, TResult>(
            TargetedInstruction<TArgument, TResult> targetedInstruction)
        {
            var monitored = MonitoredSingleInstructionSpec<TArgument, TResult>.Create(targetedInstruction);
            return CoordinatorInstruction<TArgument, TResult>.Create(monitored);
        }
    }
}
