using System;
using System.Collections;

namespace Origam.ServiceCore
{
    public static class HashTableExtensions
    {
        public static T TryGet<T>(this Hashtable table, string key)
        {
            if (table.Contains(key))
            {
                object value = table[key];
                if (value is T t)
                {
                    return t;
                }
            }
            return default(T);
        }

        public static T Get<T>(this Hashtable table, string key)
        {
            if (!table.Contains(key))
            {
                throw new ArgumentOutOfRangeException(
                    $"Missing key {key}");
            }
            object value = table[key];
            if (value is T t)
            {
                return t;
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Key {key} should be of type {typeof(T)}");
            }
        } 
    }
}