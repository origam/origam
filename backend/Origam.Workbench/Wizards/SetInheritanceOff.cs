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

namespace Origam.Schema.Wizards;

/// <summary>
/// Summary description for SetInheritanceOff.
/// </summary>
public class SetInheritanceOff : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
                AbstractSchemaItem item = Owner as AbstractSchemaItem;
                return item != null && item.Inheritable;
			}
		set
		{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
	}

	public override void Run()
	{
			AbstractSchemaItem item = Owner as AbstractSchemaItem;

			SetInheritance(item, false);
			item.ClearCacheOnPersist = false;
			item.Persist();
			item.ClearCacheOnPersist = true;
		}

	private static void SetInheritance(AbstractSchemaItem item, bool value)
	{
			item.Inheritable = value;

			foreach(AbstractSchemaItem child in item.ChildItems)
			{
				if(child.DerivedFrom == null)
				{
					SetInheritance(child, value);
				}
			}
		}
}