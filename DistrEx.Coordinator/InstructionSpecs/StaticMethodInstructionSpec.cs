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
        private StaticMethodInstructionSpec(string assemblyName, string fqTypeName, string methodName)
        {
            AssemblyName = assemblyName;
            FqTypeName = fqTypeName;
            MethodName = methodName;
        }

        internal static StaticMethodInstructionSpec<TArgument, TResult> Create(string assemblyName, string fqTypeName,
                                                                      string methodName)
        {
            return new StaticMethodInstructionSpec<TArgument, TResult>(assemblyName, fqTypeName, methodName);
        }

        protected string AssemblyName { get; private set; }

        protected string FqTypeName { get; private set; }

        protected string MethodName { get; private set; }

        public override Instruction<TArgument, TResult> GetDelegate()
        {
            Type type = Type.GetType(String.Format("{0}, {1}", FqTypeName, AssemblyName));
            if (type == null)
                throw new NullReferenceException(String.Format("Could not find type {0} in assembly {1}", FqTypeName, AssemblyName));

            MethodInfo methodInfo = type.GetMethod(MethodName, BindingFlags.NonPublic
                                                               | BindingFlags.Public
                                                               | BindingFlags.Static);
            if (methodInfo == null)
                throw new NullReferenceException(String.Format("Could not find method {0} in class {1} in assembly {2}", MethodName, FqTypeName, AssemblyName));

            return (ct, p, arg) =>
                {
                    object res;
                    try
                    {
                        res = methodInfo.Invoke(null, new object[] {ct, p, arg});
                    }
                    catch (Exception e)
                    {
                        throw new Exception(
                            String.Format("Invoking method {0}.{1} in assembly {2} with argument of type {3}",
                                          FqTypeName, methodInfo, AssemblyName, arg.GetType().FullName), e);
                    }
                    TResult castRes;
                    try
                    {
                        castRes = (TResult) res;
                    }
                    catch (Exception e)
                    {
                        throw new Exception(
                            String.Format(
                                "Casting result (of type {0}) from method {1}.{2} in assembly {3} to type {4} failed.",
                                res.GetType().FullName, FqTypeName, MethodName, AssemblyName, typeof (TResult).FullName),
                            e);
                    }
                    return castRes;
                };
        }
    }
}
