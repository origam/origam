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

namespace Origam.DA.ObjectPersistence;

public interface IPropertyContainer
{
    object GetValue();
}
/// <summary>
/// This class holds the information whether or not the property it represents was set to
/// null by the user and combines it with lazy loading.
/// If it was set to null by the user the Get method will return null, if not the
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