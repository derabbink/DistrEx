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
        private PluginManager _pluginManager;
        private Instruction<int, int> _identity;
        private Instruction<int, int> _progressIdentity;
        private Instruction<int, int> _haltingIdentity;
        private Instruction<Exception, Exception> _throw;

        private TwoPartInstruction<int, int> _twoPartIdentity;
        private TwoPartInstruction<int, int> _twoPartProgressIdentity;
        private TwoPartInstruction<int, int> _twoPartHaltingIdentity;
        private TwoPartInstruction<Exception, Exception> _twoPartThrow;

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

        #region Setup
        [SetUp]
        public void Setup()
        {
            _pluginManager = new PluginManager();
            TransportThisAssembly();
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

            _twoPartIdentity = (ct, progress, part1, argument) => argument;

            _twoPartProgressIdentity = (CancellationToken token, Action progress, Action part1, int argument) =>
            {
                progress();
                part1();
                return argument;
            };

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
        [ExpectedException(typeof(OperationCanceledException))]
        public void CancelExecution()
        {
            ManualResetEventSlim progressBlock = new ManualResetEventSlim(false);
            Action progress = progressBlock.Set;
            Task<int> t =
                Task<int>.Factory.StartNew(() =>
                {
                    SerializedResult result = _pluginManager.Execute(_haltingIdentityQualifiedName,
                                                                     _haltingIdentityMethodName,
                                                                     _cancellationTokenSource.Token, progress,
                                                                     _identityArgumentTypeName,
                                                                     _serializedIdentityArgument);
                    int castResult = (int)Deserializer.Deserialize(result.TypeName, result.Value);
                    return castResult;
                });
            //TODO ignore automatically sent progress heartbeats
            progressBlock.Wait();
            
            _cancellationTokenSource.Cancel();
            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                ExecutionException ee = (ExecutionException)e.InnerException;
                Exception dse = (Exception)Deserializer.Deserialize(ee.InnerExceptionTypeName, ee.SerializedInnerException);
                throw dse;
            }
        }

        [Test]
        [ExpectedException(typeof(OperationCanceledException))]
        public void CancelExecutionAsynPartTwo()
        {
            _twoPartHaltingIdentity = (CancellationToken token, Action p, Action part1, int argument) =>
            {
                p();
                part1();
                var block = new ManualResetEventSlim(false);
                block.Wait(token);
                p();
                return argument;
            };

            MethodInfo methodInfo = _twoPartHaltingIdentity.Method; 
            ManualResetEventSlim progressBlock = new ManualResetEventSlim(false);
            Action progress = progressBlock.Set;
            bool completed = false; 
            Action completed1 = () => completed = true; 
            Task<int> t = Task<int>.Factory.StartNew(() =>
                {
                    SerializedResult result = _pluginManager.ExecuteTwoStep(methodInfo.ReflectedType.AssemblyQualifiedName,
                                                                       methodInfo.Name,
                                                                     _cancellationTokenSource.Token, progress, completed1,
                                                                     _identityArgumentTypeName,
                                                                     _serializedIdentityArgument);
                    int castResult = (int)Deserializer.Deserialize(result.TypeName, result.Value);
                    return castResult;
                });
            progressBlock.Wait();
            
            _cancellationTokenSource.Cancel();
            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                ExecutionException ee = (ExecutionException) e.InnerException;
                Exception dse = (Exception) Deserializer.Deserialize(ee.InnerExceptionTypeName, ee.SerializedInnerException);
                throw dse;
            }
            finally
            {
                Assert.That(completed, Is.True);
            }
        }


        [Test]
        [ExpectedException(typeof(OperationCanceledException))]
        public void CancelExecutionAsynPartOne()
        {
            _twoPartHaltingIdentity = (CancellationToken token, Action p, Action part1, int argument) =>
            {
                p();
                var block = new ManualResetEventSlim(false);
                block.Wait(token);
                part1();
                p();
                return argument;
            };

            MethodInfo methodInfo = _twoPartHaltingIdentity.Method;
            ManualResetEventSlim progressBlock = new ManualResetEventSlim(false);
            Action progress = progressBlock.Set;
            bool completed = false;
            Action completed1 = () => completed = true;
            Task<int> t = Task<int>.Factory.StartNew(() =>
            {
                SerializedResult result = _pluginManager.ExecuteTwoStep(methodInfo.ReflectedType.AssemblyQualifiedName,
                                                                   methodInfo.Name,
                                                                 _cancellationTokenSource.Token, progress, completed1,
                                                                 _identityArgumentTypeName,
                                                                 _serializedIdentityArgument);
                int castResult = (int)Deserializer.Deserialize(result.TypeName, result.Value);
                return castResult;
            });
            progressBlock.Wait();

            _cancellationTokenSource.Cancel();
            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                ExecutionException ee = (ExecutionException)e.InnerException;
                Exception dse = (Exception)Deserializer.Deserialize(ee.InnerExceptionTypeName, ee.SerializedInnerException);
                throw dse;
            }
            finally
            {
                Assert.That(completed, Is.False);
            }
        }

        [Test]
        public void ExecuteCorrect()
        {
            int expected = _identityArgument;
            SerializedResult result =
                _pluginManager.Execute(_identityQualifiedName, _identityMethodName, _cancellationTokenSource.Token,
                                       () => {}, _identityArgumentTypeName, _serializedIdentityArgument);
            var actual = (int) Deserializer.Deserialize(result.TypeName, result.Value);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ExecuteCorrectAsync()
        {
            int expected = _identityArgument;
            MethodInfo methodInfo = _twoPartIdentity.Method;
            bool completed = false;
            Action completed1 = () => completed = true; 

            SerializedResult result =
                _pluginManager.ExecuteTwoStep(methodInfo.ReflectedType.AssemblyQualifiedName, methodInfo.Name,
                                             _cancellationTokenSource.Token, () => { }, completed1,
                                             _identityArgumentTypeName, _serializedIdentityArgument);
            var actual = (int)Deserializer.Deserialize(result.TypeName, result.Value);

            Assert.That(completed, Is.True);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Completed1Reported()
        {
            MethodInfo methodInfo = _twoPartProgressIdentity.Method;
            int expected = _identityArgument;
            bool completed1Reported = false;
            Action completed1 = () => completed1Reported = true; 
            SerializedResult result = _pluginManager.ExecuteTwoStep(methodInfo.ReflectedType.AssemblyQualifiedName, methodInfo.Name,
                                                             _cancellationTokenSource.Token, () => {}, completed1,
                                                             _identityArgumentTypeName, _serializedIdentityArgument);
            var actual = (int)Deserializer.Deserialize(result.TypeName, result.Value);
            Assert.That(completed1Reported, Is.True);
            Assert.That(actual, Is.EqualTo(expected));     
        }


        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ExecuteErrorAsync()
        {
            bool completed = false;
            Action completed1 = () => completed = true; 

            _twoPartThrow = (ct, p, p1, e) =>
            {
                throw e;
            };

            MethodInfo methodInfo = _twoPartThrow.Method; 
            try
            {
                _pluginManager.ExecuteTwoStep(methodInfo.ReflectedType.AssemblyQualifiedName, methodInfo.Name,
                                              _cancellationTokenSource.Token, () => {}, completed1,
                                              _throwArgumentTypeName, _serializedThrowArgument);
            }
            catch (ExecutionException e)
            {
                Exception inner = (Exception) Deserializer.Deserialize(e.InnerExceptionTypeName, e.SerializedInnerException);
                throw inner;
            }
            finally
            {
                Assert.That(completed, Is.False);
            }
        }
        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void ExecuteError()
        {
            try
            {
                _pluginManager.Execute(_throwQualifiedName, _throwMethodName, _cancellationTokenSource.Token, () => {},
                                       _throwArgumentTypeName, _serializedThrowArgument);
            }
            catch (ExecutionException e)
            {
                Exception inner = (Exception) Deserializer.Deserialize(e.InnerExceptionTypeName, e.SerializedInnerException);
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

        [Test]
        [ExpectedException(typeof(AppDomainUnloadedException))]
        public void ResettingWhileRunning()
        {
            ManualResetEventSlim progressBlock = new ManualResetEventSlim(false);
            Action progress = progressBlock.Set;
            Task<int> t =
                Task<int>.Factory.StartNew(() =>
                {
                    SerializedResult result = _pluginManager.Execute(_haltingIdentityQualifiedName,
                                                                     _haltingIdentityMethodName,
                                                                     _cancellationTokenSource.Token, progress,
                                                                     _identityArgumentTypeName,
                                                                     _serializedIdentityArgument);
                    int castResult = (int)Deserializer.Deserialize(result.TypeName, result.Value);
                    return castResult;
                });
            //TODO ignore automatically sent progress heartbeats
            progressBlock.Wait();

            _pluginManager.Reset();
            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                try
                {
                    throw e.InnerException;
                }
                catch (ExecutionException ex)
                {
                    AppDomainUnloadedException adue = (AppDomainUnloadedException) Deserializer.Deserialize(ex.InnerExceptionTypeName, ex.SerializedInnerException);
                    throw adue;
                }
            }

        }

        #region teardown
        [TearDown]
        public void Teardown()
        {
            _pluginManager.Dispose();
        }
        #endregion
    }
}
