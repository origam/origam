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
using System.Xml.XPath;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using Origam.DA;
using Origam.Service.Core;

namespace Origam.Schema.WorkflowModel;
[SchemaItemDescription("State Workflow", "state-workflow-2.png")]
[HelpTopic("State+Workflows")]
[DefaultProperty("Entity")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class StateMachine : AbstractSchemaItem
{
	public const string CategoryConst = "WorkflowStateMachine";
	public StateMachine() {}
	public StateMachine(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public StateMachine(Key primaryKey) : base(primaryKey) {}
	#region Public Methods
	public StateMachineState GetState(object value)
	{
		return AllStates()
			.FirstOrDefault(state => 
				(state.Type != StateMachineStateType.Group) 
				&& state.Value.Equals(value));
	}
	public List<StateMachineState> AllStates()
	{
		var result = new List<StateMachineState>();
		foreach(ISchemaItem item in ChildItemsRecursive)
		{
			if(item is StateMachineState state)
			{
				result.Add(state);
			}
		}
		return result;
	}
	#endregion
	#region Overriden ISchemaItem Members
	
	public override string ItemType => CategoryConst;
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		dependencies.Add(Entity);
		if(Field != null)
		{
			dependencies.Add(Field);
		}
		if(DynamicOperationsLookup != null)
		{
			dependencies.Add(DynamicOperationsLookup);
		}
		if(DynamicStatesLookup != null)
		{
			dependencies.Add(DynamicStatesLookup);
		}
		base.GetExtraDependencies (dependencies);
	}
	#endregion
	#region Properties
	[Browsable(false)]
	public List<StateMachineEvent> Events => ChildItemsByType<StateMachineEvent>(
		StateMachineEvent.CategoryConst);
	[Browsable(false)]
	public List<StateMachineDynamicLookupParameterMapping> ParameterMappings => ChildItemsByType<StateMachineDynamicLookupParameterMapping>(
		StateMachineDynamicLookupParameterMapping.CategoryConst);
	public object[] DynamicOperations(IXmlContainer data)
	{
		var dataView = GetDynamicList(DynamicOperationsLookupId, data);
		var result = new object[dataView.Count];
		for(var i=0; i < dataView.Count; i++)
		{
			result[i] = dataView[i][DynamicOperationsLookup
				.ListValueMember];
		}
		return result;
	}
	public object[] InitialStateValues(IXmlContainer data)
	{
		var list = new ArrayList();
		if(DynamicStatesLookup == null)
		{
			// states defined in the model
			foreach(StateMachineState state in AllStates())
			{
				if(state.Type == StateMachineStateType.Initial)
				{
					list.Add(state.Value);
				}
			}
		}
		else
		{
			var dataView = GetDynamicList(DynamicStatesLookupId, data);
			// dynamic state list from the data source
			foreach(DataRowView dataRowView in dataView)
			{
				if((bool)dataRowView["IsInitial"])
				{
					list.Add(dataRowView["Id"]);
				}
			}
		}
		return list.ToArray();
	}
	private DataView GetDynamicList(Guid lookupId, IXmlContainer data)
	{
		DataView view;
		var dataLookupService 
			= ServiceManager.Services.GetService<IDataLookupService>();
		if(ParameterMappings.Count == 0)
		{
			view = dataLookupService.GetList(lookupId, null);
		}
		else
		{
			if(data == null)
			{
				view = new DataView(new OrigamDataTable());
			}
			else
			{
				var parameters = new Dictionary<string, object>();
				foreach(StateMachineDynamicLookupParameterMapping 
					        parameterMapping in ParameterMappings)
				{
					var xpath 
						= "/row/" + (parameterMapping .Field.XmlMappingType 
						             == EntityColumnXmlMapping.Attribute 
							? "@" : "") + parameterMapping.Field.Name;
					var xPathNavigator = data.Xml.CreateNavigator();
					var value = xPathNavigator.Evaluate(xpath);
					if(value is XPathNodeIterator iterator)
					{
						if(iterator.Count == 0)
						{
							value = null;
						}
						else
						{
							iterator.MoveNext();
							value = iterator.Current.Value;
						}
					}
					else
					{
						value = null;
					}
					parameters.Add(parameterMapping.Name, value);
				}
				view = dataLookupService.GetList(
					lookupId, parameters, null);
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
		get => (IDataEntity)PersistenceProvider.RetrieveInstance(
			typeof(ISchemaItem), new ModelElementKey(EntityId));
		set
		{
			EntityId = (value == null) 
				? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			SetName();
		}
	}
	
	public Guid FieldId;
	[TypeConverter(typeof(StateMachineEntityFieldConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[XmlReference("field", "FieldId")]
	public IDataEntityColumn Field
	{
		get => (IDataEntityColumn)PersistenceProvider.RetrieveInstance(
			typeof(ISchemaItem), new ModelElementKey(FieldId));
		set
		{
			FieldId = (value == null) 
				? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			SetName();
		}
	}
	
	public Guid DynamicStatesLookupId;
	[TypeConverter(typeof(DataLookupConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[XmlReference("dynamicStatesLookup", "DynamicStatesLookupId")]
	public IDataLookup DynamicStatesLookup
	{
		get => (IDataLookup)PersistenceProvider.RetrieveInstance(
			typeof(ISchemaItem), 
			new ModelElementKey(DynamicStatesLookupId));
		set => DynamicStatesLookupId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
	
	public Guid DynamicOperationsLookupId;
	[TypeConverter(typeof(DataLookupConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[XmlReference("dynamicOperationsLookup", "DynamicOperationsLookupId")]
	public IDataLookup DynamicOperationsLookup
	{
		get => (IDataLookup)PersistenceProvider.RetrieveInstance(
			typeof(ISchemaItem), 
			new ModelElementKey(DynamicOperationsLookupId));
		set => DynamicOperationsLookupId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
	
	public Guid ReverseLookupId;
	[TypeConverter(typeof(DataLookupConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[XmlReference("reverseLookup", "ReverseLookupId")]
	public IDataLookup ReverseLookup
	{
		get => (IDataLookup)PersistenceProvider.RetrieveInstance(
			typeof(ISchemaItem), new ModelElementKey(
				ReverseLookupId));
		set => ReverseLookupId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
	#endregion
	#region Private Methods
	private void SetName()
	{
		if((Entity != null) && (Field != null))
		{
			Name = Entity.Name + "_" + Field.Name;
		}
		else if(Entity != null)
		{
			Name = Entity.Name;
		}
		else
		{
			Name = "";
		}
	}
	#endregion
	#region ISchemaItemFactory Members
	public override Type[] NewItemTypes => new[] 
	{ 
		typeof(StateMachineState),
		typeof(StateMachineEvent),
		typeof(StateMachineDynamicLookupParameterMapping)
	};
	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
		string itemName = null;
		if(typeof(T) == typeof(StateMachineState))
		{
			itemName = "NewState";
		}
		else if(typeof(T) == typeof(StateMachineEvent))
		{
			itemName = "NewEvent";
		}
		else if(typeof(T) 
		== typeof(StateMachineDynamicLookupParameterMapping))
		{
			itemName = "NewParameterMapping";
		}
		return base.NewItem<T>(schemaExtensionId, group, itemName);
	}
	#endregion
}
