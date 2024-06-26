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
using System.ComponentModel;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel;
[SchemaItemDescription("(Task) Service Method Call", "Tasks", "task-service-method-call.png")]
[HelpTopic("Service+Method+Call+Task")]
[ClassMetaVersion("6.0.0")]
public class ServiceMethodCallTask : WorkflowTask
{
	public ServiceMethodCallTask() {}
	public ServiceMethodCallTask(Guid schemaExtensionId) 
		: base(schemaExtensionId) {}
	public ServiceMethodCallTask(Key primaryKey) : base(primaryKey) {}
	#region Overriden AbstractSchemaItem members
	public override void GetExtraDependencies(
		System.Collections.ArrayList dependencies)
	{
		dependencies.Add(Service);
		dependencies.Add(ServiceMethod);
		base.GetExtraDependencies (dependencies);
	}
	#endregion
	#region Properties
	public Guid ServiceMethodId;	
	[TypeConverter(typeof(ServiceMethodConverter))]
    [NotNullModelElementRule()]
	[XmlReference("serviceMethod", "ServiceMethodId")]
	public IServiceMethod ServiceMethod
	{
		get
		{
			var key = new ModelElementKey
			{
				Id = this.ServiceMethodId
			};
			return (ServiceMethod)PersistenceProvider.RetrieveInstance(
				typeof(AbstractSchemaItem), key);
		}
		set
		{
			// We delete any current parameters
			foreach(ISchemaItem child in ChildItems)
			{
				if(child is ServiceMethodCallParameter)
				{
					child.IsDeleted = true;
				}
			}
			if(value == null)
			{
				ServiceMethodId = Guid.Empty;
			}
			else
			{
				ServiceMethodId = (Guid)value.PrimaryKey["Id"];
				// We generate all parameters to the function
				foreach(ServiceMethodParameter parameter 
				        in ServiceMethod.ChildItems)
				{
					var serviceMethodCallParameter 
						= NewItem<ServiceMethodCallParameter>(
							SchemaExtensionId, null);
					serviceMethodCallParameter.ServiceMethodParameter 
						= parameter;
					serviceMethodCallParameter.Name = parameter.Name;
				}
			}
		}
	}
	
	public Guid ServiceId;
	[TypeConverter(typeof(ServiceConverter))]
    [NotNullModelElementRule()]
	[XmlReference("service", "ServiceId")]
	public IService Service
	{
		get
		{
			var key = new ModelElementKey
			{
				Id = ServiceId
			};
			return (IService)PersistenceProvider.RetrieveInstance(
				typeof(AbstractSchemaItem), key);
		}
		set
		{
			if(value == null)
			{
				ServiceId = Guid.Empty;
			}
			else
			{
				ServiceId = (Guid)value.PrimaryKey["Id"];
			}
			// Reset Method
			ServiceMethod = null;
		}
	}
	#endregion
	#region ISchemaItemFactory Members
	public override Type[] NewItemTypes => new[] 
	{
		typeof(WorkflowTaskDependency),
		typeof(ServiceMethodCallParameter)
	};
	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
		string itemName = null;
		if(typeof(T) == typeof(WorkflowTaskDependency))
		{
			itemName = "NewWorkflowTaskDependency";
		}
		else if(typeof(T) == typeof(ServiceMethodCallParameter))
		{
			itemName = "NewServiceMethodCallParameter";
		}
		return base.NewItem<T>(schemaExtensionId, group, itemName);
	}
	#endregion
}
