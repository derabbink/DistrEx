using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using DistrEx.Common;
using DistrEx.Coordinator.Interface;
using DependencyResolver;

namespace DistrEx.Coordinator.TargetSpecs
{
    public class OnWorker : TargetSpec
    {
        private ISet<AssemblyName> _transportedAssemblies;

        private OnWorker()
        {
            _transportedAssemblies = new HashSet<AssemblyName>();
        }

        public static OnWorker FromEndpointConfigName(string endpointConfigName)
        {
            //TODO
            return new OnWorker();
        }

        public override void TransportAssemblies<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction)
        {
            Assembly assy = instruction.GetAssembly();
            Resolver.GetAllDependencies(assy.GetName())
                    .Where(aName => !_transportedAssemblies.Contains(aName))
                    .Do(TransportAssembly);
            throw new NotImplementedException();
        }

        private void TransportAssembly(AssemblyName assemblyName)
        {
            throw new NotImplementedException();
        }

        public override Future<TResult> Invoke<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction, CancellationToken cancellationToken, TArgument argument)
        {
            throw new NotImplementedException();
        }
    }
}
