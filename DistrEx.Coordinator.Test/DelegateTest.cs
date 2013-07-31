using System;
using System.Reflection;
using NUnit.Framework;

namespace DistrEx.Coordinator.Test
{
    [TestFixture]
    public class DelegateTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            SetupNonClosureMethod();
            SetupClosureMethod();
        }

        #endregion

        private Func<int, int> _nonClosureDelegate;
        private MethodInfo _nonClosureMethodInfo;
        private string _nonClosureMethodName;
        private string _nonClosureAssemblyQualifiedName;

        private int _closureObj1;
        private object _closureObj2;
        private Func<int, int> _closureDelegate;
        private MethodInfo _closureMethodInfo;
        private string _closureMethodName;
        private string _closureAssemblyQualifiedName;

        private void SetupNonClosureMethod()
        {
            _nonClosureDelegate = i => i;
            _nonClosureMethodInfo = _nonClosureDelegate.Method;
            _nonClosureMethodName = _nonClosureMethodInfo.Name;
            _nonClosureAssemblyQualifiedName = _nonClosureMethodInfo.ReflectedType.AssemblyQualifiedName;
        }

        private void SetupClosureMethod()
        {
            _closureObj1 = 1;
            _closureObj2 = null;
            _closureDelegate = i =>
            {
                _closureObj2 = new object();
                return i + _closureObj1;
            };
            _closureMethodInfo = _closureDelegate.Method;
            _closureMethodName = _closureMethodInfo.Name;
            _closureAssemblyQualifiedName = _closureMethodInfo.ReflectedType.AssemblyQualifiedName;
        }

        [Test]
        public void ClosureDelegateSignature()
        {
            Type constructedType = Type.GetType(_closureAssemblyQualifiedName);
            MethodInfo mi = constructedType.GetMethod(_closureMethodName, BindingFlags.NonPublic
                                                                          | BindingFlags.Public
                                                                          | BindingFlags.Instance);
            ParameterInfo[] pis = mi.GetParameters();
            Assert.That(pis.Length, Is.EqualTo(1));
            Assert.That(pis[0].ParameterType, Is.SameAs(typeof(int)));
            Assert.That(mi.ReturnType, Is.SameAs(typeof(int)));
        }

        [Test]
        [ExpectedException(typeof(TargetException))]
        public void InvokeClosureDelegate()
        {
            Type constructedType = Type.GetType(_closureAssemblyQualifiedName);
            MethodInfo mi = constructedType.GetMethod(_closureMethodName, BindingFlags.NonPublic
                                                                          | BindingFlags.Public
                                                                          | BindingFlags.Instance);
            int arg = 1;
            int expected = arg;
            object actual = mi.Invoke(null, new object[]
            {
                arg
            });
        }

        [Test]
        public void InvokeNonClosureDelegate()
        {
            Type constructedType = Type.GetType(_nonClosureAssemblyQualifiedName);
            MethodInfo mi = constructedType.GetMethod(_nonClosureMethodName, BindingFlags.NonPublic
                                                                             | BindingFlags.Public
                                                                             | BindingFlags.Static);
            int arg = 1;
            int expected = arg;
            object actual = mi.Invoke(null, new object[]
            {
                arg
            });
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void NonClosureDelegateSignature()
        {
            Type constructedType = Type.GetType(_nonClosureAssemblyQualifiedName);
            MethodInfo mi = constructedType.GetMethod(_nonClosureMethodName, BindingFlags.NonPublic
                                                                             | BindingFlags.Public
                                                                             | BindingFlags.Static);
            ParameterInfo[] pis = mi.GetParameters();
            Assert.That(pis.Length, Is.EqualTo(1));
            Assert.That(pis[0].ParameterType, Is.SameAs(typeof(int)));
            Assert.That(mi.ReturnType, Is.SameAs(typeof(int)));
        }
    }
}
