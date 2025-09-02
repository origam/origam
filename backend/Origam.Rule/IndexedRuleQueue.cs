#region license
/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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
using System.Data;
using Origam.Schema.EntityModel;

namespace Origam.Rule;

public class IndexedRuleQueue : IEnumerable<object[]>
{
    private readonly HashSet<int> hashSet = new();
    private readonly Queue<object[]> queue = new();

    public int Count => queue.Count;

    public void Enqueue(object[] entry)
    {
        hashSet.Add(GetHash(entry));
        queue.Enqueue(entry);
    }

    public bool Contains(DataRow row, DataStructureRuleSet ruleSet)
    {
        int hash = GetHash(row, ruleSet);
        return hashSet.Contains(hash);
    }

    public object[] Peek()
    {
        return queue.Peek();
    }

    public object[] Dequeue()
    {
        var entry = queue.Dequeue();
        int hash = GetHash(entry);
        hashSet.Remove(hash);
        return entry;
    }

    public void Clear()
    {
        queue.Clear();
        hashSet.Clear();
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        return queue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private int GetHash(DataRow row, DataStructureRuleSet ruleSet)
    {
        HashCode hash = new();
        hash.Add(row.GetHashCode());
        hash.Add(ruleSet?.Id);
        return hash.ToHashCode();
    }

    private int GetHash(object[] entry)
    {
        var row = entry[0] as DataRow;
        var ruleSet = entry[1] as DataStructureRuleSet;
        return GetHash(row, ruleSet);
    }
}
