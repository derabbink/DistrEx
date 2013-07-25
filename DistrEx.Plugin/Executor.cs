using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DistrEx.Plugin
{
    internal class Executor : MarshalByRefObject
    {
        //constructur is public for reflection
        public Executor()
        {
        }

        internal static Executor CreateInstanceInAppDomain(AppDomain domain)
        {
            AssemblyName ownAssyName = Assembly.GetExecutingAssembly().GetName();
            string typename = typeof(Executor).FullName;
            return domain.CreateInstanceAndUnwrap(ownAssyName.FullName, typename) as Executor;
        }

        internal object Execute(string assemblyQualifiedName, string actionName)
        {
            Type t = Type.GetType(assemblyQualifiedName, true);
            object instance = Activator.CreateInstance(t);
            MethodInfo action = t.GetMethod(actionName);
            return action.Invoke(instance, new object[] { });
        }
    }
}
