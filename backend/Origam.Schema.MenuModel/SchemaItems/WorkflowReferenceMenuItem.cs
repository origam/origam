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
using System.Xml.Serialization;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.Services;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using System.Collections.Generic;

namespace Origam.Schema.MenuModel
{
	/// <summary>
	/// Summary description for WorkflowReferenceMenuItem.
	/// </summary>
	[SchemaItemDescription("Sequential Workflow Reference", "menu_workflow.png")]
    [HelpTopic("Sequential+Workflow+Menu+Item")]
    [ClassMetaVersion("6.0.0")]
	public class WorkflowReferenceMenuItem : AbstractMenuItem
	{
		private ISchemaService _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

		public WorkflowReferenceMenuItem() : base() {}

		public WorkflowReferenceMenuItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public WorkflowReferenceMenuItem(Key primaryKey) : base(primaryKey)	{}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Workflow);

			base.GetExtraDependencies (dependencies);
		}

		public override Origam.UI.BrowserNodeCollection ChildNodes()
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
			get
			{
				return true;
			}
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
			get
			{
				return (IWorkflow)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.WorkflowId));
			}
			set
			{
				this.WorkflowId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		#endregion

		#region Private Methods
		private DataSet LoadData(DataStructureQuery query)
		{
			IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);

			dataServiceAgent.MethodName = "LoadDataByQuery";
			dataServiceAgent.Parameters.Clear();
			dataServiceAgent.Parameters.Add("Query", query);

			dataServiceAgent.Run();

			return dataServiceAgent.Result as DataSet;
		}

		private void SaveData(DataStructureQuery query, DataSet data)
		{
			IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);

			dataServiceAgent.MethodName = "StoreDataByQuery";
			dataServiceAgent.Parameters.Clear();
			dataServiceAgent.Parameters.Add("Query", query);
			dataServiceAgent.Parameters.Add("Data", data);

			dataServiceAgent.Run();
		}
		#endregion

		#region ISchemaItemFactory Members

		[Browsable(false)]
		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {
									  typeof(DataConstantReference),
									  typeof(SystemFunctionCall),
									  typeof(ReportReference)
								  };
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(DataConstantReference))
			{
				item = new DataConstantReference(schemaExtensionId);
				item.Name = "NewDataConstantReference";
			}
			else if(type == typeof(ReportReference))
			{
				item = new ReportReference(schemaExtensionId);
				item.Name = "NewReportReference";
			}
			else if(type == typeof(SystemFunctionCall))
			{
				item = new SystemFunctionCall(schemaExtensionId);
				item.Name = "NewSystemFunctionCall";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorWorkflowUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}

		public override IList<string> NewTypeNames
		{
			get
			{
				try
				{
					IBusinessServicesService agents = ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService;
					IServiceAgent agent = agents.GetAgent("WorkflowService", null, null);
					return agent.ExpectedParameterNames(this.Workflow as AbstractSchemaItem, "ExecuteWorkflow", "Parameters");
				}
				catch
				{
					return new string[] {};
				}
			}
		}
		#endregion

	}
}
