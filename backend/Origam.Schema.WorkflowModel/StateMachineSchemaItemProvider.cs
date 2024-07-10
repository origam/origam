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

using System;
using System.Collections;

namespace Origam.Schema.WorkflowModel;
public class StateMachineSchemaItemProvider : AbstractSchemaItemProvider
{
	public StateMachineSchemaItemProvider() {}
	#region Public Methods
	public StateMachine GetMachine(Guid entityId, Guid fieldId)
	{
        var result = new ArrayList();
		var childItems = ChildItems;
		foreach(StateMachine stateMachine in childItems)
		{
			if(stateMachine.EntityId.Equals(entityId) 
			&& stateMachine.FieldId.Equals(fieldId))
			{
				result.Add(stateMachine);
			}
		}
        if(result.Count == 1)
        {
            return (StateMachine)result[0];
        }
        if(result.Count > 1)
        {
            var stateMachine = (StateMachine)result[0];
            throw new Exception(
	            $"More than 1 state machine defined on an entity {stateMachine.Entity.Name} field {stateMachine.Field.Name}. Only one state machine can be defined.");
        }
        return null;
	}
    public ArrayList GetMachines(Guid entityId)
    {
        var result = new ArrayList();
        var childItems = ChildItems;
        foreach (StateMachine stateMachine in childItems)
        {
            if (stateMachine.EntityId.Equals(entityId) 
			&& stateMachine.FieldId.Equals(Guid.Empty))
            {
                result.Add(stateMachine);
            }
        }
        return result;
    }
    #endregion
	#region ISchemaItemProvider Members
	public override string RootItemType => StateMachine.CategoryConst;
	public override bool AutoCreateFolder => true;
	public override string Group => "BL";
	#endregion
	#region IBrowserNode Members
	public override string Icon =>
		// TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
		"state-workflows-2.png";
	public override string NodeText
	{
		get => "State Workflows";
		set => base.NodeText = value;
	}
	public override string NodeToolTipText =>
		// TODO:  Add EntityModelSchemaItemProvider.NodeToolTipText getter implementation
		null;
	#endregion
	#region ISchemaItemFactory Members
	public override Type[] NewItemTypes => new[]
	{
		typeof(StateMachine)
	};
	public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
	{
		return base.NewItem<T>(schemaExtensionId, group, 
			typeof(T) == typeof(StateMachine) ?
				"NewStateMachine" : null);
	}
	#endregion
}
