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
using Origam.Schema.EntityModel;


namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for StateMachineState.
/// </summary>
[SchemaItemDescription("Transition", "Operations", "transition-2.png")]
[HelpTopic("State+Transition")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class StateMachineOperation : AbstractSchemaItem
{
	public const string CategoryConst = "StateMachineOperation";

	public StateMachineOperation() : base() {}

	public StateMachineOperation(Guid schemaExtensionId) : base(schemaExtensionId) {}

	public StateMachineOperation(Key primaryKey) : base(primaryKey)	{}

	#region Overriden AbstractSchemaItem Members
		
	public override string ItemType
	{
		get
		{
				return CategoryConst;
			}
	}

	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
			dependencies.Add(this.TargetState);
			if(this.Rule != null)	dependencies.Add(this.Rule);

			base.GetExtraDependencies (dependencies);
		}

	public override void UpdateReferences()
	{
			foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
			{
				if(item.OldPrimaryKey != null)
				{
					if(item.OldPrimaryKey.Equals(this.TargetState.PrimaryKey))
					{
						this.TargetState = item as StateMachineState;
						break;
					}
				}
			}

			base.UpdateReferences ();
		}
	#endregion

	#region Properties
	private string _roles = "*";
	[NotNullModelElementRule()]
	[DefaultValue("*")]
	[XmlAttribute("roles")]
	public string Roles
	{
		get
		{
				return _roles;
			}
		set
		{
				_roles = value;
			}
	}

	private string _features;
	[XmlAttribute("features")]
	public string Features
	{
		get
		{
				return _features;
			}
		set
		{
				_features = value;
			}
	}		
        
	public Guid RuleId;

	[TypeConverter(typeof(EntityRuleConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[XmlReference("rule", "RuleId")]
	public IEntityRule Rule
	{
		get
		{
				ModelElementKey key = new ModelElementKey(this.RuleId);

                return (IEntityRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
		set
		{
				this.RuleId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
	}
		
	public Guid TargetStateId;

	[TypeConverter(typeof(StateMachineStateConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[XmlReference("targetState", "TargetStateId")]
	public StateMachineState TargetState
	{
		get
		{
				ModelElementKey key = new ModelElementKey(this.TargetStateId);

				return (StateMachineState)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
		set
		{
				this.TargetStateId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
	}
	#endregion

}