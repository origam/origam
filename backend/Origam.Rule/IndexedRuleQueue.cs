using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Origam.DA;
using Origam.Schema.EntityModel;

namespace Origam.Rule;

public class IndexedRuleQueue: IEnumerable<object[]>
{
    private readonly HashSet<int> hashSet = new ();
    private readonly Queue<object[]> queue = new ();

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
        object[] keys = DatasetTools.PrimaryKey(row);
        foreach (object key in keys)
        {
            hash.Add(key);
        }
        hash.Add(ruleSet.Id);
        return hash.ToHashCode();
    }

    private int GetHash(object[] entry)
    {
        var row = entry[0] as DataRow;
        var ruleSet = entry[1] as DataStructureRuleSet;
        return GetHash(row, ruleSet);
    }
}