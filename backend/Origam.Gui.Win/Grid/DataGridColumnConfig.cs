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

using System.Collections;
using System.Windows.Forms;

using Origam.UI;

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for DataGridColumnConfig.
	/// </summary>
	public class DataGridColumnConfig : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ColumnHeader colField;
		private System.Windows.Forms.Button btnDown;
		private System.Windows.Forms.Button btnUp;
		private System.Windows.Forms.Button btnOK;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DataGridColumnConfig()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.panel1.BackColor = OrigamColorScheme.FormBackgroundColor;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(DataGridColumnConfig));
			this.listView1 = new System.Windows.Forms.ListView();
			this.colField = new System.Windows.Forms.ColumnHeader();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnDown = new System.Windows.Forms.Button();
			this.btnUp = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// listView1
			// 
			this.listView1.CheckBoxes = true;
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.colField});
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.FullRowSelect = true;
			this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(0, 0);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(234, 296);
			this.listView1.TabIndex = 0;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// colField
			// 
			this.colField.Text = "Sloupce";
			this.colField.Width = 201;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.btnOK);
			this.panel1.Controls.Add(this.btnDown);
			this.panel1.Controls.Add(this.btnUp);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel1.Location = new System.Drawing.Point(234, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(104, 296);
			this.panel1.TabIndex = 1;
			// 
			// btnDown
			// 
			this.btnDown.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnDown.Image = ((System.Drawing.Image)(resources.GetObject("btnDown.Image")));
			this.btnDown.Location = new System.Drawing.Point(8, 152);
			this.btnDown.Name = "btnDown";
			this.btnDown.Size = new System.Drawing.Size(24, 24);
			this.btnDown.TabIndex = 3;
			this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
			// 
			// btnUp
			// 
			this.btnUp.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnUp.Image = ((System.Drawing.Image)(resources.GetObject("btnUp.Image")));
			this.btnUp.Location = new System.Drawing.Point(8, 104);
			this.btnUp.Name = "btnUp";
			this.btnUp.Size = new System.Drawing.Size(24, 24);
			this.btnUp.TabIndex = 2;
			this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
			// 
			// btnOK
			// 
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnOK.Location = new System.Drawing.Point(8, 16);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(88, 24);
			this.btnOK.TabIndex = 1;
			this.btnOK.Text = ResourceUtils.GetString("OK");
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// DataGridColumnConfig
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(338, 296);
			this.ControlBox = false;
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DataGridColumnConfig";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = ResourceUtils.GetString("ColumnConfig");
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private ArrayList _columns;

		private void listView1_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			DataGridColumnStyleHolder column = listView1.Items[e.Index].Tag as DataGridColumnStyleHolder;
			
			if(e.NewValue == CheckState.Checked)
			{
				column.Hidden = false;
			}
			else if(e.NewValue == CheckState.Unchecked)
			{
				column.Hidden = true;
			}
		}
	
		private DataGridTableStyle _tableStyle;

		private void btnUp_Click(object sender, System.EventArgs e)
		{
			if(listView1.SelectedItems.Count == 1)
			{
				ListViewItem item = listView1.SelectedItems[0];

				if(item.Index == 0) return;

				listView1.BeginUpdate();
				try
				{
					int oldIndex = item.Index;
					item.Remove();

					listView1.Items.Insert(oldIndex-1, item);
					UpdateIndexes();
					item.Selected = true;
					item.Focused = true;
					item.EnsureVisible();
				}
				finally
				{
					listView1.EndUpdate();
				}
			}
		}

		private void btnDown_Click(object sender, System.EventArgs e)
		{
			if(listView1.SelectedItems.Count == 1)
			{
				ListViewItem item = listView1.SelectedItems[0];

				if(item.Index == listView1.Items.Count-1) return;

				listView1.BeginUpdate();
				try
				{
					int oldIndex = item.Index;
					item.Remove();

					listView1.Items.Insert(oldIndex+1, item);
					UpdateIndexes();
					item.Selected = true;
					item.Focused = true;
					item.EnsureVisible();
				}
				finally
				{
					listView1.EndUpdate();
				}
			}
		}

		private void UpdateIndexes()
		{
			foreach(ListViewItem it in listView1.Items)
			{
				(it.Tag as DataGridColumnStyleHolder).Index = it.Index;
			}
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		public DataGridTableStyle TableStyle
		{
			get
			{
				return _tableStyle;
			}
			set
			{
				_tableStyle = value;
			}
		}

		public ArrayList Columns
		{
			get
			{
				return _columns;
			}
			set
			{
				this.listView1.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.listView1_ItemCheck);

				_columns = value;

				if(_columns != null)
				{
					foreach(DataGridColumnStyleHolder column in _columns)
					{
						ListViewItem item = new ListViewItem();
						item.Tag = column;
						item.Text = column.Style.HeaderText;
						item.Checked = (! column.Hidden);

						listView1.Items.Add(item);
					}
				}

				this.listView1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listView1_ItemCheck);
			}
		}
	}
}
