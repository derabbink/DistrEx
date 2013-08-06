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
        internal SerializedResult Execute(ExecutorCallback callback, string assemblyQualifiedName, string methodName, string argumentTypeName, string serializedArgument){
            Type t = Type.GetType(assemblyQualifiedName, true);
            MethodInfo func = t.GetMethod(methodName, BindingFlags.NonPublic
                                                      | BindingFlags.Public
                                                      | BindingFlags.Static);
            try
            {
                Action progressCallback = callback.Callback;
                return ExecuteWrapped(func, callback, argumentTypeName, serializedArgument, new[]
                    {
                        _cancellationTokenSource.Token,
                        progressCallback,
                        default(object) //placeholder for deserialized argument
                    });
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="completedStep1"></param>
        /// <param name="assemblyQualifiedName"></param>
        /// <param name="methodName"></param>
        /// <param name="argumentTypeName"></param>
        /// <param name="serializedArgument"></param>
        /// <returns>serialized result</returns>
        /// <exception cref="ExecutionException">If something went wrong with the execution</exception>
        internal SerializedResult ExecuteTwoStep(ExecutorCallback callback, ExecutorCallback completedStep1, string assemblyQualifiedName, string methodName, string argumentTypeName, string serializedArgument)
        {
            Type t = Type.GetType(assemblyQualifiedName, true);
            MethodInfo func = t.GetMethod(methodName, BindingFlags.NonPublic
                                                      | BindingFlags.Public
                                                      | BindingFlags.Static);
            try
            {
                Action progressCallback = callback.Callback;
                Action completedStep1Callback = completedStep1.Callback;
                return ExecuteWrapped(func, callback, argumentTypeName, serializedArgument, new[]
                    {
                        _cancellationTokenSource.Token,
                        progressCallback,
                        completedStep1Callback,
                        default(object) //placeholder for deserialized argument
                    });
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }
        }

        private SerializedResult ExecuteWrapped(MethodInfo func, ExecutorCallback callback, string argumentTypeName, string serializedArgument, object[] arguments)
        {
            try
            {
                Action reportProgress = callback.Callback;
                reportProgress();

                object argument = Deserializer.Deserialize(argumentTypeName, serializedArgument);
                arguments[arguments.Length - 1] = argument;
                reportProgress();

                object result = func.Invoke(null, arguments);
                reportProgress();

                string serializedResult = Serializer.Serialize(result);
                return new SerializedResult(result.GetType().FullName, serializedResult);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        private ExecutionException HandleException(Exception e)
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
            return new ExecutionException(serializedExTypeName, serializedEx);
        }

        internal void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
