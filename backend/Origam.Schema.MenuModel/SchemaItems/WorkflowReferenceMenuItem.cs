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
using System.Data;
using System.ComponentModel;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.Services;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using System.Collections.Generic;

namespace Origam.Schema.MenuModel;
[SchemaItemDescription("Sequential Workflow Reference", "menu_workflow.png")]
[HelpTopic("Sequential+Workflow+Menu+Item")]
[ClassMetaVersion("6.0.0")]
public class WorkflowReferenceMenuItem : AbstractMenuItem
{
	private ISchemaService _schemaService 
		= ServiceManager.Services.GetService<ISchemaService>();
	public WorkflowReferenceMenuItem() {}
	public WorkflowReferenceMenuItem(Guid schemaExtensionId) 
		: base(schemaExtensionId) {}
	public WorkflowReferenceMenuItem(Key primaryKey) : base(primaryKey)	{}
	public override void GetExtraDependencies(
		List<ISchemaItem> dependencies)
	{
		dependencies.Add(Workflow);
		base.GetExtraDependencies(dependencies);
	}
	public override UI.BrowserNodeCollection ChildNodes()
	{
#if ORIGAM_CLIENT
		return new Origam.UI.BrowserNodeCollection();
#else
		return base.ChildNodes ();
#endif
	}
	#region Properties
	[Browsable(false)]
	public bool IsRepeatable
	{
		get => true;
		set
		{
		}
	}
	public Guid WorkflowId;
	[TypeConverter(typeof(WorkflowConverter))]
	[NotNullModelElementRule()]
	[XmlReference("workflow", "WorkflowId")]
	public IWorkflow Workflow
	{
		get => (IWorkflow)PersistenceProvider.RetrieveInstance(
			typeof(ISchemaItem), new ModelElementKey(WorkflowId));
		set => WorkflowId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
	#endregion
	#region Private Methods
	private DataSet LoadData(DataStructureQuery query)
	{
		var dataServiceAgent = ServiceManager.Services
			.GetService<IBusinessServicesService>().GetAgent(
				"DataService", null, null);
		dataServiceAgent.MethodName = "LoadDataByQuery";
		dataServiceAgent.Parameters.Clear();
		dataServiceAgent.Parameters.Add("Query", query);
		dataServiceAgent.Run();
		return dataServiceAgent.Result as DataSet;
	}
	private void SaveData(DataStructureQuery query, DataSet data)
	{
		var dataServiceAgent = ServiceManager.Services
			.GetService<IBusinessServicesService>().GetAgent(
				"DataService", null, null);
		dataServiceAgent.MethodName = "StoreDataByQuery";
		dataServiceAgent.Parameters.Clear();
		dataServiceAgent.Parameters.Add("Query", query);
		dataServiceAgent.Parameters.Add("Data", data);
		dataServiceAgent.Run();
	}
	#endregion
	#region ISchemaItemFactory Members
	[Browsable(false)]
	public override Type[] NewItemTypes => new[] 
	{
		typeof(DataConstantReference),
		typeof(SystemFunctionCall),
		typeof(ReportReference)
	};
	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
		string itemName = null;
		if(typeof(T) == typeof(DataConstantReference))
		{
			itemName = "NewDataConstantReference";
		}
		else if(typeof(T) == typeof(ReportReference))
		{
			itemName = "NewReportReference";
		}
		else if(typeof(T) == typeof(SystemFunctionCall))
		{
			itemName = "NewSystemFunctionCall";
		}
		return base.NewItem<T>(schemaExtensionId, group, itemName);
	}
	public override IList<string> NewTypeNames
	{
		get
		{
			try
			{
				var businessServicesService = ServiceManager.Services
					.GetService<IBusinessServicesService>();
				var agent = businessServicesService.GetAgent(
					"WorkflowService", null, null);
				return agent.ExpectedParameterNames(
					Workflow, 
					"ExecuteWorkflow", 
					"Parameters");
			}
			catch
			{
				return new string[] {};
			}
		}
	}
	#endregion
}
