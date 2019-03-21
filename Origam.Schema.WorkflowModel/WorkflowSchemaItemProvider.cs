#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for WorkflowSchemaItemProvider.
	/// </summary>
	public class WorkflowSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory
	{
		public WorkflowSchemaItemProvider()
		{
		}

		#region ISchemaItemProvider Members
		public override string RootItemType
		{
			get
			{
				return Workflow.ItemTypeConst;
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
				return "icon_30_sequential-workflows.png";
			}
		}

		public override string NodeText
		{
			get
			{
				return "Sequential Workflows";
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
				return new Type[1] {typeof(Workflow)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			if(type == typeof(Workflow))
			{
				Workflow item = new Workflow(schemaExtensionId);
				item.RootProvider = this;
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewWorkflow";

				item.Group = group;
				this.ChildItems.Add(item);

				return item;
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorWorkflowModelUnknownType"));
		}

		#endregion
	}
}
