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
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using Origam.DA;
using Origam.Service.Core;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for WorkflowStateMachine.
	/// </summary>
	[SchemaItemDescription("State Workflow", "state-workflow-2.png")]
    [HelpTopic("State+Workflows")]
    [DefaultProperty("Entity")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
    public class StateMachine : AbstractSchemaItem, ISchemaItemFactory
	{
		public const string CategoryConst = "WorkflowStateMachine";

		public StateMachine() : base() {}

		public StateMachine(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public StateMachine(Key primaryKey) : base(primaryKey)	{}

		#region Public Methods
		public StateMachineState GetState(object value)
		{
			foreach(StateMachineState state in this.AllStates())
			{
				if(state.Type != StateMachineStateType.Group && state.Value.Equals(value))
				{
					return state;
				}
			}

			return null;
		}

		public ArrayList AllStates()
		{
			ArrayList result = new ArrayList();

			foreach(ISchemaItem item in this.ChildItemsRecursive)
			{
				if(item is StateMachineState)
				{
					result.Add(item);
				}
			}

			return result;
		}
		#endregion

		#region Overriden AbstractSchemaItem Members
		
		public override string ItemType => CategoryConst;

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Entity);
			if(this.Field != null)	dependencies.Add(this.Field);
			if(this.DynamicOperationsLookup != null)	dependencies.Add(this.DynamicOperationsLookup);
			if(this.DynamicStatesLookup != null)	dependencies.Add(this.DynamicStatesLookup);

			base.GetExtraDependencies (dependencies);
		}
		#endregion

		#region Properties
		[Browsable(false)]
		public ArrayList Events => this.ChildItemsByType(StateMachineEvent.CategoryConst);

		[Browsable(false)]
		public ArrayList ParameterMappings => this.ChildItemsByType(StateMachineDynamicLookupParameterMapping.CategoryConst);

		public object[] DynamicOperations(IXmlContainer data)
		{
			DataView view = GetDynamicList(this.DynamicOperationsLookupId, data);

			object[] result = new object[view.Count];
			for(int i=0; i<view.Count; i++)
			{
				result[i] = view[i][this.DynamicOperationsLookup.ListValueMember];
			}

			return result;
		}

		public object[] InitialStateValues(IXmlContainer data)
		{
			ArrayList list = new ArrayList();

			if(this.DynamicStatesLookup == null)
			{
				// states defined in the model
				foreach(StateMachineState state in this.AllStates())
				{
					if(state.Type == StateMachineStateType.Initial)
					{
						list.Add(state.Value);
					}
				}
			}
			else
			{
				DataView view = GetDynamicList(this.DynamicStatesLookupId, data);

				// dynamic state list from the data source
				foreach(DataRowView rv in view)
				{
					if((bool)rv["IsInitial"])
					{
						list.Add(rv["Id"]);
					}
				}
			}

			return (object[])list.ToArray();
		}

		private DataView GetDynamicList(Guid lookupId, IXmlContainer data)
		{
			DataView view;

			IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
			if(this.ParameterMappings.Count == 0)
			{
				view = ls.GetList(lookupId, null);
			}
			else
			{
				if(data == null)
				{
					view = new DataView(new OrigamDataTable());
				}
				else
				{
					Hashtable parameters = new Hashtable();
					foreach(StateMachineDynamicLookupParameterMapping pm in this.ParameterMappings)
					{
						string xpath = "/row/" + (pm.Field.XmlMappingType == EntityColumnXmlMapping.Attribute ? "@" : "") + pm.Field.Name;
						XPathNavigator nav = data.Xml.CreateNavigator();
						object val = nav.Evaluate(xpath);
						if(val is XPathNodeIterator)
						{
							XPathNodeIterator iterator = val as XPathNodeIterator;

							if(iterator.Count == 0)
							{
								val = null;
							}
							else
							{
								iterator.MoveNext();
								val = iterator.Current.Value;
							}
						}
						else
						{
							val = null;
						}

						parameters.Add(pm.Name, val);
					}

					view = ls.GetList(lookupId, parameters, null);
				}
			}

			return view;
		}
		
		public Guid EntityId;
        [NotNullModelElementRule]
		[TypeConverter(typeof(EntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("entity", "EntityId")]
		public IDataEntity Entity
		{
			get => (IDataEntity)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.EntityId));
			set
			{
				this.EntityId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
				SetName();
			}
		}
		
		public Guid FieldId;

		[TypeConverter(typeof(StateMachineEntityFieldConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("field", "FieldId")]
		public IDataEntityColumn Field
		{
			get => (IDataEntityColumn)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.FieldId));
			set
			{
				this.FieldId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);

				SetName();
			}
		}
		
		public Guid DynamicStatesLookupId;

		[TypeConverter(typeof(DataLookupConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("dynamicStatesLookup", "DynamicStatesLookupId")]
		public IDataLookup DynamicStatesLookup
		{
			get => (IDataLookup)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DynamicStatesLookupId));
			set
			{
				this.DynamicStatesLookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid DynamicOperationsLookupId;

		[TypeConverter(typeof(DataLookupConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("dynamicOperationsLookup", "DynamicOperationsLookupId")]
		public IDataLookup DynamicOperationsLookup
		{
			get => (IDataLookup)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DynamicOperationsLookupId));
			set
			{
				this.DynamicOperationsLookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid ReverseLookupId;

		[TypeConverter(typeof(DataLookupConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("reverseLookup", "ReverseLookupId")]
		public IDataLookup ReverseLookup
		{
			get => (IDataLookup)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ReverseLookupId));
			set
			{
				this.ReverseLookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		#endregion

		#region Private Methods
		private void SetName()
		{
			if(this.Entity != null & this.Field != null)
			{
				this.Name = this.Entity.Name + "_" + this.Field.Name;
			}
			else if(this.Entity != null)
			{
				this.Name = this.Entity.Name;
			}
			else
			{
				this.Name = "";
			}
		}
		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes =>
			new Type[] {
				typeof(StateMachineState),
				typeof(StateMachineEvent),
				typeof(StateMachineDynamicLookupParameterMapping)
			};

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(StateMachineState))
			{
				item = new StateMachineState(schemaExtensionId);
				item.Name = "NewState";
			}
			else if(type == typeof(StateMachineEvent))
			{
				item = new StateMachineEvent(schemaExtensionId);
				item.Name = "NewEvent";
			}
			else if(type == typeof(StateMachineDynamicLookupParameterMapping))
			{
				item = new StateMachineDynamicLookupParameterMapping(schemaExtensionId);
				item.Name = "NewParameterMapping";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorStateMachineStateUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}

		#endregion
	}
}
