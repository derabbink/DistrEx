using System;

namespace DistrEx.Common
{
    public static class Wrapper
    {
        public static Instruction<TArgument, TResult> Wrap<TArgument, TResult>(Func<TArgument, TResult> instruction)
        {
            return (ct, r, arg) => instruction(arg);
        }
    }
}
