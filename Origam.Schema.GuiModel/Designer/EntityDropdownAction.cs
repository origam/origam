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

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for EntityDropdownAction.
	/// </summary>
	[SchemaItemDescription("Dropdown Action", "UI Actions", 
        "icon_dropdown-action.png")]
    [HelpTopic("DropDown+Action")]
    [ClassMetaVersion("6.0.0")]
    public class EntityDropdownAction : EntityUIAction
	{
		public EntityDropdownAction() : base() {Init();}

		public EntityDropdownAction(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}

		public EntityDropdownAction(Key primaryKey) : base(primaryKey)	{Init();}
	
		private void Init()
		{
			this.ChildItemTypes.Remove(typeof(EntityUIActionParameterMapping));
		}

		[Browsable(false)]
		public override PanelActionType ActionType
		{
			get
			{
				return PanelActionType.Dropdown;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

	}
}
