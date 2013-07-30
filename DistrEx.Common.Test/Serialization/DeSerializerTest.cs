using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistrEx.Common.Serialization;
using NUnit.Framework;

namespace DistrEx.Common.Test.Serialization
{
    [TestFixture]
    public class DeSerializerTest
    {
        [Test]
        public void DeserializeInt()
        {
            int expected = 1;
            string serialized = Serializer.Serialize(expected);

            int actual = (int) Deserializer.Deserialize(expected.GetType().FullName, serialized);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DeserializeException()
        {
            Exception expected = new Exception("Expected");
            string serialized = Serializer.Serialize(expected);

            Exception actual = (Exception) Deserializer.Deserialize(expected.GetType().FullName, serialized);
            Assert.That(actual.Message, Is.EqualTo(expected.Message));
        }
    }
}
