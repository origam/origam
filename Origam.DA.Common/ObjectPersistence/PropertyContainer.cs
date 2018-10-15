using System;

namespace Origam.DA.ObjectPersistence
{
    public class PropertyContainer<T>
    {
        private T value;
        private bool wasSetTuNull;
        private readonly Func<Guid> idGetter;
        private readonly string containerName;
        private readonly Func<IPersistenceProvider> persistenceProviderGetter;
        private readonly Type containingObjectType;

        public PropertyContainer(string containerName, IFilePersistent containingObject)
        {
            this.containerName = containerName;
            this.idGetter = () => (Guid)containingObject.PrimaryKey["Id"];
            this.persistenceProviderGetter = ()=> containingObject.PersistenceProvider;
            containingObjectType = containingObject.GetType();
        }

        public T Get()
        {
            if (value == null && !wasSetTuNull)
            {
                value = (T) persistenceProviderGetter()
                    .RetrieveValue(idGetter(), containingObjectType, containerName);
            }
            return value;
        }

        public void Set(T value)
        {
            if (value == null)
            {
                wasSetTuNull = true;
            }
            this.value = value;
        }
    }
}