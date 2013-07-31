using System;

namespace DistrEx.Plugin
{
    [Serializable]
    public class SerializedResult
    {
        public SerializedResult(string typeName, string value)
        {
            TypeName = typeName;
            Value = value;
        }

        public string TypeName
        {
            get;
            private set;
        }

        public string Value
        {
            get;
            private set;
        }
    }
}
