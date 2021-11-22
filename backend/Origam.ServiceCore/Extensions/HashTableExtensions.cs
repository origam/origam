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