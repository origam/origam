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

using Origam.UI;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Editors;
using Origam.Windows.Editor;
using static Origam.DA.Common.Enums;

namespace Origam.Schema.DeploymentModel;

/// <summary>
/// Summary description for ServiceScriptCommandEditor.
/// </summary>
public class ServiceScriptCommandEditor : AbstractEditor
{
	private ServiceConverter _converter = new ServiceConverter();
	private bool _isLoading = false;

	private System.Windows.Forms.Label label1;
	private System.Windows.Forms.Label label2;
	private System.Windows.Forms.Label label3;
	private System.Windows.Forms.Label label4;
	private System.Windows.Forms.TextBox txtName;
	private System.Windows.Forms.TextBox txtOrder;
	private System.Windows.Forms.ComboBox cboService;
	private SqlEditor txtCommand;
	private Panel panel1;
	private Label label5;
	private ComboBox cboPlatform;

	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public ServiceScriptCommandEditor()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();

		this.BackColor = OrigamColorScheme.FormBackgroundColor;
		this.txtCommand.ContentChanged += TextArea_KeyDown;
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
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.txtName = new System.Windows.Forms.TextBox();
		this.txtOrder = new System.Windows.Forms.TextBox();
		this.cboService = new System.Windows.Forms.ComboBox();
		this.label4 = new System.Windows.Forms.Label();
		this.txtCommand = new Origam.Windows.Editor.SqlEditor();
		this.panel1 = new System.Windows.Forms.Panel();
		this.label5 = new System.Windows.Forms.Label();
		this.cboPlatform = new System.Windows.Forms.ComboBox();
		this.panel1.SuspendLayout();
		this.SuspendLayout();
		// 
		// label1
		// 
		this.label1.Location = new System.Drawing.Point(12, 12);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(72, 16);
		this.label1.TabIndex = 0;
		this.label1.Text = "Name:";
		// 
		// label2
		// 
		this.label2.Location = new System.Drawing.Point(12, 36);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(72, 16);
		this.label2.TabIndex = 1;
		this.label2.Text = "Order:";
		// 
		// label3
		// 
		this.label3.Location = new System.Drawing.Point(12, 60);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(72, 16);
		this.label3.TabIndex = 2;
		this.label3.Text = "Service:";
		// 
		// txtName
		// 
		this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtName.Location = new System.Drawing.Point(92, 12);
		this.txtName.Name = "txtName";
		this.txtName.Size = new System.Drawing.Size(618, 20);
		this.txtName.TabIndex = 3;
		this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
		// 
		// txtOrder
		// 
		this.txtOrder.Location = new System.Drawing.Point(92, 36);
		this.txtOrder.Name = "txtOrder";
		this.txtOrder.Size = new System.Drawing.Size(88, 20);
		this.txtOrder.TabIndex = 4;
		this.txtOrder.TextChanged += new System.EventHandler(this.txtOrder_TextChanged);
		// 
		// cboService
		// 
		this.cboService.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.cboService.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
		this.cboService.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
		this.cboService.Location = new System.Drawing.Point(92, 60);
		this.cboService.Name = "cboService";
		this.cboService.Size = new System.Drawing.Size(618, 21);
		this.cboService.TabIndex = 5;
		this.cboService.SelectedIndexChanged += new System.EventHandler(this.cboService_SelectedIndexChanged);
		// 
		// label4
		// 
		this.label4.Location = new System.Drawing.Point(12, 129);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(72, 16);
		this.label4.TabIndex = 7;
		this.label4.Text = "Command:";
		// 
		// txtCommand
		// 
		this.txtCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtCommand.BackColor = System.Drawing.Color.White;
		this.txtCommand.IsReadOnly = false;
		this.txtCommand.Location = new System.Drawing.Point(3, 148);
		this.txtCommand.Name = "txtCommand";
		this.txtCommand.Size = new System.Drawing.Size(710, 326);
		this.txtCommand.TabIndex = 8;
		// 
		// panel1
		// 
		this.panel1.Controls.Add(this.label5);
		this.panel1.Controls.Add(this.cboPlatform);
		this.panel1.Controls.Add(this.label1);
		this.panel1.Controls.Add(this.txtCommand);
		this.panel1.Controls.Add(this.label2);
		this.panel1.Controls.Add(this.label4);
		this.panel1.Controls.Add(this.label3);
		this.panel1.Controls.Add(this.cboService);
		this.panel1.Controls.Add(this.txtName);
		this.panel1.Controls.Add(this.txtOrder);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel1.Location = new System.Drawing.Point(0, 40);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(716, 486);
		this.panel1.TabIndex = 9;
		// 
		// label5
		// 
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(12, 96);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(45, 13);
		this.label5.TabIndex = 10;
		this.label5.Text = "Platform";
		// 
		// cboPlatform
		// 
		this.cboPlatform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cboPlatform.FormattingEnabled = true;
		this.cboPlatform.Location = new System.Drawing.Point(92, 88);
		this.cboPlatform.Name = "cboPlatform";
		this.cboPlatform.Size = new System.Drawing.Size(205, 21);
		this.cboPlatform.TabIndex = 9;
		this.cboPlatform.SelectedIndexChanged += new System.EventHandler(this.CboPlatform_SelectedIndexChanged);
		// 
		// ServiceScriptCommandEditor
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(795, 526);
		this.Controls.Add(this.panel1);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
		this.HelpButton = true;
		this.Name = "ServiceScriptCommandEditor";
		this.ShowIcon = false;
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.ContentLoaded += new System.EventHandler(this.ServiceScriptCommandEditor_ContentLoaded);
		this.Controls.SetChildIndex(this.panel1, 0);
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}
	#endregion

	private void ServiceScriptCommandEditor_ContentLoaded(object sender, System.EventArgs e)
	{
		ServiceCommandUpdateScriptActivity activity = this.ModelContent as ServiceCommandUpdateScriptActivity;

		if(this.IsViewOnly)
		{
			txtName.ReadOnly = true;
			txtOrder.ReadOnly = true;
			txtCommand.IsReadOnly = true;
			cboService.Enabled = false;
			cboPlatform.Enabled = false;
		}

		_isLoading = true;

		try
		{
			txtName.Text = activity.Name;
			txtOrder.Text = activity.ActivityOrder.ToString();
			txtCommand.Text = activity.CommandText;

			LoadServices();
			foreach (var item in Enum.GetValues(typeof(DatabaseType)))
			{
				cboPlatform.Items.Add(item);
				if((DatabaseType)item==activity.DatabaseType)
				{
					cboPlatform.SelectedItem = item;
				}
			}
                
			if(activity.Service == null)
			{
				// default service selected is DataService
				foreach (var item in cboService.Items)
				{
					if (item.ToString() == "DataService")
					{
						cboService.SelectedItem = item;
						break;
					}
				}
			}
			else
			{
				cboService.SelectedItem = activity.Service;
			}
		}
		finally
		{
			_isLoading = false;
		}
	}

	public override void SaveObject()
	{
		ServiceCommandUpdateScriptActivity activity = this.ModelContent as ServiceCommandUpdateScriptActivity;

		activity.Name = txtName.Text;
		activity.ActivityOrder = Convert.ToInt32(txtOrder.Text);
		activity.CommandText = txtCommand.Text;
			
		activity.Service = null;
		activity.DatabaseType = (DatabaseType)cboPlatform.SelectedItem;

		if(cboService.SelectedItem != null)
		{
			activity.Service = cboService.SelectedItem as IService;
		}

		base.SaveObject ();
	}

	private void LoadServices()
	{
		object currentItem = null;
			
		if(cboService.SelectedItem != null)
		{
			currentItem = _converter.ConvertFrom(cboService.SelectedItem);
		}

		try
		{
			cboService.BeginUpdate();
			cboService.Items.Clear();
			foreach(object o in _converter.GetStandardValues())
			{
				cboService.Items.Add(o);
			}
		}
		finally
		{
			cboService.EndUpdate();
		}


		if(currentItem != null)
		{
			_isLoading = true;
			cboService.SelectedItem = _converter.ConvertFrom(currentItem);
			_isLoading = false;
		}
	}

	private void cboService_SelectedIndexChanged(object sender, System.EventArgs e)
	{
		if(! _isLoading & ! this.IsViewOnly) this.IsDirty = true;
	}

	private void txtName_TextChanged(object sender, System.EventArgs e)
	{
		if(! _isLoading & ! this.IsViewOnly)
		{
			ModelContent.Name = txtName.Text;
			this.IsDirty = true;
			this.TitleName = txtName.Text;
		}
	}

	private void txtOrder_TextChanged(object sender, System.EventArgs e)
	{
		if(! _isLoading & ! this.IsViewOnly) this.IsDirty = true;
	}

	private void TextArea_KeyDown(object sender, EventArgs e)
	{
		if(! _isLoading & ! this.IsViewOnly) this.IsDirty = true;
	}

	private void CboPlatform_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!_isLoading & !this.IsViewOnly) this.IsDirty = true;
	}
}