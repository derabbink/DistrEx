using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using DependencyResolver;

namespace DistrEx.Plugin.Test
{
    [TestFixture]
    public class PluginManagerTest
    {
        private PluginManager _pluginManager;
        private Func<int, int> _identity;
        private Func<Exception, Exception> _throw;
        private int _identityArgument = 1;
        private Exception _throwArgument;

        private string _identityQualifiedName;
        private string _identityMethodName;
        private string _throwQualifiedName;
        private string _throwMethodName;

        #region setup

        [SetUp]
        public void Setup()
        {
            _pluginManager = new PluginManager();
            TransportThisAssembly();
            _identityArgument = 1;
            _throwArgument = new Exception("Expected");
            _identity = i => i;
            MethodInfo mi = _identity.Method;
            _identityQualifiedName = mi.ReflectedType.AssemblyQualifiedName;
            _identityMethodName = mi.Name;

            _throw = e => { throw e; };
            mi = _throw.Method;
            _throwQualifiedName = mi.ReflectedType.AssemblyQualifiedName;
            _throwMethodName = mi.Name;
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

        #endregion

        #region tests

        [Test]
        public void ExecuteCorrect()
        {
            var expected = _identityArgument;
            int actual = (int) _pluginManager.Execute(_identityQualifiedName, _identityMethodName, _identityArgument);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ExecuteError()
        {
            _pluginManager.Execute(_throwQualifiedName, _throwMethodName, _throwArgument);
        }

        #endregion

        #region teardown
        
        [TearDown]
        public void Teardown()
        {
            _pluginManager.Reset();
        }

        #endregion

    }
}
