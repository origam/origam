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
/// Summary description for Submenu.
/// </summary>
[SchemaItemDescription("Dynamic Menu", "icon_dynamic-menu.png")]
[HelpTopic("Dynamic+Menu")]
[ClassMetaVersion("6.0.0")]
public class DynamicMenu : AbstractMenuItem, ISchemaItemFactory
{
	public DynamicMenu() : base() {}
	public DynamicMenu(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public DynamicMenu(Key primaryKey) : base(primaryKey)	{}
	[Browsable(false)]
	public override string Roles
	{
		get
		{
            return "*";
		}
		set
		{
            throw new InvalidOperationException();
		}
	}
	string _classPath;
    [XmlAttribute("classPath")]
	public string ClassPath
	{
		get
		{
			return _classPath;
		}
		set
		{
            _classPath = value;
		}
	}
}
