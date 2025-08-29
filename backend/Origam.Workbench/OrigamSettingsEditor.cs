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
using System.Drawing;
using System.Windows.Forms;
using Origam.UI;

namespace Origam.Workbench;
/// <summary>
/// Summary description for OrigamSettingsEditor.
/// </summary>
public class OrigamSettingsEditor :  AbstractViewContent
{
	Pads.PropertyPad _propertyPad = WorkbenchSingleton.Workbench.GetPad(typeof(Pads.PropertyPad)) as Pads.PropertyPad;
		
	private System.Windows.Forms.ColumnHeader colName;
	private System.Windows.Forms.ColumnHeader colModelConnection;
	private System.Windows.Forms.ColumnHeader colDatabaseConnection;
	private System.Windows.Forms.ListView lvwConfigurations;
	private System.Windows.Forms.ToolBar toolbar;
	private System.Windows.Forms.ToolBarButton btnAdd;
	private System.Windows.Forms.ToolBarButton btnDelete;
	private System.Windows.Forms.ToolBarButton btnClone;
	private System.Windows.Forms.ImageList imageList1;
	private System.ComponentModel.IContainer components;
	public OrigamSettingsEditor()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
		StatusText = $"Origam Settings loaded from: {ConfigurationManager.UserProfileOrigamSettings}";
		this.Icon = Icon.FromHandle(new Bitmap(Images.ConnectionConfiguration).GetHicon());
		if(_propertyPad == null)
		{
			throw new Exception(ResourceUtils.GetString("ErrorEditConfigurations"));
		}
		_propertyPad.PropertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(PropertyGrid_PropertyValueChanged);
		this.BackColor = OrigamColorScheme.FormBackgroundColor;
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
		this.components = new System.ComponentModel.Container();
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(OrigamSettingsEditor));
		this.lvwConfigurations = new System.Windows.Forms.ListView();
		this.colName = new System.Windows.Forms.ColumnHeader();
		this.colModelConnection = new System.Windows.Forms.ColumnHeader();
		this.colDatabaseConnection = new System.Windows.Forms.ColumnHeader();
		this.toolbar = new System.Windows.Forms.ToolBar();
		this.btnAdd = new System.Windows.Forms.ToolBarButton();
		this.btnDelete = new System.Windows.Forms.ToolBarButton();
		this.btnClone = new System.Windows.Forms.ToolBarButton();
		this.imageList1 = new System.Windows.Forms.ImageList(this.components);
		this.SuspendLayout();
		// 
		// lvwConfigurations
		// 
		this.lvwConfigurations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.colName,
																							this.colModelConnection,
																							this.colDatabaseConnection});
		this.lvwConfigurations.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lvwConfigurations.FullRowSelect = true;
		this.lvwConfigurations.GridLines = true;
		this.lvwConfigurations.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
		this.lvwConfigurations.Location = new System.Drawing.Point(0, 28);
		this.lvwConfigurations.Name = "lvwConfigurations";
		this.lvwConfigurations.Size = new System.Drawing.Size(784, 330);
		this.lvwConfigurations.TabIndex = 0;
		this.lvwConfigurations.View = System.Windows.Forms.View.Details;
		this.lvwConfigurations.SelectedIndexChanged += new System.EventHandler(this.lvwConfigurations_SelectedIndexChanged);
		// 
		// colName
		// 
		this.colName.Text = ResourceUtils.GetString("NameTitle");
		this.colName.Width = 129;
		// 
		// colModelConnection
		// 
		this.colModelConnection.Text = ResourceUtils.GetString("ModelConnectionTitle");
		this.colModelConnection.Width = 270;
		// 
		// colDatabaseConnection
		// 
		this.colDatabaseConnection.Text = ResourceUtils.GetString("DataConnectionTitle");
		this.colDatabaseConnection.Width = 268;
		// 
		// toolbar
		// 
		this.toolbar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
		this.toolbar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																				   this.btnAdd,
																				   this.btnDelete,
																				   this.btnClone});
		this.toolbar.DropDownArrows = true;
		this.toolbar.ImageList = this.imageList1;
		this.toolbar.Location = new System.Drawing.Point(0, 0);
		this.toolbar.Name = "toolbar";
		this.toolbar.ShowToolTips = true;
		this.toolbar.Size = new System.Drawing.Size(784, 28);
		this.toolbar.TabIndex = 1;
		this.toolbar.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
		this.toolbar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolbar_ButtonClick);
		// 
		// btnAdd
		// 
		this.btnAdd.ImageIndex = 0;
		this.btnAdd.Text = ResourceUtils.GetString("ButtonAdd");
		// 
		// btnDelete
		// 
		this.btnDelete.ImageIndex = 1;
		this.btnDelete.Text = ResourceUtils.GetString("ButtonDelete");
		// 
		// btnClone
		// 
		this.btnClone.ImageIndex = 2;
		this.btnClone.Text = ResourceUtils.GetString("ButtonClone");
		// 
		// imageList1
		// 
		this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
		this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
		this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
		// 
		// OrigamSettingsEditor
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(784, 358);
		this.Controls.Add(this.lvwConfigurations);
		this.Controls.Add(this.toolbar);
		this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document;
		this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
		this.Name = "OrigamSettingsEditor";
		this.TabText = "Connection Configuration";
		this.Text = "Connection Configuration";
		this.TitleName = "Connection Configuration";
		this.Closed += new System.EventHandler(this.OrigamSettingsEditor_Closed);
		this.Closing += new System.ComponentModel.CancelEventHandler(this.OrigamSettingsEditor_Closing);
		this.ResumeLayout(false);
	}
	#endregion
	protected override void ViewSpecificLoad(object objectToLoad)
	{
		OrigamSettingsCollection settings = objectToLoad as OrigamSettingsCollection;
		foreach(OrigamSettings setting in settings)
		{
			ListViewItem item = NewItem();
			item.Tag = setting;
			lvwConfigurations.Items.Add(item);
		}
		RefreshList();
	}
	public override void SaveObject()
	{
		OrigamSettingsCollection settings = new OrigamSettingsCollection();
		foreach(ListViewItem item in lvwConfigurations.Items)
		{
			OrigamSettings setting = item.Tag as OrigamSettings;
			settings.Add(setting);
		}
		ConfigurationManager.WriteConfiguration(settings);
	}
	private ListViewItem NewItem()
	{
		return new ListViewItem(new string[3]);
	}
	private void propertyGrid1_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
	{
		this.IsDirty = true;
	}
	private void OrigamSettingsEditor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
	{
		if(IsDirty)
		{
			DialogResult result = MessageBox.Show(
				ResourceUtils.GetString("DoYouWantSave", this.TitleName), 
				ResourceUtils.GetString("SaveTitle"), 
				MessageBoxButtons.YesNoCancel, 
				MessageBoxIcon.Question);
		
			switch(result)
			{
                case DialogResult.Yes:
                    {
                        SaveObject();
                        break;
                    }

                case DialogResult.Cancel:
                    {
                        e.Cancel = true;
                        break;
                    }
            }
		}
	}
	private void lvwConfigurations_SelectedIndexChanged(object sender, System.EventArgs e)
	{
		if(_closing)
        {
            return;
        }

        if (lvwConfigurations.SelectedItems.Count > 0)
		{
			OrigamSettings[] selectedSettings = new OrigamSettings[lvwConfigurations.SelectedItems.Count];
			int i = 0;
			foreach(ListViewItem item in lvwConfigurations.SelectedItems)
			{
				selectedSettings[i] = item.Tag as OrigamSettings;
				i++;
			}
			_propertyPad.PropertyGrid.SelectedObjects = selectedSettings;
			if(!_propertyPad.Visible)
			{
				Commands.ViewPropertyPad cmd = new Origam.Workbench.Commands.ViewPropertyPad();
				cmd.Run();
			}
		}
	}
	private void RefreshList()
	{
		// refresh list
		foreach(ListViewItem item in lvwConfigurations.Items)
		{
			OrigamSettings origamSettings = item.Tag as OrigamSettings;
			item.SubItems[0].Text = origamSettings.Name;
			item.SubItems[1].Text = origamSettings.ModelSourceControlLocation;
			item.SubItems[2].Text = origamSettings.DataConnectionString;
		}
	}
	private void PropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
	{
		RefreshList();
		this.IsDirty = true;
	}
	private void toolbar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
	{
		if(e.Button == btnAdd)
		{
			ListViewItem newItem = NewItem();
			newItem.Tag = new OrigamSettings("New Configuration");
			lvwConfigurations.Items.Add(newItem);
			RefreshList();
		}
		if(e.Button == btnDelete)
		{
			if(lvwConfigurations.SelectedItems.Count <= 0)
            {
                return;
            }

            ListViewItem[] items = new ListViewItem[lvwConfigurations.SelectedItems.Count];
			lvwConfigurations.SelectedItems.CopyTo(items, 0);
			foreach(ListViewItem item in items)
			{
				lvwConfigurations.Items.Remove(item);
			}
			_propertyPad.PropertyGrid.SelectedObjects = null;
		}
	
		if(e.Button == btnClone)
		{
			if(lvwConfigurations.SelectedItems.Count <= 0)
            {
                return;
            }

            foreach (ListViewItem item in lvwConfigurations.SelectedItems)
			{
				ListViewItem newItem = NewItem();
				OrigamSettings settings = (item.Tag as OrigamSettings).Clone() as OrigamSettings;
				settings.Name = ResourceUtils.GetString("CopyOf", settings.Name);
				newItem.Tag = settings;
				lvwConfigurations.Items.Add(newItem);
			}
			RefreshList();
		}
		this.IsDirty = true;
	}
	private bool _closing = false;
	private void OrigamSettingsEditor_Closed(object sender, System.EventArgs e)
	{
		_closing = true;
		_propertyPad.PropertyGrid.SelectedObjects = null;
	}
}
