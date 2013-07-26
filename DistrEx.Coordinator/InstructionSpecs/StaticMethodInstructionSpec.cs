using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;

namespace DistrEx.Coordinator.InstructionSpecs
{
    internal class StaticMethodInstructionSpec<TArgument, TResult> : InstructionSpec<TArgument, TResult>
    {
        private StaticMethodInstructionSpec(string assemblyQualifiedName, string methodName)
        {
            AssemblyQualifiedName = assemblyQualifiedName;
            MethodName = methodName;
        }

        internal static StaticMethodInstructionSpec<TArgument, TResult> Create(string assemblyQualifiedName,
                                                                               string methodName)
        {
            return new StaticMethodInstructionSpec<TArgument, TResult>(assemblyQualifiedName, methodName);
        }

        protected string AssemblyQualifiedName { get; private set; }

        protected string MethodName { get; private set; }

        public override Instruction<TArgument, TResult> GetDelegate()
        {
            throw new NotImplementedException("This method should only be used on non-transferrable instruction specs that contain actual delegates");
//            Type type = Type.GetType(AssemblyQualifiedName);
//            if (type == null)
//                throw new NullReferenceException(String.Format("Could not find type {0}", AssemblyQualifiedName));
//
//            MethodInfo methodInfo = type.GetMethod(MethodName, BindingFlags.NonPublic
//                                                               | BindingFlags.Public
//                                                               | BindingFlags.Static);
//            if (methodInfo == null)
//                throw new NullReferenceException(String.Format("Could not find method {0} in type {1}", MethodName, AssemblyQualifiedName));
//
//            return (ct, p, arg) =>
//                {
//                    object res;
//                    try
//                    {
//                        res = methodInfo.Invoke(null, new object[] {ct, p, arg});
//                    }
//                    catch (Exception e)
//                    {
//                        throw new Exception(
//                            String.Format("Invoking method {0} in type {1} with argument of type {2}",
//                                          MethodName, AssemblyQualifiedName, arg.GetType().FullName), e);
//                    }
//                    TResult castRes;
//                    try
//                    {
//                        castRes = (TResult) res;
//                    }
//                    catch (Exception e)
//                    {
//                        throw new Exception(
//                            String.Format(
//                                "Casting result (of type {0}) from method {1} in type {2} to type {3} failed.",
//                                res.GetType().FullName, MethodName, AssemblyQualifiedName, typeof (TResult).FullName),
//                            e);
//                    }
//                    return castRes;
//                };
        }

        public override Assembly GetAssembly()
        {
            return Type.GetType(AssemblyQualifiedName).Assembly;
        }

        public override string GetAssemblyQualifiedName()
        {
            return AssemblyQualifiedName;
        }

        public override string GetMethodName()
        {
            return MethodName;
        }
    }
}
