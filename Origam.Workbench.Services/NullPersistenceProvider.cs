using System;
using System.Collections.Generic;
using Origam.DA;
using Origam.DA.ObjectPersistence;

namespace Origam.Workbench.Services
{
    public class NullPersistenceProvider : IPersistenceProvider
    {
        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public event EventHandler InstancePersisted;
        public void OnInstancePersisted(object sender)
        {
        }

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

        public object RetrieveInstance(Type type, Key primaryKey, bool useCache, bool throwNotFoundException)
        {
            return null;
        }

        public void RefreshInstance(IPersistent persistentObject)
        {
        }

        public void RemoveFromCache(IPersistent instance)
        {
        }

        public List<T> RetrieveList<T>(IDictionary<string, object> filter = null)
        {
            return new System.Collections.Generic.List<T>();
        }

        public List<T> RetrieveListByType<T>(string itemType)
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

        public List<T> RetrieveListByParent<T>(Key primaryKey, string parentTableName, string childTableName, bool useCache)
        {
            return new System.Collections.Generic.List<T>();
        }

        public List<T> RetrieveListByGroup<T>(Key primaryKey)
        {
            return new System.Collections.Generic.List<T>();
        }

        public void Persist(IPersistent obj)
        { 
        }

        public void FlushCache()
        {
        }

        public void DeletePackage(Guid packageId)
        {
        }

        public void BeginTransaction()
        {
        }

        public void EndTransaction()
        {
        }

        public void EndTransactionDontSave()
        {
            throw new NotImplementedException();
        }

        public object RetrieveValue(Guid instanceId, Type parentType, string fieldName)
        {
            return null;
        }

        public void RestrictToLoadedPackage(bool b)
        {
        }

        public ILocalizationCache LocalizationCache { get; }
    }
}