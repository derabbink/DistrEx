using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace DistrEx.Common.InstructionResult
{
    /// <summary>
    /// Represents a message in an observable sequence which represents an instructions lifecycle
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public abstract class ProgressingResult<TResult>
    {
        public bool IsProgress { get { return !IsResult; } }

        public abstract bool IsResult { get; }

        /// <summary>
        /// Result value, only set if IsResult
        /// </summary>
        public virtual TResult ResultValue
        {
            get {
                Contract.Requires(IsResult);
                return default(TResult);
            }
        }
    }
}
