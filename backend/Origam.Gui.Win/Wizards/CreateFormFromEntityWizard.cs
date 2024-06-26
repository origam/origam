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
using System.Collections;
using System.Windows.Forms;
using Origam.Schema.EntityModel;

namespace Origam.Gui.Win.Wizards;
/// <summary>
/// Summary description for CreateLookupFromEntityWizard.
/// </summary>
public class CreateFormFromEntityWizard : System.Windows.Forms.Form
{
	private System.Windows.Forms.Button btnOK;
	private System.Windows.Forms.Button btnCancel;
	private System.Windows.Forms.CheckedListBox lstFields;
	private System.Windows.Forms.TextBox txtRole;
	private System.Windows.Forms.Label lblRole;
	// tells whether to generate checklist only for text columns
	private Boolean _textColumnsOnly = false;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;
	public CreateFormFromEntityWizard(Boolean textColumnsOnly)
	{
		_textColumnsOnly = textColumnsOnly;
		InitializeComponent();
	}
	public CreateFormFromEntityWizard()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
	    lstFields.CheckOnClick = true;
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
		this.btnOK = new System.Windows.Forms.Button();
		this.btnCancel = new System.Windows.Forms.Button();
		this.lstFields = new System.Windows.Forms.CheckedListBox();
		this.lblRole = new System.Windows.Forms.Label();
		this.txtRole = new System.Windows.Forms.TextBox();
		this.SuspendLayout();
		// 
		// btnOK
		// 
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.btnOK.Location = new System.Drawing.Point(16, 264);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(96, 24);
		this.btnOK.TabIndex = 2;
		this.btnOK.Text = strings.Ok_Button;
		this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
		// 
		// btnCancel
		// 
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.btnCancel.Location = new System.Drawing.Point(120, 264);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(96, 24);
		this.btnCancel.TabIndex = 3;
		this.btnCancel.Text = strings.Cancel_Button;
		this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
		// 
		// lstFields
		// 
		this.lstFields.Location = new System.Drawing.Point(8, 8);
		this.lstFields.Name = "lstFields";
		this.lstFields.Size = new System.Drawing.Size(216, 214);
		this.lstFields.Sorted = true;
		this.lstFields.TabIndex = 0;
		// 
		// lblRole
		// 
		this.lblRole.Location = new System.Drawing.Point(16, 232);
		this.lblRole.Name = "lblRole";
		this.lblRole.Size = new System.Drawing.Size(40, 16);
		this.lblRole.TabIndex = 10;
		this.lblRole.Text = strings.Role_Label;
		this.lblRole.Visible = false;
		// 
		// txtRole
		// 
		this.txtRole.Location = new System.Drawing.Point(56, 232);
		this.txtRole.Name = "txtRole";
		this.txtRole.Size = new System.Drawing.Size(160, 20);
		this.txtRole.TabIndex = 1;
		this.txtRole.Text = "";
		this.txtRole.Visible = false;
		// 
		// CreateFormFromEntityWizard
		// 
		this.AcceptButton = this.btnOK;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.CancelButton = this.btnCancel;
		this.ClientSize = new System.Drawing.Size(230, 302);
		this.ControlBox = false;
		this.Controls.Add(this.txtRole);
		this.Controls.Add(this.lblRole);
		this.Controls.Add(this.lstFields);
		this.Controls.Add(this.btnCancel);
		this.Controls.Add(this.btnOK);
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.Name = "CreateFormFromEntityWizard";
		this.ShowInTaskbar = false;
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.ResumeLayout(false);
	}
	#endregion
	#region Event Handlers
	private void btnOK_Click(object sender, System.EventArgs e)
	{
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
	public string Role
	{
		get
		{
			return txtRole.Text;
		}
		set
		{
			txtRole.Text = value;
		}
	}
	public Hashtable SelectedFieldNames
	{
		get
		{
			Hashtable result = new Hashtable();
			foreach(IDataEntityColumn column in lstFields.CheckedItems)
			{
				result.Add(column.Name, null);
			}
			return result;
		}
	}
	public ICollection SelectedFields
	{
		get
		{
			return lstFields.CheckedItems;
		}
	}
	private bool _isRoleVisible = false;
	public bool IsRoleVisible
	{
		get
		{
			return _isRoleVisible;
		}
		set
		{
			_isRoleVisible = value;
			lblRole.Visible = _isRoleVisible;
			txtRole.Visible = _isRoleVisible;
		}
	}
	#endregion
	#region Private Methods
	private void SetUpForm()
	{
		lstFields.Items.Clear();
		if(this.Entity == null) return;
		foreach(IDataEntityColumn column in this.Entity.EntityColumns)
		{
		    if (string.IsNullOrEmpty(column.ToString())) continue;
			if (!this._textColumnsOnly 
			    || (column.DataType == Origam.Schema.OrigamDataType.String
				|| column.DataType == Origam.Schema.OrigamDataType.Memo))
			{
				lstFields.Items.Add(column);
			}
		}
	}
	#endregion
}
