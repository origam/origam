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
using System.Collections;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Workbench.Commands;

namespace Origam.Workbench.Pads
{
	public class FindSchemaItemResultsPad : AbstractResultPad
    {
		private System.Windows.Forms.ListView lvwResults;
		private System.Windows.Forms.ColumnHeader colItemType;
		private System.Windows.Forms.ColumnHeader colRootType;
		private System.Windows.Forms.ColumnHeader colItemPath;
		private System.Windows.Forms.ColumnHeader colFolderPath;
		private System.ComponentModel.IContainer components = null;

		private int sortColumn;

		private SchemaBrowser _schemaBrowser;
		ArrayList _results = new ArrayList();

		public FindSchemaItemResultsPad()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			_schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
			lvwResults.SmallImageList = _schemaBrowser.EbrSchemaBrowser.imgList;
			lvwResults.ColumnClick += OnColumnClick; 
		}

		private void OnColumnClick(object sender, ColumnClickEventArgs eventArgs)
		{
			if (eventArgs.Column != sortColumn)
			{
				sortColumn = eventArgs.Column;
				lvwResults.Sorting = SortOrder.Ascending;
			}
			else
			{
				lvwResults.Sorting = lvwResults.Sorting == SortOrder.Ascending ?
					SortOrder.Descending : SortOrder.Ascending;
			}
			this.lvwResults.ListViewItemSorter = new ListViewItemComparer(eventArgs.Column,
				lvwResults.Sorting);
			lvwResults.Sort();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}

				_schemaBrowser = null;
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FindSchemaItemResultsPad));
			this.lvwResults = new System.Windows.Forms.ListView();
			this.colItemPath = new System.Windows.Forms.ColumnHeader();
			this.colRootType = new System.Windows.Forms.ColumnHeader();
			this.colItemType = new System.Windows.Forms.ColumnHeader();
			this.colFolderPath = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// lvwResults
			// 
			this.lvwResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lvwResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						 this.colItemPath,
																						 this.colRootType,
																						 this.colItemType,
																						 this.colFolderPath});
			this.lvwResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvwResults.FullRowSelect = true;
			this.lvwResults.Location = new System.Drawing.Point(0, 0);
			this.lvwResults.MultiSelect = false;
			this.lvwResults.Name = "lvwResults";
			this.lvwResults.Size = new System.Drawing.Size(816, 245);
			this.lvwResults.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.lvwResults.TabIndex = 0;
			this.lvwResults.View = System.Windows.Forms.View.Details;
			this.lvwResults.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvwResults_KeyDown);
			this.lvwResults.DoubleClick += new System.EventHandler(this.lvwResults_DoubleClick);
			// 
			// colItemPath
			// 
			this.colItemPath.Text = ResourceUtils.GetString("FoundInTitle");
			this.colItemPath.Width = 375;
			// 
			// colRootType
			// 
			this.colRootType.Text = ResourceUtils.GetString("RootTypeTitle");
			this.colRootType.Width = 131;
			// 
			// colItemType
			// 
			this.colItemType.Text = ResourceUtils.GetString("TypeTitle");
			this.colItemType.Width = 120;
			// 
			// colFolderPath
			// 
			this.colFolderPath.Text = ResourceUtils.GetString("FolderTitle");
			this.colFolderPath.Width = 324;
			// 
			// FindSchemaItemResultsPad
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(816, 245);
			this.Controls.Add(this.lvwResults);
			this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft) 
				| WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight) 
				| WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop) 
				| WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
			this.HideOnClose = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FindSchemaItemResultsPad";
			this.TabText = ResourceUtils.GetString("FindResultsTitle");
			this.Text = ResourceUtils.GetString("FindResultsTitle");
			this.ResumeLayout(false);

		}
		#endregion

		#region Public Methods
		public void ResetResults()
		{
			lvwResults.Items.Clear();
			_results.Clear();
			if(_schemaBrowser != null)
			{
				_schemaBrowser.RedrawContent();
			}
		}

		public void DisplayResults(AbstractSchemaItem[] results)
		{
			ResetResults();
			if(results.Length > 0)
			{
                ListViewItem[] resultListItems = new ListViewItem[results.LongLength];
                for (int i = 0; i < results.LongLength; i++)
                {
                    var item = results[i];
                    resultListItems[i] = GetResult(item);
                    _results.Add(item);
                }
                lvwResults.Items.AddRange(resultListItems);
                ViewFindSchemaItemResultsPad cmd = new ViewFindSchemaItemResultsPad();
				cmd.Run();
			}
			_schemaBrowser.RedrawContent();
		}

		public ArrayList Results
		{
			get
			{
				return _results;
			}
		}

		private ListViewItem GetResult(AbstractSchemaItem item)
		{
			if(item == null) return null;

			if(! LicensePolicy.ModelElementPolicy(item.GetType().Name, ModelElementPolicyCommand.Show))
			{
				return null;
			}

			string name = SchemaItemName(item.GetType());
			string rootName = SchemaItemName(item.RootItem.GetType());

			if(name == null) name = item.ItemType;
			if(rootName == null) rootName = item.RootItem.ItemType;

			ListViewItem newItem = new ListViewItem(new string[] {item.Path, rootName, name, item.RootItem.Group == null ? "" : item.RootItem.Group.Path});
			newItem.Tag = item;
			newItem.ImageIndex = Convert.ToInt32(item.RootItem.Icon);

			return newItem;
		}
		#endregion

		private void ActivateItem()
		{
			if(lvwResults.SelectedItems.Count > 0)
			{
				try
				{
                    AbstractSchemaItem schemaItem = lvwResults.SelectedItems[0].Tag as AbstractSchemaItem;
                    OpenParentPackage(schemaItem.SchemaExtensionId);
                    _schemaBrowser.EbrSchemaBrowser.SelectItem(schemaItem);
					ViewSchemaBrowserPad cmd = new ViewSchemaBrowserPad();
					cmd.Run();
				}
				catch(Exception ex)
				{
					Origam.UI.AsMessageBox.ShowError(this, ex.Message, ResourceUtils.GetString("ErrorTitle"), ex);
				}
			}
		}

        private void lvwResults_DoubleClick(object sender, System.EventArgs e)
		{
			ActivateItem();
		}

		private string SchemaItemName(Type type)
		{
			object[] attributes = type.GetCustomAttributes(typeof(SchemaItemDescriptionAttribute), true);

			if(attributes != null && attributes.Length > 0)
				return (attributes[0] as SchemaItemDescriptionAttribute).Name;
			else
				return null;

		}

		private void lvwResults_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Enter)
			{
				ActivateItem();
			}
		}

		public void Clear()
		{
			lvwResults.Items.Clear();
		}
	}
	internal class ListViewItemComparer : IComparer {
		private readonly int col;
		private readonly SortOrder order;

		public ListViewItemComparer(int column, SortOrder order) 
		{
			col=column;
			this.order = order;
		}
		public int Compare(object x, object y) 
		{
			int returnVal= -1;
			returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text,
				((ListViewItem)y).SubItems[col].Text);
			if (order == SortOrder.Descending)
			{
				returnVal *= -1;
			}
			return returnVal;
		}
	}
}

