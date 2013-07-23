using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public class InstructionSpec<TArgument, TResult>
    {
        protected InstructionSpec(string assemblyName, string fqClassName, string methodName)
        {
            AssemblyName = assemblyName;
            FqClassName = fqClassName;
            MethodName = methodName;
        } 

        public static InstructionSpec<TArgument, TResult> StaticMethod(string assemblyName, string fqClassName, string methodName)
        {
            return new InstructionSpec<TArgument, TResult>(assemblyName, fqClassName, methodName);
        }

        public string AssemblyName { get; private set; }

        public string FqClassName { get; private set; }

        public string MethodName { get; private set; }

        /// <summary>
        /// Creates an actual (executable) Instruction from this specification
        /// </summary>
        /// <returns></returns>
        public Instruction<TArgument, TResult> Materialize()
        {
            Action act = () => { };
            MethodInfo mi = act.Method;
            var name = mi.Name;
            var type = mi.ReflectedType;
            var assy = Assembly.GetAssembly(type);

            throw new NotImplementedException();
        }
    }
}
