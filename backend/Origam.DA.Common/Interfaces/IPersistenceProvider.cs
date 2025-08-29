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
#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

namespace Origam.DA.ObjectPersistence;

/// <summary>
/// Summary description for IPersistenceProvider.
/// </summary>
public interface IPersistenceProvider : ICloneable, IDisposable
{
    event EventHandler<IPersistent> InstancePersisted;
    void OnTransactionEnded(object sender);
    ICompiledModel CompiledModel { get; set; }

    /// <summary>
    /// Insert or update the current object instance.
    /// </summary>
    object RetrieveInstance(Type type, Key primaryKey);

    /// <summary>
    /// Insert or update the current object instance.
    /// </summary>
    object RetrieveInstance(Type type, Key primaryKey, bool useCache);
    object RetrieveInstance(Type type, Key primaryKey, bool useCache, bool throwNotFoundException);
    T RetrieveInstance<T>(Guid instanceId);
    T RetrieveInstance<T>(Guid instanceId, bool useCache);
    T RetrieveInstance<T>(Guid instanceId, bool useCache, bool throwNotFoundException);

    /// <summary>
    /// Refreshes the current object with data from the dataset.
    /// </summary>
    /// <param name="persistentObject"></param>
    void RefreshInstance(IPersistent persistentObject);

    /// <summary>
    /// Removes the item from the internal cache.
    /// </summary>
    /// <param name="instance"></param>
    void RemoveFromCache(IPersistent instance);

    /// <summary>
    /// Retrieves a list of objects by specified filter. It does not load data from database,
    /// just from the initialized data, loaded by Init() before.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    List<T> RetrieveList<T>(IDictionary<string, object> filter = null);
    List<T> RetrieveListByCategory<T>(string category);
    List<T> RetrieveListByPackage<T>(Guid packageId);
    T[] FullTextSearch<T>(string text);
    List<T> RetrieveListByParent<T>(
        Key primaryKey,
        string parentTableName,
        string childTableName,
        bool useCache
    );
    List<T> RetrieveListByGroup<T>(Key primaryKey);

    /// <summary>
    /// Persist (inserts or updates) an object.
    /// </summary>
    /// <param name="obj">The object to persist</param>
    void Persist(IPersistent obj);
    void FlushCache();
    void DeletePackage(Guid packageId);
    bool IsInTransaction { get; }
    void RunInTransaction(Action action);
    void BeginTransaction();
    void EndTransaction();
    void EndTransactionDontSave();
    object RetrieveValue(Guid instanceId, Type parentType, string fieldName);
    void RestrictToLoadedPackage(bool b);

    ILocalizationCache LocalizationCache { get; }
    List<string> Files(IPersistent item);
    List<T> GetReference<T>(Key key);

    bool IsOfType<T>(Guid id);
}
