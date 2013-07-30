using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DistrEx.Common.Serialization
{
    public static class Serializer
    {
        public static string Serialize(object value)
        {
            Contract.Requires(value != null);
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(mStream, value);
                    return Convert.ToBase64String(mStream.ToArray());
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Serializing type {0} failed", value.GetType()), e);
            }
        }
    }
}
