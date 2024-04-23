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

using Origam.Schema;

namespace Origam.Workbench;

/// <summary>
/// Summary description for PropertyPadListItem.
/// </summary>
public class PropertyPadListItem
{
	private Control _control;

	public PropertyPadListItem(Control control)
	{
			_control = control;
		}

	public Control Control
	{
		get
		{
				return _control;
			}
		set
		{
				_control = value;
			}
	}

	public override string ToString()
	{
			ISchemaItem si = _control.Tag as ISchemaItem;

			if(si == null)
			{
				return base.ToString();
			}
			else
			{
				return si.Name + " [" + _control.GetType().FullName + "]";
			}
		}

}