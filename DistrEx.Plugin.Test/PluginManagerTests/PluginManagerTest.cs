using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DistrEx.Common;
using NUnit.Framework;
using DependencyResolver;

namespace DistrEx.Plugin.Test.PluginManagerTests
{
    [TestFixture]
    public class PluginManagerTest
    {
        private PluginManager _pluginManager;

        private Instruction<int, int> _identity;
        private Instruction<int, int> _progressIdentity;
        private Instruction<int, int> _haltingIdentity;
        private Instruction<Exception, Exception> _throw;
        private int _identityArgument = 1;
        private Exception _throwArgument;

        private string _identityQualifiedName;
        private string _identityMethodName;
        private string _progressIdentityQualifiedName;
        private string _progressIdentityMethodName;
        private string _haltingIdentityQualifiedName;
        private string _haltingIdentityMethodName;
        private string _throwQualifiedName;
        private string _throwMethodName;

        private CancellationTokenSource _cancellationTokenSource;

        #region setup

        [SetUp]
        public void Setup()
        {
            _pluginManager = new PluginManager();
            _identityArgument = 1;
            _throwArgument = new Exception("Expected");
            _identity = (ct, p, i) => i;
            MethodInfo mi = _identity.Method;
            _identityQualifiedName = mi.ReflectedType.AssemblyQualifiedName;
            _identityMethodName = mi.Name;

            //need explicit types: http://stackoverflow.com/questions/4466859
            _progressIdentity = (CancellationToken ct, Action p, int i) =>
                {
                    p();
                    return i;
                };
            mi = _progressIdentity.Method;
            _progressIdentityQualifiedName = mi.ReflectedType.AssemblyQualifiedName;
            _progressIdentityMethodName = mi.Name;

            //need explicit types: http://stackoverflow.com/questions/4466859
            _haltingIdentity = (CancellationToken ct, Action p, int i) =>
            {
                p();
                ManualResetEventSlim block = new ManualResetEventSlim(false);
                block.Wait(ct);
                p();
                return i;
            };
            mi = _haltingIdentity.Method;
            _haltingIdentityQualifiedName = mi.ReflectedType.AssemblyQualifiedName;
            _haltingIdentityMethodName = mi.Name;

            _throw = (ct, p, e) => { throw e; };
            mi = _throw.Method;
            _throwQualifiedName = mi.ReflectedType.AssemblyQualifiedName;
            _throwMethodName = mi.Name;

            _cancellationTokenSource = new CancellationTokenSource();
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
            int actual = (int) _pluginManager.Execute(_identityQualifiedName, _identityMethodName, _cancellationTokenSource.Token, () => {}, _identityArgument);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ExecuteError()
        {
            _pluginManager.Execute(_throwQualifiedName, _throwMethodName, _cancellationTokenSource.Token, () => {}, _throwArgument);
        }

        [Test]
        public void ProgressReporting()
        {
            var expected = _identityArgument;
            bool progressReported = false;
            Action progress = () => progressReported = true;
            int actual = (int)_pluginManager.Execute(_progressIdentityQualifiedName, _progressIdentityMethodName, _cancellationTokenSource.Token, progress, _identityArgument);
            Assert.That(progressReported, Is.True);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CancelExecution()
        {
            ManualResetEventSlim progressBlock = new ManualResetEventSlim(false);
            Action progress = progressBlock.Set;
            Task<int> t =
                Task<int>.Factory.StartNew(() =>
                    {
                        int result = (int) _pluginManager.Execute(_haltingIdentityQualifiedName, _haltingIdentityMethodName,
                                                                  _cancellationTokenSource.Token, progress, _identityArgument);
                        return result;
                    });
            progressBlock.Wait();
            progressBlock.Reset();
            _cancellationTokenSource.Cancel();
            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                Assert.That(e.InnerException, Is.TypeOf<OperationCanceledException>());
            }
            Assert.That(progressBlock.IsSet, Is.False);
        }

        #endregion

    }
}
