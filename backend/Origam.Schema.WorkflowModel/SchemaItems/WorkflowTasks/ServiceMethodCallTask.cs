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
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for ServiceCallTask.
	/// </summary>
	[SchemaItemDescription("(Task) Service Method Call", "Tasks", "task-service-method-call.png")]
    [HelpTopic("Service+Method+Call+Task")]
    [ClassMetaVersion("6.0.0")]
	public class ServiceMethodCallTask : WorkflowTask, ISchemaItemFactory
	{
		public ServiceMethodCallTask() : base() {}

		public ServiceMethodCallTask(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public ServiceMethodCallTask(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem members
		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Service);
			dependencies.Add(this.ServiceMethod);

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
				ModelElementKey key = new ModelElementKey();
				key.Id = this.ServiceMethodId;

				return (ServiceMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				// We delete any current parameters
				foreach(ISchemaItem child in this.ChildItems)
				{
					if(child is ServiceMethodCallParameter)
					{
						child.IsDeleted = true;
					}
				}

				if(value == null)
				{
					this.ServiceMethodId = Guid.Empty;
				}
				else
				{
					this.ServiceMethodId = (Guid)value.PrimaryKey["Id"];

					// We generate all parameters to the function
					foreach(ServiceMethodParameter parameter in this.ServiceMethod.ChildItems)
					{
						ServiceMethodCallParameter parameterRef = this.NewItem(typeof(ServiceMethodCallParameter), this.SchemaExtensionId, null) as ServiceMethodCallParameter;
						parameterRef.ServiceMethodParameter = parameter;
						parameterRef.Name = parameter.Name;
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
				ModelElementKey key = new ModelElementKey();
				key.Id = this.ServiceId;

				return (IService)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.ServiceId = Guid.Empty;
				}
				else
				{
					this.ServiceId = (Guid)value.PrimaryKey["Id"];
				}

				// Reset Method
				this.ServiceMethod = null;
			}
		}
		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {
									  typeof(WorkflowTaskDependency),
									  typeof(ServiceMethodCallParameter)
								  };
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(WorkflowTaskDependency))
			{
				item = new WorkflowTaskDependency(schemaExtensionId);
				item.Name = "NewWorkflowTaskDependency";
			}
			else if(type == typeof(ServiceMethodCallParameter))
			{
				item = new ServiceMethodCallParameter(schemaExtensionId);
				item.Name = "NewServiceMethodCallParameter";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorWorkflowUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}

		#endregion
	}
}
