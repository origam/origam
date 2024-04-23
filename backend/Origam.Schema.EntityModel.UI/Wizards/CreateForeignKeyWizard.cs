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

namespace Origam.Schema.EntityModel.UI.Wizards;

/// <summary>
/// Summary description for CreateForeignKeyWizard.
/// </summary>
public class CreateForeignKeyWizard : System.Windows.Forms.Form
{
	private System.Windows.Forms.Button btnCancel;
	private System.Windows.Forms.Button btnOK;
	private System.Windows.Forms.Label lblEntity;
	private System.Windows.Forms.ComboBox cboEntity;
	private System.Windows.Forms.Label lblName;
	private System.Windows.Forms.TextBox txtName;
	private System.Windows.Forms.Label lblField;
	private System.Windows.Forms.ComboBox cboField;
	private System.Windows.Forms.CheckBox chkAllowNulls;
	private System.Windows.Forms.Label label1;
	private System.Windows.Forms.ComboBox cboLookup;
	private System.Windows.Forms.TextBox txtCaption;
	private System.Windows.Forms.Label lblCaption;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public CreateForeignKeyWizard()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
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
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnOK = new System.Windows.Forms.Button();
		this.lblEntity = new System.Windows.Forms.Label();
		this.cboEntity = new System.Windows.Forms.ComboBox();
		this.lblName = new System.Windows.Forms.Label();
		this.txtName = new System.Windows.Forms.TextBox();
		this.lblField = new System.Windows.Forms.Label();
		this.cboField = new System.Windows.Forms.ComboBox();
		this.chkAllowNulls = new System.Windows.Forms.CheckBox();
		this.label1 = new System.Windows.Forms.Label();
		this.cboLookup = new System.Windows.Forms.ComboBox();
		this.txtCaption = new System.Windows.Forms.TextBox();
		this.lblCaption = new System.Windows.Forms.Label();
		this.SuspendLayout();
		// 
		// btnCancel
		// 
		this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.btnCancel.Location = new System.Drawing.Point(880, 338);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(96, 24);
		this.btnCancel.TabIndex = 12;
		this.btnCancel.Text = "&Cancel";
		this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
		// 
		// btnOK
		// 
		this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
		this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.btnOK.Location = new System.Drawing.Point(772, 338);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(96, 24);
		this.btnOK.TabIndex = 11;
		this.btnOK.Text = "&OK";
		this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
		// 
		// lblEntity
		// 
		this.lblEntity.Location = new System.Drawing.Point(13, 12);
		this.lblEntity.Name = "lblEntity";
		this.lblEntity.Size = new System.Drawing.Size(96, 16);
		this.lblEntity.TabIndex = 0;
		this.lblEntity.Text = "Foreign Entity:";
		// 
		// cboEntity
		// 
		this.cboEntity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.cboEntity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cboEntity.Location = new System.Drawing.Point(125, 12);
		this.cboEntity.Name = "cboEntity";
		this.cboEntity.Size = new System.Drawing.Size(851, 21);
		this.cboEntity.Sorted = true;
		this.cboEntity.TabIndex = 1;
		this.cboEntity.SelectedValueChanged += new System.EventHandler(this.cboEntity_SelectedValueChanged);
		// 
		// lblName
		// 
		this.lblName.Location = new System.Drawing.Point(13, 108);
		this.lblName.Name = "lblName";
		this.lblName.Size = new System.Drawing.Size(104, 17);
		this.lblName.TabIndex = 7;
		this.lblName.Text = "FK Field Name:";
		// 
		// txtName
		// 
		this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtName.Location = new System.Drawing.Point(125, 108);
		this.txtName.Name = "txtName";
		this.txtName.Size = new System.Drawing.Size(851, 20);
		this.txtName.TabIndex = 8;
		// 
		// lblField
		// 
		this.lblField.Location = new System.Drawing.Point(13, 36);
		this.lblField.Name = "lblField";
		this.lblField.Size = new System.Drawing.Size(96, 17);
		this.lblField.TabIndex = 2;
		this.lblField.Text = "Foreign Field:";
		// 
		// cboField
		// 
		this.cboField.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.cboField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cboField.Location = new System.Drawing.Point(125, 36);
		this.cboField.Name = "cboField";
		this.cboField.Size = new System.Drawing.Size(851, 21);
		this.cboField.Sorted = true;
		this.cboField.TabIndex = 3;
		this.cboField.SelectedIndexChanged += new System.EventHandler(this.cboField_SelectedIndexChanged);
		// 
		// chkAllowNulls
		// 
		this.chkAllowNulls.Location = new System.Drawing.Point(13, 85);
		this.chkAllowNulls.Name = "chkAllowNulls";
		this.chkAllowNulls.Size = new System.Drawing.Size(136, 15);
		this.chkAllowNulls.TabIndex = 6;
		this.chkAllowNulls.Text = "Allow Nulls";
		// 
		// label1
		// 
		this.label1.Location = new System.Drawing.Point(13, 60);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(96, 16);
		this.label1.TabIndex = 4;
		this.label1.Text = "Lookup:";
		// 
		// cboLookup
		// 
		this.cboLookup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.cboLookup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cboLookup.Location = new System.Drawing.Point(125, 60);
		this.cboLookup.Name = "cboLookup";
		this.cboLookup.Size = new System.Drawing.Size(851, 21);
		this.cboLookup.Sorted = true;
		this.cboLookup.TabIndex = 5;
		// 
		// txtCaption
		// 
		this.txtCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtCaption.Location = new System.Drawing.Point(125, 132);
		this.txtCaption.Name = "txtCaption";
		this.txtCaption.Size = new System.Drawing.Size(851, 20);
		this.txtCaption.TabIndex = 10;
		// 
		// lblCaption
		// 
		this.lblCaption.Location = new System.Drawing.Point(13, 132);
		this.lblCaption.Name = "lblCaption";
		this.lblCaption.Size = new System.Drawing.Size(104, 16);
		this.lblCaption.TabIndex = 9;
		this.lblCaption.Text = "Caption:";
		// 
		// CreateForeignKeyWizard
		// 
		this.AcceptButton = this.btnOK;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.CancelButton = this.btnCancel;
		this.ClientSize = new System.Drawing.Size(992, 374);
		this.ControlBox = false;
		this.Controls.Add(this.lblCaption);
		this.Controls.Add(this.txtCaption);
		this.Controls.Add(this.label1);
		this.Controls.Add(this.cboLookup);
		this.Controls.Add(this.chkAllowNulls);
		this.Controls.Add(this.lblField);
		this.Controls.Add(this.cboField);
		this.Controls.Add(this.lblEntity);
		this.Controls.Add(this.cboEntity);
		this.Controls.Add(this.lblName);
		this.Controls.Add(this.txtName);
		this.Controls.Add(this.btnCancel);
		this.Controls.Add(this.btnOK);
		this.Name = "CreateForeignKeyWizard";
		this.ShowInTaskbar = false;
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Create Foreign Key Wizard";
		this.ResumeLayout(false);
		this.PerformLayout();

	}
	#endregion

	#region Public Properties
	private IDataEntity _masterEntity;
	public IDataEntity MasterEntity
	{
		get
		{
			return _masterEntity;
		}
		set
		{
			_masterEntity = value;
			SetupForm();
		}
	}

	public string ForeignKeyName
	{
		get
		{
			return this.txtName.Text;
		}
	}

	public string Caption
	{
		get
		{
			return this.txtCaption.Text;
		}
	}

	public IDataEntity ForeignEntity
	{
		get
		{
			return cboEntity.SelectedItem as IDataEntity;
		}
	}

	public IDataEntityColumn ForeignField
	{
		get
		{
			return cboField.SelectedItem as IDataEntityColumn;
		}
	}
		
	public IDataLookup Lookup
	{
		get
		{
			return cboLookup.SelectedItem as IDataLookup;
		}
	}

	public bool AllowNulls
	{
		get
		{
			return this.chkAllowNulls.Checked;
		}
	}
	#endregion

	#region Private Methods
	private void SetupForm()
	{
		this.txtName.Text = "";

		cboEntity.Items.Clear();
		cboLookup.Items.Clear();
		cboField.Items.Clear();
		chkAllowNulls.Checked = true;

		try
		{
			cboEntity.BeginUpdate();
			cboLookup.BeginUpdate();

			foreach(IDataEntity entity in this.MasterEntity.RootProvider.ChildItems)
			{
				cboEntity.Items.Add(entity);
			}

			Workbench.Services.SchemaService schema = Workbench.Services.ServiceManager.Services.GetService(typeof(Workbench.Services.SchemaService)) as Workbench.Services.SchemaService;
			IDataLookupSchemaItemProvider lookups = schema.GetProvider(typeof(IDataLookupSchemaItemProvider)) as IDataLookupSchemaItemProvider;

			foreach(object lookup in lookups.ChildItems)
			{
				cboLookup.Items.Add(lookup);
			}
		}
		finally
		{
			cboEntity.EndUpdate();
			cboLookup.EndUpdate();
		}
	}
	#endregion

	private void btnOK_Click(object sender, System.EventArgs e)
	{
		if(this.ForeignEntity == null)
		{
			MessageBox.Show(ResourceUtils.GetString("SelectForeignEntity"), ResourceUtils.GetString("ForeignKeyWiz"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		if(this.ForeignField == null)
		{
			MessageBox.Show(ResourceUtils.GetString("SelectForeignField"), ResourceUtils.GetString("ForeignKeyWiz"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		if(txtName.Text == "") 
		{
			MessageBox.Show(ResourceUtils.GetString("EnterKeyName"), ResourceUtils.GetString("ForeignKeyWiz"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}

		this.DialogResult = DialogResult.OK;

		this.Close();
	}

	private void btnCancel_Click(object sender, System.EventArgs e)
	{
		this.Close();
	}

	private void cboField_SelectedIndexChanged(object sender, System.EventArgs e)
	{
		if(this.ForeignEntity != null && this.ForeignField != null)
		{
			txtName.Text = "ref" + this.ForeignEntity.Name + this.ForeignField.Name;
		}
	}

	private void cboEntity_SelectedValueChanged(object sender, System.EventArgs e)
	{
		cboField.Items.Clear();

		try
		{
			cboField.BeginUpdate();

			if(this.ForeignEntity != null)
			{
				foreach(IDataEntityColumn column in this.ForeignEntity.EntityColumns)
				{
					cboField.Items.Add(column);
				}
			}
		}
		finally
		{
			cboField.EndUpdate();
		}
	}
}