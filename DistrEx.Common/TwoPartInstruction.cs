using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DistrEx.Common
{
    /// <summary>
    /// Specifies a cancellable two-part instruction that reports progress:
    /// part 1: some instructions,
    /// then follows an invocation of <paramref name="reportCompletedPart1"/>,
    /// then follows part 2: some more instructions
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="cancellationToken">CancellationToken to respect in Instruction's implementation</param>
    /// <param name="reportProgress">Delegate to invoke when reporting progress</param>
    /// <param name="reportCompletedPart1"></param>
    /// <param name="argument">Data input</param>
    /// <returns></returns>
    public delegate TResult TwoPartInstruction<in TArgument, out TResult>(CancellationToken cancellationToken, Action reportProgress, Action reportCompletedPart1, TArgument argument);
}
