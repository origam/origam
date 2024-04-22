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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;


namespace Origam.Schema.WorkflowModel;

public enum StateMachineEventType
{
	StateEntry = 0,
	StateTransition = 1,
	StateExit = 2,
	RecordCreated = 3,
	RecordUpdated = 4,
	RecordDeleted = 5,
	RecordCreatedUpdated = 6,
	BeforeRecordDeleted = 7,
}

/// <summary>
/// Summary description for StateMachineState.
/// </summary>
[SchemaItemDescription("Event", "Events", "event-4.png")]
[HelpTopic("Data+Events")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class StateMachineEvent : AbstractSchemaItem
{
	public const string CategoryConst = "StateMachineEvent";

	public StateMachineEvent() : base() {Init();}

	public StateMachineEvent(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}

	public StateMachineEvent(Key primaryKey) : base(primaryKey)	{Init();}

	private void Init()
	{
			this.ChildItemTypes.Add(typeof(StateMachineEventParameterMapping));
			this.ChildItemTypes.Add(typeof(StateMachineEventFieldDependency));
		}

	#region Overriden AbstractSchemaItem Members
		
	public override string ItemType => CategoryConst;

	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
			dependencies.Add(this.Action);
			if(this.OldState != null)	dependencies.Add(this.OldState);
			if(this.NewState != null)	dependencies.Add(this.NewState);

			base.GetExtraDependencies (dependencies);
		}

	public override void UpdateReferences()
	{
			foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
			{
				if(item.OldPrimaryKey != null)
				{
					if(this.NewState != null && item.OldPrimaryKey.Equals(this.NewState.PrimaryKey))
					{
						this.NewState = item as StateMachineState;
						break;
					}
					if(this.OldState != null && item.OldPrimaryKey.Equals(this.OldState.PrimaryKey))
					{
						this.OldState = item as StateMachineState;
						break;
					}
				}
			}

			base.UpdateReferences ();
		}
	#endregion

	#region Properties
	[Browsable(false)]
	public ArrayList ParameterMappings => this.ChildItemsByType(StateMachineEventParameterMapping.CategoryConst);

	[Browsable(false)]
	public ArrayList FieldDependencies => this.ChildItemsByType(StateMachineEventFieldDependency.CategoryConst);
		
	[XmlAttribute ("type")]
	public StateMachineEventType Type { get; set; }
		
	public Guid ActionId;

	[TypeConverter(typeof(WorkflowConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[XmlReference("action", "ActionId")]
	public IWorkflow Action
	{
		get
		{
				ModelElementKey key = new ModelElementKey(this.ActionId);

				return (IWorkflow)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
		set => this.ActionId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
		
	public Guid OldStateId;

	[TypeConverter(typeof(StateMachineStateConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[XmlReference("oldState", "OldStateId")]
	public StateMachineState OldState
	{
		get
		{
				ModelElementKey key = new ModelElementKey(this.OldStateId);

				return (StateMachineState)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
		set => this.OldStateId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
		
	public Guid NewStateId;

	[TypeConverter(typeof(StateMachineStateConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[XmlReference("newState", "NewStateId")]
	public StateMachineState NewState
	{
		get
		{
				ModelElementKey key = new ModelElementKey(this.NewStateId);

				return (StateMachineState)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
		set => this.NewStateId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
		
	[XmlAttribute ("roles")]
	public string Roles { get; set; } = "*";
		
	[XmlAttribute ("features")]
	public string Features { get; set; }
	#endregion

	public override int CompareTo(object obj)
	{
			StateMachineEvent compared = obj as StateMachineEvent;
            if (compared == null)
            {
                return base.CompareTo(obj);
            }
			return (this.Type.CompareTo(compared.Type));
		}
}