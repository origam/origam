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

namespace Origam.Schema.MenuModel;

/// <summary>
/// Summary description for Menu.
/// </summary>
[SchemaItemDescription("Context Menu", "home.png")]
[HelpTopic("Context+Menu")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("1.0.0")]
public class ContextMenu : AbstractSchemaItem, ISchemaItemFactory
{
	public const string CategoryConst = "Menu";

	public ContextMenu() : base() { Init(); }

	public ContextMenu(Guid schemaExtensionId) : base(schemaExtensionId) { Init(); }

	public ContextMenu(Key primaryKey) : base(primaryKey) { Init(); }

	private void Init()
	{
			ChildItemTypes.Add(typeof(Submenu));
			ChildItemTypes.Add(typeof(FormReferenceMenuItem));
			ChildItemTypes.Add(typeof(DataConstantReferenceMenuItem));
			ChildItemTypes.Add(typeof(WorkflowReferenceMenuItem));
			ChildItemTypes.Add(typeof(ReportReferenceMenuItem));
			ChildItemTypes.Add(typeof(DashboardMenuItem));
			ChildItemTypes.Add(typeof(DynamicMenu));

		}
	#region Overriden AbstractSchemaItem Members

	public override string ItemType
	{
		get
		{
				return CategoryConst;
			}
	}

	[Browsable(false)]
	public override bool UseFolders
	{
		get
		{
				return false;
			}
	}
	#endregion
}