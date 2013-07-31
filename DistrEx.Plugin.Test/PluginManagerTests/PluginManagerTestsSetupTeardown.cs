using System;
using System.IO;
using System.Reflection;
using DependencyResolver;
using NUnit.Framework;

namespace DistrEx.Plugin.Test.PluginManagerTests
{
    [SetUpFixture]
    public class PluginManagerTestsSetupTeardown
    {
        private PluginManager _pluginManager;

        [SetUp]
        public void Setup()
        {
            _pluginManager = new PluginManager();
            //apparently this is not necessary
            //TransportThisAssembly();
        }

        private void TransportThisAssembly()
        {
            Assembly thisAssembly = GetType().Assembly;
            IObservable<AssemblyName> dependencies = Resolver.GetAllDependencies(thisAssembly.GetName());
            dependencies.Subscribe(aName =>
            {
                String path = new Uri(aName.CodeBase).LocalPath;
                using (Stream assyFileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    _pluginManager.Load(assyFileStream, aName.Name);
                }
            });
        }

        [TearDown]
        public void Teardown()
        {
            //_pluginManager.Reset();
        }
    }
}
