using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DistrEx.Plugin
{
    /// <summary>
    /// Helper to execute code in other appdomain
    /// Usable for ONE instruction only. Create a new instance for each instruction
    /// </summary>
    internal class Executor : MarshalByRefObject
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        //constructur is public for reflection
        public Executor()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        internal static Executor CreateInstanceInAppDomain(AppDomain domain)
        {
            AssemblyName ownAssyName = Assembly.GetExecutingAssembly().GetName();
            string typename = typeof(Executor).FullName;
            return domain.CreateInstanceAndUnwrap(ownAssyName.FullName, typename) as Executor;
        }

        internal object Execute(ExecutorCallback callback, string assemblyQualifiedName, string methodName, object argument)
        {
            Type t = Type.GetType(assemblyQualifiedName, true);
            MethodInfo action = t.GetMethod(methodName, BindingFlags.NonPublic
                                                        | BindingFlags.Public
                                                        | BindingFlags.Static);
            try
            {
                Action reportProgress = callback.Progress;
                return action.Invoke(null, new object[] {_cancellationTokenSource.Token, reportProgress, argument});

            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        internal void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
