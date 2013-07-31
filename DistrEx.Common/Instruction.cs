using System;
using System.Threading;

namespace DistrEx.Common
{
    public delegate TResult Instruction<in TArgument, out TResult>(CancellationToken cancellationToken, Action reportProgress, TArgument argument);
}
