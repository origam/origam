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
using System.Collections.Generic;
using System.Linq;

namespace Origam.Extensions;

public static class DictionaryExtensions
{
    /// <summary>
    /// Will fail if source dictionary contains duplicate values!
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Dictionary<TValue, TKey> Invert<TKey, TValue>(
        this IDictionary<TKey, TValue> source
    )
    {
        bool duplicatevaluesExist = source.Values.Distinct().Count() != source.Values.Count;
        if (duplicatevaluesExist)
        {
            throw new ArgumentException(
                "Dictionary cannot be inverted because it contains duplicate values."
            );
        }
        var invertedDict = new Dictionary<TValue, TKey>();
        foreach (var entry in source)
        {
            invertedDict[entry.Value] = entry.Key;
        }
        return invertedDict;
    }

    public static List<KeyValuePair<K, V>> RemoveByValueSelector<K, V>(
        this IDictionary<K, V> dict,
        Func<V, bool> valueSelectorFunc
    )
        where V : class
    {
        List<KeyValuePair<K, V>> entriesToRemove = dict.Where(entry =>
                valueSelectorFunc.Invoke(entry.Value)
            )
            .ToList();

        entriesToRemove.ForEach(entryToRemove => dict.Remove(entryToRemove));
        return entriesToRemove;
    }

    public static List<KeyValuePair<K, V>> RemoveByKeySelector<K, V>(
        this IDictionary<K, V> dict,
        Func<K, bool> keySelectorFunc
    )
        where V : class
    {
        List<KeyValuePair<K, V>> entriesToRemove = dict.Where(entry =>
                keySelectorFunc.Invoke(entry.Key)
            )
            .ToList();

        entriesToRemove.ForEach(entryToRemove => dict.Remove(entryToRemove));
        return entriesToRemove;
    }

    public static void RemoveIfPresent<K, V>(this IDictionary<K, V> dict, K key)
    {
        if (dict.ContainsKey(key))
        {
            dict.Remove(key);
        }
    }

    public static void AddOrReplace<K, V>(this Dictionary<K, V> dict, K key, V value)
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = value;
        }
        else
        {
            dict.Add(key, value);
        }
    }

    public static void AddOrReplaceRange<K, V>(
        this IDictionary<K, V> dict,
        IDictionary<K, V> otherDict
    )
    {
        if (otherDict == null)
        {
            return;
        }
        foreach (var keyValuePair in otherDict)
        {
            dict[keyValuePair.Key] = keyValuePair.Value;
        }
    }

    public static string Print<K, V>(this IDictionary<K, V> dict, bool inLine = false)
    {
        if (inLine)
        {
            return dict.Select(x => x.Key + ": " + x.Value).Aggregate("{", (x, y) => $"{x}, {y}")
                + "}";
        }
        return dict.Select(x => x.Key + ": " + x.Value).Aggregate("\t\t", (x, y) => $"{x}\n\t\t{y}")
            + "\n";
    }
}
