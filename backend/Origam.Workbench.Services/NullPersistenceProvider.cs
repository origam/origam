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
using Origam.DA;
using Origam.DA.ObjectPersistence;

namespace Origam.Workbench.Services;

public class NullPersistenceProvider : IPersistenceProvider
{
    public object Clone()
    {
        throw new NotImplementedException();
    }

    public void Dispose() { }

    public event EventHandler<IPersistent> InstancePersisted
    {
        add { }
        remove { }
    }

    public void OnInstancePersisted(object sender) { }

    public void OnTransactionEnded(object sender)
    {
        throw new NotImplementedException();
    }

    public ICompiledModel CompiledModel { get; set; }

    public object RetrieveInstance(Type type, Key primaryKey)
    {
        return null;
    }

    public object RetrieveInstance(Type type, Key primaryKey, bool useCache)
    {
        return null;
    }

    public object RetrieveInstance(
        Type type,
        Key primaryKey,
        bool useCache,
        bool throwNotFoundException
    )
    {
        return null;
    }

    public T RetrieveInstance<T>(Guid instanceId)
    {
        return default;
    }

    public T RetrieveInstance<T>(Guid instanceId, bool useCache)
    {
        return default;
    }

    public T RetrieveInstance<T>(Guid instanceId, bool useCache, bool throwNotFoundException)
    {
        return default;
    }

    public void RefreshInstance(IPersistent persistentObject) { }

    public void RemoveFromCache(IPersistent instance) { }

    public List<T> RetrieveList<T>(IDictionary<string, object> filter = null)
    {
        return new System.Collections.Generic.List<T>();
    }

    public List<T> RetrieveListByCategory<T>(string category)
    {
        return new System.Collections.Generic.List<T>();
    }

    public List<T> RetrieveListByPackage<T>(Guid packageId)
    {
        return new System.Collections.Generic.List<T>();
    }

    public T[] FullTextSearch<T>(string text)
    {
        return new T[0];
    }

    public List<T> RetrieveListByParent<T>(
        Key primaryKey,
        string parentTableName,
        string childTableName,
        bool useCache
    )
    {
        return new System.Collections.Generic.List<T>();
    }

    public List<T> RetrieveListByGroup<T>(Key primaryKey)
    {
        return new System.Collections.Generic.List<T>();
    }

    public void Persist(IPersistent obj) { }

    public void FlushCache() { }

    public void DeletePackage(Guid packageId) { }

    public bool IsInTransaction { get; }

    public void RunInTransaction(Action action)
    {
        action();
    }

    public void BeginTransaction() { }

    public void EndTransaction() { }

    public void EndTransactionDontSave()
    {
        throw new NotImplementedException();
    }

    public object RetrieveValue(Guid instanceId, Type parentType, string fieldName)
    {
        return null;
    }

    public void RestrictToLoadedPackage(bool b) { }

    public ILocalizationCache LocalizationCache { get; }

    public List<string> Files(IPersistent persistentObject)
    {
        return new List<string>();
    }

    public List<T> GetReference<T>(Key key)
    {
        return new List<T>();
    }

    public bool IsOfType<T>(Guid id)
    {
        return false;
    }
}
