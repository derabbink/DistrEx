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
            Action progressCallback = callback.Callback;
            return ExecuteWrapped(func, callback, argumentTypeName, serializedArgument, new[]
            {
                _cancellationTokenSource.Token,
                progressCallback,
                default(object) //placeholder for deserialized argument
            });

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
            Action progressCallback = callback.Callback;
            Action completedStep1Callback = completedStep1.Callback;
            Action onetimeCompletedStep1Callback = () =>
                {
                    if (completedStep1Callback != null)
                        completedStep1Callback();
                    completedStep1Callback = null;
                };
            var result = ExecuteWrapped(func, callback, argumentTypeName, serializedArgument, new[]
            {
                _cancellationTokenSource.Token,
                progressCallback,
                onetimeCompletedStep1Callback,
                default(object) //placeholder for deserialized argument
            });
            onetimeCompletedStep1Callback();
            return result;
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
                throw ExecutionException.FromException(e.InnerException);
            }
            catch (Exception e)
            {
                throw ExecutionException.FromException(e);
            }
        }

        internal void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
