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
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Schema.WorkflowModel;

[SchemaItemDescription(name: "Workflow Page", iconName: "workflow-page.png")]
[HelpTopic(topic: "Workflow+Page")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class WorkflowPage : AbstractPage
{
    public WorkflowPage()
        : base()
    {
        Init();
    }

    public WorkflowPage(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId)
    {
        Init();
    }

    public WorkflowPage(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        this.ChildItemTypes.Add(item: typeof(PageParameterMapping));
        this.ChildItemTypes.Add(item: typeof(PageParameterFileMapping));
        this.ChildItemTypes.Add(item: typeof(RedirectWorkflowPageAction));
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Workflow);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override IList<string> NewTypeNames
    {
        get
        {
            try
            {
                IBusinessServicesService agents =
                    ServiceManager.Services.GetService(
                        serviceType: typeof(IBusinessServicesService)
                    ) as IBusinessServicesService;
                IServiceAgent agent = agents.GetAgent(
                    serviceType: "WorkflowService",
                    ruleEngine: null,
                    workflowEngine: null
                );
                return agent.ExpectedParameterNames(
                    item: this.Workflow,
                    method: "ExecuteWorkflow",
                    parameter: "Parameters"
                );
            }
            catch
            {
                return new List<string>();
            }
        }
    }
    public override Type[] NameableTypes =>
        new Type[] { typeof(PageParameterMapping), typeof(PageParameterFileMapping) };
    #region Properties
    public Guid WorkflowId;

    [Category(category: "Workflow")]
    [TypeConverter(type: typeof(WorkflowConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "workflow", idField: "WorkflowId")]
    public Workflow Workflow
    {
        get =>
            (Workflow)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.WorkflowId)
                );
        set { this.WorkflowId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]; }
    }

    [Category(category: "InputValidation")]
    [XmlAttribute(attributeName: "disableConstraintForInputValidation")]
    public bool DisableConstraintForInputValidation { get; set; }

    [Browsable(browsable: false)]
    public new DataConstant CacheMaxAge
    {
        get { return null; }
        set { }
    }
    #endregion
}
