
using System;
using System.Collections;
using System.Collections.Generic;

namespace Origam.DA.ObjectPersistence
{
    public abstract class AbstractPersistenceProvider : IPersistenceProvider
    {
        private readonly Queue<object> transactionEndEventQueue = new Queue<object>();
        public virtual ICompiledModel CompiledModel
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public event EventHandler InstancePersisted;

        public virtual void BeginTransaction()
        {
        }

        public abstract object Clone();

        public abstract void DeletePackage(Guid packageId);

        public abstract void Dispose();

        public virtual void EndTransaction()
        {
            while (transactionEndEventQueue.Count > 0)
            {
                object sender = transactionEndEventQueue.Dequeue();
                InstancePersisted?.Invoke(sender, EventArgs.Empty);
            }
        }

        public virtual void EndTransactionDontSave()
        {
            
        }

        public abstract object RetrieveValue(Guid instanceId, Type parentType, string fieldName);
        public virtual void RestrictToLoadedPackage(bool b)
        {
        }

        public abstract ILocalizationCache LocalizationCache { get;}

        public abstract void FlushCache();

        public abstract void RemoveFromCache(IPersistent instance);
        public abstract List<T> RetrieveList<T>(IDictionary<string, object> filter=null);

        public abstract List<T> RetrieveListByType<T>(string itemType);

        public abstract List<T> RetrieveListByPackage<T>(Guid packageId);

        public abstract List<T> FullTextSearch<T>(string text);

        public abstract List<T> RetrieveListByParent<T>(Key primaryKey,
            string parentTableName,
            string childTableName, bool useCache);

        public abstract List<T> RetrieveListByGroup<T>(Key primaryKey);

        public void OnTransactionEnded(object sender)
        {
            transactionEndEventQueue.Enqueue(sender);
        }

        public abstract void Persist(IPersistent obj);

        public abstract void RefreshInstance(IPersistent persistentObject);

        public abstract object RetrieveInstance(Type type, Key primaryKey);

        public abstract object RetrieveInstance(Type type, Key primaryKey, bool useCache);

        public abstract object RetrieveInstance(Type type, Key primaryKey, bool useCache, bool throwNotFoundException);
    }
}
