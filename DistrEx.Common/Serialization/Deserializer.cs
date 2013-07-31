using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DistrEx.Common.Serialization
{
    public static class Deserializer
    {
        public static object Deserialize(string typeName, string value)
        {
            Contract.Requires(typeName != null);
            Contract.Requires(value != null);
            try
            {
                byte[] bytes = Convert.FromBase64String(value);
                using (var mStream = new MemoryStream(bytes))
                {
                    return new BinaryFormatter().Deserialize(mStream);
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Deserializing (of type {0}) failed", typeName), e);
            }
        }
    }
}
