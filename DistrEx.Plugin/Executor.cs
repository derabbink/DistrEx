using System;
using System.Reflection;
using System.Threading;
using DistrEx.Common.Serialization;

namespace DistrEx.Plugin
{
    /// <summary>
    ///     Helper to execute code in other appdomain
    ///     Usable for ONE instruction only. Create a new instance for each instruction
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

        /// <summary>
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="assemblyQualifiedName"></param>
        /// <param name="methodName"></param>
        /// <param name="argumentTypeName"></param>
        /// <param name="serializedArgument"></param>
        /// <returns>serialized result</returns>
        /// <exception cref="ExecutionException">If something went wrong with the execution</exception>
        internal SerializedResult Execute(ExecutorCallback callback, string assemblyQualifiedName, string methodName, string argumentTypeName, string serializedArgument)
        {
            Type t = Type.GetType(assemblyQualifiedName, true);
            MethodInfo func = t.GetMethod(methodName, BindingFlags.NonPublic
                                                      | BindingFlags.Public
                                                      | BindingFlags.Static);
            try
            {
                return ExecuteWrapped(func, callback, argumentTypeName, serializedArgument);
            }
            catch (Exception e)
            {
                string serializedExTypeName;
                string serializedEx;
                try
                {
                    serializedEx = Serializer.Serialize(e);
                    serializedExTypeName = e.GetType().FullName;
                }
                catch
                {
                    serializedEx = null;
                    serializedExTypeName = typeof(Exception).FullName;
                }
                throw new ExecutionException(serializedExTypeName, serializedEx);
            }
        }

        private SerializedResult ExecuteWrapped(MethodInfo func, ExecutorCallback callback, string argumentTypeName, string serializedArgument)
        {
            try
            {
                Action reportProgress = callback.Progress;
                reportProgress();

                object argument = Deserializer.Deserialize(argumentTypeName, serializedArgument);
                reportProgress();

                object result = func.Invoke(null, new[]
                {
                    _cancellationTokenSource.Token, reportProgress, argument
                });
                reportProgress();

                string serializedResult = Serializer.Serialize(result);
                return new SerializedResult(result.GetType().FullName, serializedResult);
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
