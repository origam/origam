using System;

namespace Origam.DA.ObjectPersistence
{

    public interface IPropertyContainer
    {
        object GetValue();
    }
    /// <summary>
    /// This class holds the information whether or not the property it represents was set to
    /// null by the user. And combines it with lazy loading
    /// If it was set to null by the user the Get method will return null, if not and the
    /// actual value is retrieved.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyContainer<T>: IPropertyContainer
    {
        private T value;
        private bool wasSetToNull;
        private readonly Guid id;
        private readonly string containerName;
        private readonly Func<IPersistenceProvider> persistenceProviderGetter;
        private readonly Type containingObjectType;

        public PropertyContainer(string containerName, IFilePersistent containingObject)
        {
            this.containerName = containerName;
            id = (Guid)containingObject.PrimaryKey["Id"];
            persistenceProviderGetter = ()=> containingObject.PersistenceProvider;
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
                    .RetrieveValue(id, containingObjectType, containerName);
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