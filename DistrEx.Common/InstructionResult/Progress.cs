using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistrEx.Common.InstructionResult
{
    public class Progress<TResult> : ProgressingResult<TResult>
    {
        private static Progress<TResult> _defaultInstance = null;

        public static Progress<TResult> Default
        {
            get { return _defaultInstance ?? (_defaultInstance = new Progress<TResult>()); }
        }

        private Progress() {} 

        public override bool IsResult
        {
            get { return false; }
        }


    }
}
