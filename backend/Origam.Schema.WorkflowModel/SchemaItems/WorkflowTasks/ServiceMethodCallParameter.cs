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
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;

using Origam.Workbench.Services;

namespace Origam.Schema.WorkflowModel;
[SchemaItemDescription("Parameter", "Parameters", "parameter-mapping-blm.png")]
[HelpTopic("Service+Method+Call+Parameter")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class ServiceMethodCallParameter : AbstractSchemaItem
{
	public const string CategoryConst = "ServiceMethodCallParameter";
	public ServiceMethodCallParameter() {}
	public ServiceMethodCallParameter(Guid schemaExtensionId) 
		: base(schemaExtensionId) {}
	public ServiceMethodCallParameter(Key primaryKey) : base(primaryKey) {}
	#region Overriden AbstractDataEntityColumn Members
	
	public override string ItemType => CategoryConst;
	[Browsable(false)]
	public override bool UseFolders => false;
	public override void GetExtraDependencies(ArrayList dependencies)
	{
		dependencies.Add(ServiceMethodParameter);
		base.GetExtraDependencies(dependencies);
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
			var key = new ModelElementKey
			{
				Id = ServiceMethodParameterId
			};
			return (ServiceMethodParameter)PersistenceProvider
				.RetrieveInstance(typeof(ServiceMethodParameter), key);
		}
		set => ServiceMethodParameterId = (Guid)value.PrimaryKey["Id"];
	}
	#endregion
	#region ISchemaItemFactory Members
	[Browsable(false)]
	public override Type[] NewItemTypes
	{
		get
		{
			var result = new ArrayList();
			if(ServiceMethodParameter.AllowContextReference)
			{
				result.Add(typeof(ContextReference));
			}
			if(ServiceMethodParameter.AllowDataConstantReference)
			{
				result.Add(typeof(DataConstantReference));
			}
			if(ServiceMethodParameter.AllowSystemFunctionCall)
			{
				result.Add(typeof(SystemFunctionCall));
			}
			if(ServiceMethodParameter.AllowDataStructureReference)
			{
				result.Add(typeof(DataStructureReference));
			}
			if(ServiceMethodParameter.AllowTransformationReference)
			{
				result.Add(typeof(TransformationReference));
			}
			if(ServiceMethodParameter.AllowReportReference)
			{
				result.Add(typeof(ReportReference));
			}
			if(ServiceMethodParameter.AllowWorkflowReference)
			{
				result.Add(typeof(WorkflowReference));
			}
			return (Type[])result.ToArray(typeof(Type));
		}
	}
	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
		string itemName = null;
		if(typeof(T) == typeof(ContextReference))
		{
			itemName = "NewContextReference";
		}
		else if(typeof(T) == typeof(DataConstantReference))
		{
			itemName = "NewDataConstantReference";
		}
		else if(typeof(T) == typeof(DataStructureReference))
		{
			itemName = "NewDataStructureReference";
		}
		else if(typeof(T) == typeof(TransformationReference))
		{
			itemName = "NewTransformationReference";
		}
		else if(typeof(T) == typeof(ReportReference))
		{
			itemName = "NewReportReference";
		}
		else if(typeof(T) == typeof(WorkflowReference))
		{
			itemName = "NewWorkflowReference";
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
				var serviceMethodCallTask = ParentItem as ServiceMethodCallTask;
				var businessServicesService = ServiceManager.Services
					.GetService<IBusinessServicesService>();
				var serviceAgent = businessServicesService.GetAgent(
					serviceMethodCallTask.Service.Name, null, null);
				return serviceAgent.ExpectedParameterNames(
					serviceMethodCallTask, 
					serviceMethodCallTask.ServiceMethod.Name, 
					ServiceMethodParameter.Name);
			}
			catch
			{
				return new string[] {};
			}
		}
	}
	#endregion
}
