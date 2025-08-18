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
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Schema.Wizards;
/// <summary>
/// Summary description for SetInheritanceOff.
/// </summary>
public class CreatePackageFolders : AbstractMenuCommand
{
	WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
	public override bool IsEnabled
	{
		get
		{
			return _schema.ActiveNode is Package;
		}
		set
		{
			throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
		}
	}
	public override void Run()
	{
		foreach(ISchemaItemProvider provider in _schema.Providers)
		{
			SchemaItemGroup group = provider.NewGroup(_schema.ActiveSchemaExtensionId);
			group.Name = _schema.ActiveExtension.Name;
			group.Persist();
		}
		(_schema.ActiveNode as Package).Refresh();
	}
	private static void SetInheritance(ISchemaItem item, bool value)
	{
		item.Inheritable = value;
		foreach(ISchemaItem child in item.ChildItems)
		{
			if(child.DerivedFrom == null)
			{
				SetInheritance(child, value);
			}
		}
	}
}
