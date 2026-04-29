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
using System.Data;
using Origam.DA;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.MenuModel;

[SchemaItemDescription(name: "Sequential Workflow Reference", iconName: "menu_workflow.png")]
[HelpTopic(topic: "Sequential+Workflow+Menu+Item")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class WorkflowReferenceMenuItem : AbstractMenuItem
{
    private ISchemaService _schemaService = ServiceManager.Services.GetService<ISchemaService>();

    public WorkflowReferenceMenuItem() { }

    public WorkflowReferenceMenuItem(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public WorkflowReferenceMenuItem(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: Workflow);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override UI.BrowserNodeCollection ChildNodes()
    {
#if ORIGAM_CLIENT
        return new Origam.UI.BrowserNodeCollection();
#else
        return base.ChildNodes();
#endif
    }

    #region Properties
    [Browsable(browsable: false)]
    public bool IsRepeatable
    {
        get => true;
        set { }
    }
    public Guid WorkflowId;

    [TypeConverter(type: typeof(WorkflowConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "workflow", idField: "WorkflowId")]
    public IWorkflow Workflow
    {
        get =>
            (IWorkflow)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: WorkflowId)
                );
        set => WorkflowId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    #endregion
    #region Private Methods
    private DataSet LoadData(DataStructureQuery query)
    {
        var dataServiceAgent = ServiceManager
            .Services.GetService<IBusinessServicesService>()
            .GetAgent(serviceType: "DataService", ruleEngine: null, workflowEngine: null);
        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query);
        dataServiceAgent.Run();
        return dataServiceAgent.Result as DataSet;
    }

    private void SaveData(DataStructureQuery query, DataSet data)
    {
        var dataServiceAgent = ServiceManager
            .Services.GetService<IBusinessServicesService>()
            .GetAgent(serviceType: "DataService", ruleEngine: null, workflowEngine: null);
        dataServiceAgent.MethodName = "StoreDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query);
        dataServiceAgent.Parameters.Add(key: "Data", value: data);
        dataServiceAgent.Run();
    }
    #endregion
    #region ISchemaItemFactory Members
    [Browsable(browsable: false)]
    public override Type[] NewItemTypes =>
        new[]
        {
            typeof(DataConstantReference),
            typeof(SystemFunctionCall),
            typeof(ReportReference),
        };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(DataConstantReference))
        {
            itemName = "NewDataConstantReference";
        }
        else if (typeof(T) == typeof(ReportReference))
        {
            itemName = "NewReportReference";
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
                var businessServicesService =
                    ServiceManager.Services.GetService<IBusinessServicesService>();
                var agent = businessServicesService.GetAgent(
                    serviceType: "WorkflowService",
                    ruleEngine: null,
                    workflowEngine: null
                );
                return agent.ExpectedParameterNames(
                    item: Workflow,
                    method: "ExecuteWorkflow",
                    parameter: "Parameters"
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
