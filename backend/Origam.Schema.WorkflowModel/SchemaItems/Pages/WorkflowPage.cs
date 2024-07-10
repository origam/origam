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

using Origam.DA.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Schema.WorkflowModel;
[SchemaItemDescription("Workflow Page", "workflow-page.png")]
[HelpTopic("Workflow+Page")]
[ClassMetaVersion("6.0.0")]
public class WorkflowPage : AbstractPage
{
	public WorkflowPage() : base() {Init();}
	public WorkflowPage(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public WorkflowPage(Key primaryKey) : base(primaryKey) {Init();}
	private void Init()
	{
		this.ChildItemTypes.Add(typeof(PageParameterMapping));
		this.ChildItemTypes.Add(typeof(PageParameterFileMapping));
		this.ChildItemTypes.Add(typeof(RedirectWorkflowPageAction));
	}
	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		dependencies.Add(this.Workflow);
		base.GetExtraDependencies (dependencies);
	}
	public override IList<string> NewTypeNames
	{
		get
		{
			try
			{
				IBusinessServicesService agents = ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService;
				IServiceAgent agent = agents.GetAgent("WorkflowService", null, null);
				return agent.ExpectedParameterNames(this.Workflow, "ExecuteWorkflow", "Parameters");
			}
			catch
			{
				return new List<string>();
			}
		}
	}
	public override Type[] NameableTypes =>
		new Type[] {typeof(PageParameterMapping), typeof(PageParameterFileMapping)};
	#region Properties
	public Guid WorkflowId;
	[Category("Workflow")]
	[TypeConverter(typeof(WorkflowConverter))]
    [NotNullModelElementRule()]
	[XmlReference("workflow", "WorkflowId")]
	public Workflow Workflow
	{
		get => (Workflow)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.WorkflowId));
		set
		{
			this.WorkflowId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
		}
	}
	[Category("InputValidation")]
	[XmlAttribute ("disableConstraintForInputValidation")]
	public bool DisableConstraintForInputValidation { get; set; }
	[Browsable(false)]
    public new DataConstant CacheMaxAge
    {
        get
        {
            return null;
        }
        set
        {
        }
    }
    #endregion
}
