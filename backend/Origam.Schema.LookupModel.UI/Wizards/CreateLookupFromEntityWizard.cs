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

using Origam.Schema.EntityModel;

namespace Origam.Schema.LookupModel.UI.Wizards;
/// <summary>
/// Summary description for CreateLookupFromEntityWizard.
/// </summary>
public class CreateLookupFromEntityWizard : System.Windows.Forms.Form
{
	private System.Windows.Forms.TextBox txtName;
	private System.Windows.Forms.Label lblName;
	private System.Windows.Forms.ComboBox cboDisplayField;
	private System.Windows.Forms.Label lblDisplayField;
	private System.Windows.Forms.ComboBox cboListFilter;
	private System.Windows.Forms.Label lblListFilter;
	private System.Windows.Forms.Button btnOK;
	private System.Windows.Forms.Button btnCancel;
	private System.Windows.Forms.Label lblIdFilter;
	private System.Windows.Forms.ComboBox cboIdFilter;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;
	public CreateLookupFromEntityWizard()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
		//
		// TODO: Add any constructor code after InitializeComponent call
		//
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
		this.txtName = new System.Windows.Forms.TextBox();
		this.lblName = new System.Windows.Forms.Label();
		this.cboDisplayField = new System.Windows.Forms.ComboBox();
		this.lblDisplayField = new System.Windows.Forms.Label();
		this.cboListFilter = new System.Windows.Forms.ComboBox();
		this.lblListFilter = new System.Windows.Forms.Label();
		this.btnOK = new System.Windows.Forms.Button();
		this.btnCancel = new System.Windows.Forms.Button();
		this.lblIdFilter = new System.Windows.Forms.Label();
		this.cboIdFilter = new System.Windows.Forms.ComboBox();
		this.SuspendLayout();
		// 
		// txtName
		// 
		this.txtName.Location = new System.Drawing.Point(96, 16);
		this.txtName.Name = "txtName";
		this.txtName.Size = new System.Drawing.Size(272, 20);
		this.txtName.TabIndex = 0;
		this.txtName.Text = "";
		// 
		// lblName
		// 
		this.lblName.Location = new System.Drawing.Point(8, 16);
		this.lblName.Name = "lblName";
		this.lblName.Size = new System.Drawing.Size(72, 16);
		this.lblName.TabIndex = 1;
		this.lblName.Text = ResourceUtils.GetString("NameLabel");
		// 
		// cboDisplayField
		// 
		this.cboDisplayField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cboDisplayField.Location = new System.Drawing.Point(96, 40);
		this.cboDisplayField.Name = "cboDisplayField";
		this.cboDisplayField.Size = new System.Drawing.Size(272, 21);
		this.cboDisplayField.Sorted = true;
		this.cboDisplayField.TabIndex = 2;
		this.cboDisplayField.SelectedIndexChanged += new System.EventHandler(this.cboDisplayField_SelectedIndexChanged);
		// 
		// lblDisplayField
		// 
		this.lblDisplayField.Location = new System.Drawing.Point(8, 40);
		this.lblDisplayField.Name = "lblDisplayField";
		this.lblDisplayField.Size = new System.Drawing.Size(72, 16);
		this.lblDisplayField.TabIndex = 3;
		this.lblDisplayField.Text = ResourceUtils.GetString("DisplayFieldLabel");
		// 
		// cboListFilter
		// 
		this.cboListFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cboListFilter.Location = new System.Drawing.Point(96, 64);
		this.cboListFilter.Name = "cboListFilter";
		this.cboListFilter.Size = new System.Drawing.Size(272, 21);
		this.cboListFilter.Sorted = true;
		this.cboListFilter.TabIndex = 4;
		// 
		// lblListFilter
		// 
		this.lblListFilter.Location = new System.Drawing.Point(8, 64);
		this.lblListFilter.Name = "lblListFilter";
		this.lblListFilter.Size = new System.Drawing.Size(72, 16);
		this.lblListFilter.TabIndex = 5;
		this.lblListFilter.Text = ResourceUtils.GetString("ListFilterLabel");
		// 
		// btnOK
		// 
		this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.btnOK.Location = new System.Drawing.Point(168, 128);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(96, 24);
		this.btnOK.TabIndex = 8;
		this.btnOK.Text = ResourceUtils.GetString("ButtonOK");
		this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
		// 
		// btnCancel
		// 
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.btnCancel.Location = new System.Drawing.Point(272, 128);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(96, 24);
		this.btnCancel.TabIndex = 9;
		this.btnCancel.Text = ResourceUtils.GetString("ButtonCancel");
		this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
		// 
		// lblIdFilter
		// 
		this.lblIdFilter.Location = new System.Drawing.Point(8, 88);
		this.lblIdFilter.Name = "lblIdFilter";
		this.lblIdFilter.Size = new System.Drawing.Size(72, 16);
		this.lblIdFilter.TabIndex = 7;
		this.lblIdFilter.Text = ResourceUtils.GetString("IdFilterLabel");
		// 
		// cboIdFilter
		// 
		this.cboIdFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cboIdFilter.Location = new System.Drawing.Point(96, 88);
		this.cboIdFilter.Name = "cboIdFilter";
		this.cboIdFilter.Size = new System.Drawing.Size(272, 21);
		this.cboIdFilter.Sorted = true;
		this.cboIdFilter.TabIndex = 6;
		// 
		// CreateLookupFromEntityWizard
		// 
		this.AcceptButton = this.btnOK;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.CancelButton = this.btnCancel;
		this.ClientSize = new System.Drawing.Size(378, 167);
		this.ControlBox = false;
		this.Controls.Add(this.lblIdFilter);
		this.Controls.Add(this.cboIdFilter);
		this.Controls.Add(this.btnCancel);
		this.Controls.Add(this.btnOK);
		this.Controls.Add(this.lblListFilter);
		this.Controls.Add(this.cboListFilter);
		this.Controls.Add(this.lblDisplayField);
		this.Controls.Add(this.cboDisplayField);
		this.Controls.Add(this.lblName);
		this.Controls.Add(this.txtName);
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.Name = "CreateLookupFromEntityWizard";
		this.ShowInTaskbar = false;
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = ResourceUtils.GetString("CreateLookupWiz");
		this.ResumeLayout(false);
	}
	#endregion
	#region Event Handlers
	private void btnOK_Click(object sender, System.EventArgs e)
	{
		if(this.LookupName == "" 
			| this.Entity == null 
			| this.NameColumn == null 
			| this.IdColumn == null
			| this.IdFilter == null)
		{
			MessageBox.Show(ResourceUtils.GetString("EnterAllInfo"), ResourceUtils.GetString("LookupWiz"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		this.DialogResult = DialogResult.OK;
		this.Close();
	}
	private void btnCancel_Click(object sender, System.EventArgs e)
	{
		this.Close();
	}
	#endregion
	#region Public Properties
	private IDataEntity _entity;
	public IDataEntity Entity
	{
		get
		{
			return _entity;
		}
		set
		{
			_entity = value;
			SetUpForm();
		}
	}
	public string LookupName
	{
		get
		{
			return txtName.Text;
		}
	}
	public IDataEntityColumn NameColumn
	{
		get
		{
			return this.cboDisplayField.SelectedItem as IDataEntityColumn;
		}
	}
	private IDataEntityColumn _idColumn = null;
	public IDataEntityColumn IdColumn
	{
		get
		{
			return _idColumn;
		}
	}
	public EntityFilter IdFilter
	{
		get
		{
			return cboIdFilter.SelectedItem as EntityFilter;
		}
	}
	
	public EntityFilter ListFilter
	{
		get
		{
			return cboListFilter.SelectedItem as EntityFilter;
		}
	}
	#endregion
	#region Private Methods
	private void SetUpForm()
	{
		cboIdFilter.Items.Clear();
		cboListFilter.Items.Clear();
		cboDisplayField.Items.Clear();
		IDataEntityColumn nameColumn = null;
		if(this.Entity == null) return;
		txtName.Text = this.Entity.Name;
		EntityFilter idFilter = null;
		foreach(var filter in Entity.ChildItemsByType<EntityFilter>(EntityFilter.CategoryConst))
		{
			cboListFilter.Items.Add(filter);
			cboIdFilter.Items.Add(filter);
			if(filter.Name == "GetId") idFilter = filter;
		}
		if(idFilter != null) cboIdFilter.SelectedItem = idFilter;
		foreach(IDataEntityColumn column in this.Entity.EntityColumns)
		{
		    if(string.IsNullOrEmpty(column.ToString())) continue;
			if(column.Name == "Name") nameColumn = column;
			if(column.IsPrimaryKey && !column.ExcludeFromAllFields) _idColumn = column;
			cboDisplayField.Items.Add(column);
		}
		cboDisplayField.SelectedItem = nameColumn;
		if(_idColumn == null) throw new Exception("Entity has no primary key defined. Cannot create lookup.");
	}
	#endregion
	private void cboDisplayField_SelectedIndexChanged(object sender, System.EventArgs e)
	{
		if(this.NameColumn.Name != "Name")
		{
			this.txtName.Text = this.Entity.Name + "_" + this.NameColumn.Name;
		}
	}
}
