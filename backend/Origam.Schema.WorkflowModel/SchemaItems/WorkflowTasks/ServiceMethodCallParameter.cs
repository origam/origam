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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;

using Origam.Workbench.Services;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for ServiceMethodCallParameter.
	/// </summary>
	[SchemaItemDescription("Parameter", "Parameters", "parameter-mapping-blm.png")]
    [HelpTopic("Service+Method+Call+Parameter")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class ServiceMethodCallParameter : AbstractSchemaItem, ISchemaItemFactory
	{
		public const string CategoryConst = "ServiceMethodCallParameter";

		public ServiceMethodCallParameter() : base() {}

		public ServiceMethodCallParameter(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public ServiceMethodCallParameter(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractDataEntityColumn Members
		
		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}

		[Browsable(false)]
		public override bool UseFolders
		{
			get
			{
				return false;
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.ServiceMethodParameter);

			base.GetExtraDependencies (dependencies);
		}

		#endregion

		#region Properties
		public Guid ServiceMethodParameterId;

		[XmlReference("serviceMethodParameter", "ServiceMethodParameterId")]
        [ReadOnly(true)]
		public ServiceMethodParameter ServiceMethodParameter
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.ServiceMethodParameterId;

				return (ServiceMethodParameter)this.PersistenceProvider.RetrieveInstance(typeof(ServiceMethodParameter), key);
			}
			set
			{
				this.ServiceMethodParameterId = (Guid)value.PrimaryKey["Id"];
			}
		}
		#endregion

		#region ISchemaItemFactory Members

		[Browsable(false)]
		public override Type[] NewItemTypes
		{
			get
			{
				ArrayList result = new ArrayList();
				if(this.ServiceMethodParameter.AllowContextReference)
				{
					result.Add(typeof(ContextReference));
				}
				if(this.ServiceMethodParameter.AllowDataConstantReference)
				{
					result.Add(typeof(DataConstantReference));
				}
				if(this.ServiceMethodParameter.AllowSystemFunctionCall)
				{
					result.Add(typeof(SystemFunctionCall));
				}
				if(this.ServiceMethodParameter.AllowDataStructureReference)
				{
					result.Add(typeof(DataStructureReference));
				}
				if(this.ServiceMethodParameter.AllowTransformationReference)
				{
					result.Add(typeof(TransformationReference));
				}
				if(this.ServiceMethodParameter.AllowReportReference)
				{
					result.Add(typeof(ReportReference));
				}
				if(this.ServiceMethodParameter.AllowWorkflowReference)
				{
					result.Add(typeof(WorkflowReference));
				}

				return (Type[]) result.ToArray(typeof(Type));
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(ContextReference))
			{
				item = new ContextReference(schemaExtensionId);
				item.Name = "NewContextReference";
			}
			else if(type == typeof(DataConstantReference))
			{
				item = new DataConstantReference(schemaExtensionId);
				item.Name = "NewDataConstantReference";
			}
			else if(type == typeof(DataStructureReference))
			{
				item = new DataStructureReference(schemaExtensionId);
				item.Name = "NewDataStructureReference";
			}
			else if(type == typeof(TransformationReference))
			{
				item = new TransformationReference(schemaExtensionId);
				item.Name = "NewTransformationReference";
			}
			else if(type == typeof(ReportReference))
			{
				item = new ReportReference(schemaExtensionId);
				item.Name = "NewReportReference";
			}
			else if(type == typeof(WorkflowReference))
			{
				item = new WorkflowReference(schemaExtensionId);
				item.Name = "NewWorkflowReference";
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
					ServiceMethodCallTask call = this.ParentItem as ServiceMethodCallTask;
					IBusinessServicesService agents = ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService;
					IServiceAgent agent = agents.GetAgent(call.Service.Name, null, null);
					return agent.ExpectedParameterNames(call, call.ServiceMethod.Name, ServiceMethodParameter.Name);
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
