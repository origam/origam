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

using System.Data;
using System.Windows.Forms;

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for DataBindingTools.
/// </summary>
public class DataBindingTools
{
	public static DataRow CurrentRow(Control control, string property)
	{
		if(control.Parent is AsDataGrid)
		{
			AsDataGrid grid = control.Parent as AsDataGrid;
			if(grid.DataSource == null)
            {
                return null;
            }

            if (grid.BindingContext[grid.DataSource, grid.DataMember] == null)
            {
                return null;
            }

            if (grid.BindingContext[grid.DataSource, grid.DataMember].Position < 0)
            {
                return null;
            }

            return (grid.BindingContext[grid.DataSource, grid.DataMember].Current as DataRowView).Row;
		}
		else if(control.Parent is FilterPanel)
		{
			return null;
			//AsPanel panel = this.Parent.Parent as AsPanel;
			//return ((DataRowView)(panel.BindingContext[panel.DataSource, panel.DataMember] as CurrencyManager).Current).Row;
		}
		else
		{
			if(control.DataBindings[property] == null)
            {
                return null;
            }

            if (control.DataBindings[property].BindingManagerBase == null)
            {
                return null;
            }

            if (control.DataBindings[property].BindingManagerBase.Position < 0)
            {
                return null;
            }

            return ((DataRowView)control.DataBindings[property].BindingManagerBase.Current).Row;
		}
	}
    public static void UpdateBindedFormComponent(
        BindingsCollection bindings, string mappingName)
    {
        foreach (Binding binding in bindings)
        {
            if (binding.BindingMemberInfo.BindingField == mappingName)
            {
                binding.ReadValue();
                break;
            }
        }
    }
}
