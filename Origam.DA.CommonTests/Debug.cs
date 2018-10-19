#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.Data;
using System.Windows.Forms;

//using System.Windows.Forms;

namespace Origam.DA
{
	/// <summary>
	/// Summary description for Debug.
	/// </summary>
	public sealed class DebugClass
	{
		private static Form _form;
		private static DataGrid dataGrid1;
        private const int MAX_ERRORS_PER_TABLE = 10;
		
		private DebugClass()
		{
		}

		private static void CheckInit()
		{
			if(_form ==null)
			{
				_form = new Form();
				dataGrid1 = new DataGrid();
				((System.ComponentModel.ISupportInitialize)(dataGrid1)).BeginInit();
				_form.SuspendLayout();
				// 
				// dataGrid1
				// 
				dataGrid1.DataMember = "";
				dataGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
				dataGrid1.Name = "dataGrid1";
				dataGrid1.TabIndex = 0;
				// 
				// Form2
				// 
				_form.Controls.Add(dataGrid1);
				_form.Name = "TEST_FORM";
				_form.Text = "DebugForm";
				((System.ComponentModel.ISupportInitialize)(dataGrid1)).EndInit();
				_form.ResumeLayout(false);				
			
			}
		}


		public static void Show(DataSet dataSet)
		{
			CheckInit();
			dataGrid1.DataSource = dataSet;
			_form.ShowDialog();
		}


	}
}
