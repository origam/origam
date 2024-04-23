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
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Origam.Schema.LookupModel.UI.Wizards;

/// <summary>
/// Summary description for CreateLookupFromEntityWizard.
/// </summary>
public class CreateFieldWithLookupEntityWizard : System.Windows.Forms.Form
{
	public class InitialValue
	{
		public string Code { get; set; }
		public string Name { get; set; }
		public bool IsDefault { get; set; }
	}

	private System.Windows.Forms.TextBox txtName;
	private System.Windows.Forms.Label lblName;
	private System.Windows.Forms.Button btnOK;
	private System.Windows.Forms.Button btnCancel;
	private System.Windows.Forms.Label lblCaption;
	private System.Windows.Forms.TextBox txtCaption;
	private System.Windows.Forms.CheckBox chkAllowNulls;
	private DataGridView grdInitialValues;
	private Label label1;
	private BindingList<InitialValue> _initialValues = new BindingList<InitialValue>();
	private CheckBox chkTwoColumn;
	private DataGridViewTextBoxColumn colCode;
	private DataGridViewTextBoxColumn colName;
	private DataGridViewCheckBoxColumn colDefault;
	private TextBox txtNameFieldName;
	private Label lblNameFieldName;
	private TextBox txtKeyFieldName;
	private Label lblKeyFieldName;
	private TextBox txtKeyFieldCaption;
	private Label lblKeyFieldCaption;
	private TextBox txtNameFieldCaption;
	private Label lblNameFieldCaption;

	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public CreateFieldWithLookupEntityWizard()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
		grdInitialValues.DataSource = _initialValues;
		UpdateScreen();
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
		this.lblCaption = new System.Windows.Forms.Label();
		this.btnOK = new System.Windows.Forms.Button();
		this.btnCancel = new System.Windows.Forms.Button();
		this.txtCaption = new System.Windows.Forms.TextBox();
		this.chkAllowNulls = new System.Windows.Forms.CheckBox();
		this.grdInitialValues = new System.Windows.Forms.DataGridView();
		this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colDefault = new System.Windows.Forms.DataGridViewCheckBoxColumn();
		this.label1 = new System.Windows.Forms.Label();
		this.chkTwoColumn = new System.Windows.Forms.CheckBox();
		this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.txtNameFieldName = new System.Windows.Forms.TextBox();
		this.lblNameFieldName = new System.Windows.Forms.Label();
		this.txtKeyFieldName = new System.Windows.Forms.TextBox();
		this.lblKeyFieldName = new System.Windows.Forms.Label();
		this.txtKeyFieldCaption = new System.Windows.Forms.TextBox();
		this.lblKeyFieldCaption = new System.Windows.Forms.Label();
		this.txtNameFieldCaption = new System.Windows.Forms.TextBox();
		this.lblNameFieldCaption = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)(this.grdInitialValues)).BeginInit();
		this.SuspendLayout();
		// 
		// txtName
		// 
		this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtName.Location = new System.Drawing.Point(123, 16);
		this.txtName.Name = "txtName";
		this.txtName.Size = new System.Drawing.Size(286, 20);
		this.txtName.TabIndex = 1;
		// 
		// lblName
		// 
		this.lblName.Location = new System.Drawing.Point(8, 19);
		this.lblName.Name = "lblName";
		this.lblName.Size = new System.Drawing.Size(116, 20);
		this.lblName.TabIndex = 0;
		this.lblName.Text = "Lookup Entity Name";
		// 
		// lblCaption
		// 
		this.lblCaption.Location = new System.Drawing.Point(8, 43);
		this.lblCaption.Name = "lblCaption";
		this.lblCaption.Size = new System.Drawing.Size(116, 20);
		this.lblCaption.TabIndex = 2;
		this.lblCaption.Text = "Caption";
		// 
		// btnOK
		// 
		this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
		this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.btnOK.Location = new System.Drawing.Point(211, 368);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(96, 24);
		this.btnOK.TabIndex = 16;
		this.btnOK.Text = "&OK";
		this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
		// 
		// btnCancel
		// 
		this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.btnCancel.Location = new System.Drawing.Point(313, 368);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(96, 24);
		this.btnCancel.TabIndex = 17;
		this.btnCancel.Text = "&Cancel";
		this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
		// 
		// txtCaption
		// 
		this.txtCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtCaption.Location = new System.Drawing.Point(123, 40);
		this.txtCaption.Name = "txtCaption";
		this.txtCaption.Size = new System.Drawing.Size(286, 20);
		this.txtCaption.TabIndex = 3;
		// 
		// chkAllowNulls
		// 
		this.chkAllowNulls.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.chkAllowNulls.Location = new System.Drawing.Point(8, 66);
		this.chkAllowNulls.Name = "chkAllowNulls";
		this.chkAllowNulls.Size = new System.Drawing.Size(129, 24);
		this.chkAllowNulls.TabIndex = 4;
		this.chkAllowNulls.Text = "Allow Nulls";
		// 
		// grdInitialValues
		// 
		this.grdInitialValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.grdInitialValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.grdInitialValues.AutoGenerateColumns = false;
		this.grdInitialValues.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.colName,
			this.colDefault});
		this.grdInitialValues.Location = new System.Drawing.Point(8, 171);
		this.grdInitialValues.Name = "grdInitialValues";
		this.grdInitialValues.Size = new System.Drawing.Size(401, 171);
		this.grdInitialValues.TabIndex = 15;
		// 
		// colName
		// 
		this.colName.DataPropertyName = "Name";
		this.colName.HeaderText = "Name";
		this.colName.Name = "colName";
		// 
		// colDefault
		// 
		this.colDefault.DataPropertyName = "IsDefault";
		this.colDefault.HeaderText = "Default";
		this.colDefault.Name = "colDefault";
		// 
		// label1
		// 
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(8, 153);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(68, 13);
		this.label1.TabIndex = 14;
		this.label1.Text = "Initial values:";
		// 
		// chkTwoColumn
		// 
		this.chkTwoColumn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
		this.chkTwoColumn.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.chkTwoColumn.Location = new System.Drawing.Point(253, 66);
		this.chkTwoColumn.Name = "chkTwoColumn";
		this.chkTwoColumn.Size = new System.Drawing.Size(156, 24);
		this.chkTwoColumn.TabIndex = 5;
		this.chkTwoColumn.Text = "Two-Column (Key, Name)";
		this.chkTwoColumn.CheckedChanged += new System.EventHandler(this.chkTwoColumn_CheckedChanged);
		// 
		// colCode
		// 
		this.colCode.DataPropertyName = "Code";
		this.colCode.HeaderText = "Code";
		this.colCode.Name = "colCode";
		// 
		// txtNameFieldName
		// 
		this.txtNameFieldName.Location = new System.Drawing.Point(123, 95);
		this.txtNameFieldName.Name = "txtNameFieldName";
		this.txtNameFieldName.Size = new System.Drawing.Size(101, 20);
		this.txtNameFieldName.TabIndex = 7;
		// 
		// lblNameFieldName
		// 
		this.lblNameFieldName.Location = new System.Drawing.Point(8, 98);
		this.lblNameFieldName.Name = "lblNameFieldName";
		this.lblNameFieldName.Size = new System.Drawing.Size(116, 20);
		this.lblNameFieldName.TabIndex = 6;
		this.lblNameFieldName.Text = "\"Name\" Field Name";
		// 
		// txtKeyFieldName
		// 
		this.txtKeyFieldName.Location = new System.Drawing.Point(123, 121);
		this.txtKeyFieldName.Name = "txtKeyFieldName";
		this.txtKeyFieldName.Size = new System.Drawing.Size(101, 20);
		this.txtKeyFieldName.TabIndex = 11;
		this.txtKeyFieldName.Visible = false;
		// 
		// lblKeyFieldName
		// 
		this.lblKeyFieldName.Location = new System.Drawing.Point(8, 124);
		this.lblKeyFieldName.Name = "lblKeyFieldName";
		this.lblKeyFieldName.Size = new System.Drawing.Size(116, 20);
		this.lblKeyFieldName.TabIndex = 10;
		this.lblKeyFieldName.Text = "\"Key\" Field Name";
		this.lblKeyFieldName.Visible = false;
		// 
		// txtKeyFieldCaption
		// 
		this.txtKeyFieldCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtKeyFieldCaption.Location = new System.Drawing.Point(277, 121);
		this.txtKeyFieldCaption.Name = "txtKeyFieldCaption";
		this.txtKeyFieldCaption.Size = new System.Drawing.Size(132, 20);
		this.txtKeyFieldCaption.TabIndex = 13;
		this.txtKeyFieldCaption.Visible = false;
		this.txtKeyFieldCaption.TextChanged += new System.EventHandler(this.txtKeyFieldCaption_TextChanged);
		// 
		// lblKeyFieldCaption
		// 
		this.lblKeyFieldCaption.Location = new System.Drawing.Point(230, 124);
		this.lblKeyFieldCaption.Name = "lblKeyFieldCaption";
		this.lblKeyFieldCaption.Size = new System.Drawing.Size(51, 20);
		this.lblKeyFieldCaption.TabIndex = 12;
		this.lblKeyFieldCaption.Text = "Caption";
		this.lblKeyFieldCaption.Visible = false;
		// 
		// txtNameFieldCaption
		// 
		this.txtNameFieldCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtNameFieldCaption.Location = new System.Drawing.Point(277, 95);
		this.txtNameFieldCaption.Name = "txtNameFieldCaption";
		this.txtNameFieldCaption.Size = new System.Drawing.Size(132, 20);
		this.txtNameFieldCaption.TabIndex = 9;
		this.txtNameFieldCaption.TextChanged += new System.EventHandler(this.txtNameFieldCaption_TextChanged);
		// 
		// lblNameFieldCaption
		// 
		this.lblNameFieldCaption.Location = new System.Drawing.Point(230, 98);
		this.lblNameFieldCaption.Name = "lblNameFieldCaption";
		this.lblNameFieldCaption.Size = new System.Drawing.Size(51, 20);
		this.lblNameFieldCaption.TabIndex = 8;
		this.lblNameFieldCaption.Text = "Caption";
		// 
		// CreateFieldWithLookupEntityWizard
		// 
		this.AcceptButton = this.btnOK;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.CancelButton = this.btnCancel;
		this.ClientSize = new System.Drawing.Size(424, 404);
		this.ControlBox = false;
		this.Controls.Add(this.txtKeyFieldCaption);
		this.Controls.Add(this.lblKeyFieldCaption);
		this.Controls.Add(this.txtNameFieldCaption);
		this.Controls.Add(this.lblNameFieldCaption);
		this.Controls.Add(this.txtKeyFieldName);
		this.Controls.Add(this.lblKeyFieldName);
		this.Controls.Add(this.txtNameFieldName);
		this.Controls.Add(this.lblNameFieldName);
		this.Controls.Add(this.chkTwoColumn);
		this.Controls.Add(this.label1);
		this.Controls.Add(this.grdInitialValues);
		this.Controls.Add(this.chkAllowNulls);
		this.Controls.Add(this.txtCaption);
		this.Controls.Add(this.txtName);
		this.Controls.Add(this.btnCancel);
		this.Controls.Add(this.btnOK);
		this.Controls.Add(this.lblCaption);
		this.Controls.Add(this.lblName);
		this.Name = "CreateFieldWithLookupEntityWizard";
		this.ShowInTaskbar = false;
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Create Lookup Wizard";
		((System.ComponentModel.ISupportInitialize)(this.grdInitialValues)).EndInit();
		this.ResumeLayout(false);
		this.PerformLayout();

	}
	#endregion

	#region Event Handlers
	private void btnOK_Click(object sender, System.EventArgs e)
	{
		if(LookupName == "" 
		   || (txtCaption.Visible && LookupCaption == ""))
		{
			MessageBox.Show(ResourceUtils.GetString("EnterAllInfo"), 
				ResourceUtils.GetString("LookupWiz"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		if (!AllowNulls && InitialValues.Count > 0 && DefaultInitialValue == null)
		{
			if (MessageBox.Show(ResourceUtils.GetString("DefaultValueNotSet"),
				    ResourceUtils.GetString("LookupWiz"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
			    == DialogResult.Cancel)
			{
				return;
			}
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
	public string LookupName
	{
		get
		{
			return txtName.Text;
		}
	}

	public string LookupCaption
	{
		get
		{
			return txtCaption.Text;
		}
	}

	public string NameFieldName
	{
		get
		{
			return txtNameFieldName.Text;
		}
		set
		{
			txtNameFieldName.Text = value;
		}
	}

	public string NameFieldCaption
	{
		get
		{
			return txtNameFieldCaption.Text;
		}
		set
		{
			txtNameFieldCaption.Text = value;
		}
	}

	public string KeyFieldName
	{
		get
		{
			return txtKeyFieldName.Text;
		}
		set
		{
			txtKeyFieldName.Text = value;
		}
	}

	public string KeyFieldCaption
	{
		get
		{
			return txtKeyFieldCaption.Text;
		}
		set
		{
			txtKeyFieldCaption.Text = value;
		}
	}

	public bool AllowNulls
	{
		get
		{
			return chkAllowNulls.Checked;
		}
		set
		{
			chkAllowNulls.Checked = value;
		}
	}

	public bool TwoColumns
	{
		get
		{
			return chkTwoColumn.Checked;
		}
	}

	public bool ForceTwoColumns
	{
		get
		{
			return !chkTwoColumn.Enabled;
		}
		set
		{
			chkTwoColumn.Checked = true;
			chkTwoColumn.Visible = false;
			chkAllowNulls.Visible = false;
			lblCaption.Visible = false;
			txtCaption.Visible = false;
		}
	}

	public IList<InitialValue> InitialValues
	{
		get
		{
			return _initialValues;
		}
	}

	public InitialValue DefaultInitialValue
	{
		get
		{
			foreach (var item in InitialValues)
			{
				if (item.IsDefault)
				{
					return item;
				}
			}
			return null;
		}
	}
	#endregion

	#region Private Methods
	private void SetUpForm()
	{
	}
	#endregion

	private void chkTwoColumn_CheckedChanged(object sender, EventArgs e)
	{
		UpdateScreen();
	}

	private void UpdateScreen()
	{
		lblKeyFieldName.Visible = lblKeyFieldCaption.Visible
			= txtKeyFieldName.Visible = txtKeyFieldCaption.Visible
				= chkTwoColumn.Checked;
		grdInitialValues.Columns.Clear();
		if (chkTwoColumn.Checked)
		{
			grdInitialValues.Columns.AddRange(new DataGridViewColumn[] {
				colDefault,
				colCode,
				colName
			});
			colCode.DisplayIndex = 0;
			colName.DisplayIndex = 1;
			colDefault.DisplayIndex = 2;
		}
		else
		{
			grdInitialValues.Columns.AddRange(new DataGridViewColumn[] {
				colDefault,
				colName
			});
			colName.DisplayIndex = 0;
			colDefault.DisplayIndex = 1;
		}
	}

	private void txtNameFieldCaption_TextChanged(object sender, EventArgs e)
	{
		colName.HeaderText = txtNameFieldCaption.Text;
	}

	private void txtKeyFieldCaption_TextChanged(object sender, EventArgs e)
	{
		colCode.HeaderText = txtKeyFieldCaption.Text;
	}
}