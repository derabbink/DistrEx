using System.Reflection;

namespace DistrEx.Coordinator.Interface
{
    public abstract class Spec
    {
        public abstract Assembly GetAssembly();

        public abstract string GetAssemblyQualifiedName();

        public abstract string GetMethodName();
    }
}