// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.ObjectModel;
using System.Text;

namespace Origam.Windows.Editor;

[Serializable()]
public class QualifiedNameCollection : Collection<QualifiedName>
{
    public QualifiedNameCollection() { }

    public QualifiedNameCollection(QualifiedNameCollection names)
    {
        AddRange(names: names);
    }

    public QualifiedNameCollection(QualifiedName[] names)
    {
        AddRange(names: names);
    }

    public bool HasItems
    {
        get { return Count > 0; }
    }
    public bool IsEmpty
    {
        get { return !HasItems; }
    }

    public override string ToString()
    {
        StringBuilder text = new StringBuilder();
        for (int i = 0; i < Count; i++)
        {
            if (i > 0)
            {
                text.Append(value: " > ");
            }
            text.Append(value: this[index: i].ToString());
        }
        return text.ToString();
    }

    public void AddRange(QualifiedName[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            Add(item: names[i]);
        }
    }

    public void AddRange(QualifiedNameCollection names)
    {
        for (int i = 0; i < names.Count; i++)
        {
            Add(item: names[index: i]);
        }
    }

    public void RemoveLast()
    {
        if (HasItems)
        {
            RemoveAt(index: Count - 1);
        }
    }

    public void RemoveFirst()
    {
        if (HasItems)
        {
            RemoveFirst(howMany: 1);
        }
    }

    public void RemoveFirst(int howMany)
    {
        if (howMany > Count)
        {
            howMany = Count;
        }
        while (howMany > 0)
        {
            RemoveAt(index: 0);
            --howMany;
        }
    }

    public string GetLastPrefix()
    {
        if (HasItems)
        {
            QualifiedName name = this[index: Count - 1];
            return name.Prefix;
        }
        return String.Empty;
    }

    public string GetNamespaceForPrefix(string prefix)
    {
        foreach (QualifiedName name in this)
        {
            if (name.Prefix == prefix)
            {
                return name.Namespace;
            }
        }
        return String.Empty;
    }

    public QualifiedName GetLast()
    {
        if (HasItems)
        {
            return this[index: Count - 1];
        }
        return null;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        QualifiedNameCollection rhs = obj as QualifiedNameCollection;
        if (rhs != null)
        {
            if (Count == rhs.Count)
            {
                for (int i = 0; i < Count; ++i)
                {
                    QualifiedName lhsName = this[index: i];
                    QualifiedName rhsName = rhs[index: i];
                    if (!lhsName.Equals(obj: rhsName))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        return false;
    }

    public string GetRootNamespace()
    {
        if (HasItems)
        {
            return this[index: 0].Namespace;
        }
        return String.Empty;
    }
}
