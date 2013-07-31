using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DependencyResolver;
using DistrEx.Common;
using DistrEx.Common.Serialization;
using NUnit.Framework;

namespace DistrEx.Plugin.Test.PluginManagerTests
{
    [TestFixture]
    public class PluginManagerTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _pluginManager = new PluginManager();
            _identityArgument = 1;
            _serializedIdentityArgument = Serializer.Serialize(_identityArgument);
            _identityArgumentTypeName = _identityArgument.GetType().FullName;
            _throwArgument = new Exception("Expected");
            _serializedThrowArgument = Serializer.Serialize(_throwArgument);
            _throwArgumentTypeName = _throwArgument.GetType().FullName;
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
                var block = new ManualResetEventSlim(false);
                block.Wait(ct);
                p();
                return i;
            };
            mi = _haltingIdentity.Method;
            _haltingIdentityQualifiedName = mi.ReflectedType.AssemblyQualifiedName;
            _haltingIdentityMethodName = mi.Name;

            _throw = (ct, p, e) =>
            {
                throw e;
            };
            mi = _throw.Method;
            _throwQualifiedName = mi.ReflectedType.AssemblyQualifiedName;
            _throwMethodName = mi.Name;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        #endregion

        private PluginManager _pluginManager;

        private Instruction<int, int> _identity;
        private Instruction<int, int> _progressIdentity;
        private Instruction<int, int> _haltingIdentity;
        private Instruction<Exception, Exception> _throw;
        private int _identityArgument;
        private string _serializedIdentityArgument;
        private string _identityArgumentTypeName;
        private Exception _throwArgument;
        private string _serializedThrowArgument;
        private string _throwArgumentTypeName;

        private string _identityQualifiedName;
        private string _identityMethodName;
        private string _progressIdentityQualifiedName;
        private string _progressIdentityMethodName;
        private string _haltingIdentityQualifiedName;
        private string _haltingIdentityMethodName;
        private string _throwQualifiedName;
        private string _throwMethodName;

        private CancellationTokenSource _cancellationTokenSource;

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

        [Test]
        public void CancelExecution()
        {
            var progressBlock = new ManualResetEventSlim(false);
            Action progress = progressBlock.Set;
            Task<int> t =
                Task<int>.Factory.StartNew(() =>
                {
                    SerializedResult result = _pluginManager.Execute(_haltingIdentityQualifiedName,
                                                                     _haltingIdentityMethodName,
                                                                     _cancellationTokenSource.Token, progress,
                                                                     _identityArgumentTypeName,
                                                                     _serializedIdentityArgument);
                    var castResult = (int) Deserializer.Deserialize(result.TypeName, result.Value);
                    return castResult;
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

        [Test]
        public void ExecuteCorrect()
        {
            int expected = _identityArgument;
            SerializedResult result =
                _pluginManager.Execute(_identityQualifiedName, _identityMethodName, _cancellationTokenSource.Token,
                                       () =>
                                       {
                                       }, _identityArgumentTypeName, _serializedIdentityArgument);
            var actual = (int) Deserializer.Deserialize(result.TypeName, result.Value);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ExecuteError()
        {
            try
            {
                _pluginManager.Execute(_throwQualifiedName, _throwMethodName, _cancellationTokenSource.Token, () =>
                {
                },
                                       _throwArgumentTypeName, _serializedThrowArgument);
            }
            catch (ExecutionException e)
            {
                var inner = (Exception) Deserializer.Deserialize(e.InnerExceptionTypeName, e.InnerExceptionTypeName);
                throw inner;
            }
        }

        [Test]
        public void ProgressReporting()
        {
            int expected = _identityArgument;
            bool progressReported = false;
            Action progress = () => progressReported = true;
            SerializedResult result = _pluginManager.Execute(_progressIdentityQualifiedName, _progressIdentityMethodName,
                                                             _cancellationTokenSource.Token, progress,
                                                             _identityArgumentTypeName, _serializedIdentityArgument);
            var actual = (int) Deserializer.Deserialize(result.TypeName, result.Value);
            Assert.That(progressReported, Is.True);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
