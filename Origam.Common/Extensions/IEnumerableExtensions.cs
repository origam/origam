using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Origam.Extensions
{
    public static class IEnumerableExtensions
    {
        public static ArrayList ToArrayList(this IEnumerable iEnum)
        {
            var arrayList = new ArrayList();
            foreach (object obj in iEnum)
            {
                arrayList.Add(obj);
            }
            return arrayList;
        }

        public static List<T> ToList<T>(this IEnumerable iEnum) => 
            iEnum.Cast<T>().ToList();
    }
}