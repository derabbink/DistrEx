using System;
using System.Threading;

namespace DistrEx.Common
{
    /// <summary>
    /// Specifies a cancellable instruction that can report progress.
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="cancellationToken">CancellationToken to respect in Instruction's implementation</param>
    /// <param name="reportProgress">Delegate to invoke when reporting progress</param>
    /// <param name="argument">Data input</param>
    /// <returns></returns>
    public delegate TResult Instruction<in TArgument, out TResult>(CancellationToken cancellationToken, Action reportProgress, TArgument argument);
}
