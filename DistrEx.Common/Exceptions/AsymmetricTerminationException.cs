using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistrEx.Common.Exceptions
{
    public class AsymmetricTerminationException : OperationCanceledException
    {
        public AsymmetricTerminationException(string message) : base(message) {}

        public AsymmetricTerminationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
