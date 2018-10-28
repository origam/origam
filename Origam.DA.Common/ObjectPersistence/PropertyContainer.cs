using System;

namespace Origam.DA.ObjectPersistence
{

    public interface IPropertyContainer
    {
        object GetValue();
    }

    public class PropertyContainer<T>: IPropertyContainer
    {
        private T value;
        private bool wasSetToNull;
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

        public object GetValue()
        {
            return Get();
        }

        public T Get()
        {
            if (value == null && !wasSetToNull)
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
                wasSetToNull = true;
            }
            this.value = value;
        }
    }
}