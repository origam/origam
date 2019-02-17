#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for StateMachineSchemaItemProvider.
	/// </summary>
	public class StateMachineSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory
	{
		public StateMachineSchemaItemProvider()
		{
		}

		#region Public Methods
		public StateMachine GetMachine(Guid entityId, Guid fieldId)
		{
            ArrayList result = new ArrayList();
			SchemaItemCollection childItems = this.ChildItems;
			foreach(StateMachine sm in childItems)
			{
				if(sm.EntityId.Equals(entityId) && sm.FieldId.Equals(fieldId))
				{
					result.Add(sm);
				}
			}

            if (result.Count == 1)
            {
                return (StateMachine)result[0];
            }
            else if (result.Count > 1)
            {
                StateMachine sm = (StateMachine)result[0];
                throw new Exception(string.Format("More than 1 state machine defined on an entity {0} field {1}. Only one state machine can be defined.",
                    sm.Entity.Name, sm.Field.Name));
            }
            else
            {
			    return null;
            }
		}

        public ArrayList GetMachines(Guid entityId)
        {
            ArrayList result = new ArrayList();
            SchemaItemCollection childItems = this.ChildItems;
            foreach (StateMachine sm in childItems)
            {
                if (sm.EntityId.Equals(entityId) && sm.FieldId.Equals(Guid.Empty))
                {
                    result.Add(sm);
                }
            }

            return result;
        }
        #endregion

		#region ISchemaItemProvider Members
		public override string RootItemType
		{
			get
			{
				return StateMachine.ItemTypeConst;
			}
		}
		public override bool AutoCreateFolder
		{
			get
			{
				return true;
			}
		}
		public override string Group
		{
			get
			{
				return "BL";
			}
		}
		#endregion

		#region IBrowserNode Members

		public override string Icon
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
				return "icon_32_state-workflows.png";
			}
		}

		public override string NodeText
		{
			get
			{
				return "State Workflows";
			}
			set
			{
				base.NodeText = value;
			}
		}

		public override string NodeToolTipText
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.NodeToolTipText getter implementation
				return null;
			}
		}

		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[1] {typeof(StateMachine)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			if(type == typeof(StateMachine))
			{
				StateMachine item = new StateMachine(schemaExtensionId);
				item.RootProvider = this;
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewStateMachine";

				item.Group = group;
				this.ChildItems.Add(item);

				return item;
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorWorkflowModelUknownType"));
		}

		#endregion
	}
}
