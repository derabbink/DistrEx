using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

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
                using (MemoryStream mStream = new MemoryStream(bytes))
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
