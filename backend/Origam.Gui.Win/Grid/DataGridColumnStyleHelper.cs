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
using System.Windows.Forms;
using System.Data;

using Origam.Schema.EntityModel;

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for DataGridColumnStyleHelper.
	/// </summary>
	public class DataGridColumnStyleHelper
	{
		private DataGridColumnStyleHelper()
		{
		}

		public static EntityFormatting Formatting(DataGridColumnStyle columnStyle, CurrencyManager source, int rowNum)
		{
			try
			{
				if(columnStyle.DataGridTableStyle == null) return null;

				AsPanel panel = columnStyle.DataGridTableStyle.DataGrid.Parent as AsPanel;
				if(panel != null)
				{
					DataRow row = (source.List[rowNum] as DataRowView).Row;

					if(row.Table.Columns.Contains("Id"))
					{
						if(row["id"] is Guid)
						{
							return panel.Formatting(row, (Guid)row["Id"]);
						}
					}
				}
			}
			catch{}

			return null;
		}
	}
}
