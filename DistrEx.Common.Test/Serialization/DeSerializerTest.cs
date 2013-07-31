using System;
using DistrEx.Common.Serialization;
using NUnit.Framework;

namespace DistrEx.Common.Test.Serialization
{
    [TestFixture]
    public class DeSerializerTest
    {
        [Test]
        public void DeserializeException()
        {
            var expected = new Exception("Expected");
            string serialized = Serializer.Serialize(expected);

            var actual = (Exception) Deserializer.Deserialize(expected.GetType().FullName, serialized);
            Assert.That(actual.Message, Is.EqualTo(expected.Message));
        }

        [Test]
        public void DeserializeInt()
        {
            int expected = 1;
            string serialized = Serializer.Serialize(expected);

            var actual = (int) Deserializer.Deserialize(expected.GetType().FullName, serialized);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
