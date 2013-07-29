using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using DistrEx.Common;
using DistrEx.Communication.Contracts.Service;
using DistrEx.Communication.Service.Executor;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.TargetSpecs;
using DistrEx.Coordinator.Test.Util;
using Microsoft.Test.ApplicationControl;
using NUnit.Framework;

namespace DistrEx.Coordinator.Test.TargetSpecs
{
    [TestFixture]
    public class OnWorkerTest
    {
        private AutomatedApplication _workerProcess;
        private IExecutorCallback _callbackHandler;
        private TargetSpec _onWorker;

        private Instruction<int, int> _identity;
        private Instruction<Exception, Exception> _throw;
        private int _argumentIdentity;
        private Exception _argumentThrow;

        #region setup
        [TestFixtureSetUp]
        public void SetupFixture()
        {
            //_workerProcess = ProcessHelper.Start(ConfigurationManager.AppSettings.Get("DistrEx.Coordinator.Test.worker-exe-file"));
            _callbackHandler = new ExecutorCallbackService();
            _onWorker = OnWorker.FromEndpointConfigNames("localhost-assemblyManager", "localhost-executor", _callbackHandler);

            ConfigureOperations();
        }

        private void ConfigureOperations()
        {
            _identity = (ct, p, i) => i;
            _throw = (ct, _, e) => { throw e; };
            _argumentIdentity = 1;
            _argumentThrow = new Exception("Expected");
        }
        #endregion

        #region tests

        [Test]
        public void SuccessfulOnWorker()
        {
            int expected = _argumentIdentity;
            int actual = Interface.Coordinator.Do(_onWorker.Do(_identity), _argumentIdentity).ResultValue;
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion

        #region teardown
        [TearDown]
        public void Teardown()
        {
            _onWorker.ClearAssemblies();
        }

        [TestFixtureTearDown]
        public void TeardownFixture()
        {
            //ProcessHelper.Stop(_workerProcess);
        }
        #endregion
    }
}
