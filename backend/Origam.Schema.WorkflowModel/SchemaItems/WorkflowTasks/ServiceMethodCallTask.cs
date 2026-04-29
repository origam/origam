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
using System.ComponentModel;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel;

[SchemaItemDescription(
    name: "(Task) Service Method Call",
    folderName: "Tasks",
    iconName: "task-service-method-call.png"
)]
[HelpTopic(topic: "Service+Method+Call+Task")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class ServiceMethodCallTask : WorkflowTask
{
    public ServiceMethodCallTask() { }

    public ServiceMethodCallTask(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public ServiceMethodCallTask(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem members
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: Service);
        dependencies.Add(item: ServiceMethod);
        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
    #region Properties
    public Guid ServiceMethodId;

    [TypeConverter(type: typeof(ServiceMethodConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "serviceMethod", idField: "ServiceMethodId")]
    public IServiceMethod ServiceMethod
    {
        get
        {
            var key = new ModelElementKey { Id = this.ServiceMethodId };
            return (ServiceMethod)
                PersistenceProvider.RetrieveInstance(type: typeof(ISchemaItem), primaryKey: key);
        }
        set
        {
            // We delete any current parameters
            foreach (ISchemaItem child in ChildItems)
            {
                if (child is ServiceMethodCallParameter)
                {
                    child.IsDeleted = true;
                }
            }
            if (value == null)
            {
                ServiceMethodId = Guid.Empty;
            }
            else
            {
                ServiceMethodId = (Guid)value.PrimaryKey[key: "Id"];
                // We generate all parameters to the function
                foreach (ServiceMethodParameter parameter in ServiceMethod.ChildItems)
                {
                    var serviceMethodCallParameter = NewItem<ServiceMethodCallParameter>(
                        schemaExtensionId: SchemaExtensionId,
                        group: null
                    );
                    serviceMethodCallParameter.ServiceMethodParameter = parameter;
                    serviceMethodCallParameter.Name = parameter.Name;
                }
            }
        }
    }

    public Guid ServiceId;

    [TypeConverter(type: typeof(ServiceConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "service", idField: "ServiceId")]
    public IService Service
    {
        get
        {
            var key = new ModelElementKey { Id = ServiceId };
            return (IService)
                PersistenceProvider.RetrieveInstance(type: typeof(ISchemaItem), primaryKey: key);
        }
        set
        {
            if (value == null)
            {
                ServiceId = Guid.Empty;
            }
            else
            {
                ServiceId = (Guid)value.PrimaryKey[key: "Id"];
            }
            // Reset Method
            ServiceMethod = null;
        }
    }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes =>
        new[] { typeof(WorkflowTaskDependency), typeof(ServiceMethodCallParameter) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(WorkflowTaskDependency))
        {
            itemName = "NewWorkflowTaskDependency";
        }
        else if (typeof(T) == typeof(ServiceMethodCallParameter))
        {
            itemName = "NewServiceMethodCallParameter";
        }
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: itemName
        );
    }
    #endregion
}
