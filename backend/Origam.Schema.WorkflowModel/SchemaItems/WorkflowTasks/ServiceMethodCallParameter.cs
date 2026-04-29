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
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Schema.WorkflowModel;

[SchemaItemDescription(
    name: "Parameter",
    folderName: "Parameters",
    iconName: "parameter-mapping-blm.png"
)]
[HelpTopic(topic: "Service+Method+Call+Parameter")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class ServiceMethodCallParameter : AbstractSchemaItem
{
    public const string CategoryConst = "ServiceMethodCallParameter";

    public ServiceMethodCallParameter() { }

    public ServiceMethodCallParameter(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public ServiceMethodCallParameter(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType => CategoryConst;

    [Browsable(browsable: false)]
    public override bool UseFolders => false;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: ServiceMethodParameter);
        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
    #region Properties
    public Guid ServiceMethodParameterId;

    [XmlReference(attributeName: "serviceMethodParameter", idField: "ServiceMethodParameterId")]
    [ReadOnly(isReadOnly: true)]
    public ServiceMethodParameter ServiceMethodParameter
    {
        get
        {
            var key = new ModelElementKey { Id = ServiceMethodParameterId };
            return (ServiceMethodParameter)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ServiceMethodParameter),
                    primaryKey: key
                );
        }
        set => ServiceMethodParameterId = (Guid)value.PrimaryKey[key: "Id"];
    }
    #endregion
    #region ISchemaItemFactory Members
    [Browsable(browsable: false)]
    public override Type[] NewItemTypes
    {
        get
        {
            var result = new List<Type>();
            if (ServiceMethodParameter.AllowContextReference)
            {
                result.Add(item: typeof(ContextReference));
            }
            if (ServiceMethodParameter.AllowDataConstantReference)
            {
                result.Add(item: typeof(DataConstantReference));
            }
            if (ServiceMethodParameter.AllowSystemFunctionCall)
            {
                result.Add(item: typeof(SystemFunctionCall));
            }
            if (ServiceMethodParameter.AllowDataStructureReference)
            {
                result.Add(item: typeof(DataStructureReference));
            }
            if (ServiceMethodParameter.AllowTransformationReference)
            {
                result.Add(item: typeof(TransformationReference));
            }
            if (ServiceMethodParameter.AllowReportReference)
            {
                result.Add(item: typeof(ReportReference));
            }
            if (ServiceMethodParameter.AllowWorkflowReference)
            {
                result.Add(item: typeof(WorkflowReference));
            }
            return result.ToArray();
        }
    }

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(ContextReference))
        {
            itemName = "NewContextReference";
        }
        else if (typeof(T) == typeof(DataConstantReference))
        {
            itemName = "NewDataConstantReference";
        }
        else if (typeof(T) == typeof(DataStructureReference))
        {
            itemName = "NewDataStructureReference";
        }
        else if (typeof(T) == typeof(TransformationReference))
        {
            itemName = "NewTransformationReference";
        }
        else if (typeof(T) == typeof(ReportReference))
        {
            itemName = "NewReportReference";
        }
        else if (typeof(T) == typeof(WorkflowReference))
        {
            itemName = "NewWorkflowReference";
        }
        else if (typeof(T) == typeof(SystemFunctionCall))
        {
            itemName = "NewSystemFunctionCall";
        }
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: itemName
        );
    }

    public override IList<string> NewTypeNames
    {
        get
        {
            try
            {
                var serviceMethodCallTask = ParentItem as ServiceMethodCallTask;
                var businessServicesService =
                    ServiceManager.Services.GetService<IBusinessServicesService>();
                var serviceAgent = businessServicesService.GetAgent(
                    serviceType: serviceMethodCallTask.Service.Name,
                    ruleEngine: null,
                    workflowEngine: null
                );
                return serviceAgent.ExpectedParameterNames(
                    item: serviceMethodCallTask,
                    method: serviceMethodCallTask.ServiceMethod.Name,
                    parameter: ServiceMethodParameter.Name
                );
            }
            catch
            {
                return new string[] { };
            }
        }
    }
    #endregion
}
