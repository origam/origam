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

using System.Windows.Forms;
using System.ComponentModel;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.Gui.Designer;
/// <summary>
/// Summary description for ControlExtenderProvider.
/// </summary>
[ProvideProperty("SchemaItemName", typeof(Control))]
[ProvideProperty("Roles", typeof(Control))]
[ProvideProperty("Features", typeof(Control))]
[ProvideProperty("SchemaItemId", typeof(Control))]
public class ControlExtenderProvider : IExtenderProvider
{
	public ControlExtenderProvider()
	{
	}
	[ExtenderProvidedProperty()]
	[Category("(ORIGAM)")]
	public string GetSchemaItemId(Control acontrol)
	{
		ISchemaItem si = acontrol.Tag as ISchemaItem;
		if(si != null)
		{
			return si.PrimaryKey["Id"].ToString();
		}

        return null;
    }
	[ExtenderProvidedProperty()]
	[Category("(ORIGAM)")]
	public string GetSchemaItemName(Control acontrol)
	{
		ISchemaItem si = acontrol.Tag as ISchemaItem;
		if(si != null)
		{
			return si.Name;
		}

        return null;
    }
	public void SetSchemaItemName(Control acontrol, string value)
	{
		ISchemaItem si = acontrol.Tag as ISchemaItem;
		if(si != null)
		{
			si.Name = value;
		}
	}
	[Category("(ORIGAM)")]
	public string GetRoles(Control acontrol)
	{
		ControlSetItem csi = acontrol.Tag as ControlSetItem;
		if(csi != null)
		{
			return csi.Roles;
		}

        return null;
    }
	public void SetRoles(Control acontrol, string value)
	{
		ControlSetItem csi = acontrol.Tag as ControlSetItem;
		if(csi != null)
		{
			csi.Roles = value;
		}
	}
	[Category("(ORIGAM)")]
	public string GetFeatures(Control acontrol)
	{
		ControlSetItem csi = acontrol.Tag as ControlSetItem;
		if(csi != null)
		{
			return csi.Features;
		}

        return null;
    }
	public void SetFeatures(Control acontrol, string value)
	{
		ControlSetItem csi = acontrol.Tag as ControlSetItem;
		if(csi != null)
		{
			csi.Features = value;
		}
	}
	#region IExtenderProvider Members
	public bool CanExtend(object extendee) 
	{
		if (extendee is Control && (extendee as Control).Tag is ISchemaItem)
        {
            return true;
        }

        return false;
    }
	#endregion
}
