using System;
using System.Collections.Generic;

namespace Origam.Extensions
{
    public static class ObjectExtensions
    {
        private static readonly Dictionary<Type, object> caschedDefaultTypes =
            new Dictionary<Type, object>();
        
        public static bool IsDefault(this object obj)
        {
            if (ReferenceEquals(obj, null)) return true;
            
            Type type = obj.GetType();
            if (type.IsValueType)
            {
                return obj.Equals(GetValueTypeDefault(type));
            }
            return false;
        }

        private static object GetValueTypeDefault(Type type)
        {
            if (caschedDefaultTypes.ContainsKey(type))
            {
                return caschedDefaultTypes[type];
            }
            object defValue = Activator.CreateInstance(type);
            caschedDefaultTypes.Add(type, defValue);
            return defValue;
        }
    }
}