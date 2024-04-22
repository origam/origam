#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Origam.Extensions;

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
            
    public static T[] ToArray<T>(this IEnumerable iEnum) => 
        iEnum.Cast<T>().ToArray();
        
    public static IEnumerable<T> Peek<T>(this IEnumerable<T> source,
        Action<T>
            action)
    {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            return Iterator();

            IEnumerable<T> Iterator() 
            {
                foreach (var item in source)
                {
                    action(item);
                    yield return item;
                }
            }
        }
}