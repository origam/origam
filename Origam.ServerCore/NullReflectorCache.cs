namespace Origam.ServerCore
{
    class NullReflectorCache : IReflectorCache
    {
        public object InvokeObject(string classname, string assembly)
        {
            return null;
        }

        public object InvokeObject(string typeName, object[] args)
        {
            return null;
        }

        public bool SetValue(object instance, string property, object value)
        {
            return false;
        }
    }
}