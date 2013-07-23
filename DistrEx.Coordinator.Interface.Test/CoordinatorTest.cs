using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DistrEx.Coordinator.Interface.Test
{
    [TestFixture]
    public class CoordinatorTest
    {
        private InstructionSpec<object, object> _identity;
        private TargetSpec _local;

        #region setup

        [SetUp]
        public void Setup()
        {
//            _identity = InstructionSpec<object, object>.StaticMethod();
//            _local = TargetSpec.
        }

        #endregion

        #region tests

        [Test]
        public void DoSuccessful()
        {
            object expected = new object();
            object actual = Coordinator.Do(_local.Do(_identity), expected);
            Assert.That(actual, Is.SameAs(expected));
        }

        #endregion
    }
}
