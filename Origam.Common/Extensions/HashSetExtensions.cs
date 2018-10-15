using System.Collections.Generic;

namespace Origam.Extensions
{
    public static class HashSetExtensions
    {
        public static void AddOrReplace<T>(this HashSet<T> hashSet, T item)
        {
            if (hashSet.Contains(item))
            {
                hashSet.Remove(item);
            }
            hashSet.Add(item);
        }
    }
}