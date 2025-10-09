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

namespace Origam.Gui.Win.Wizards;
/// <summary>
/// Summary description for CreateLookupFromEntityWizard.
/// </summary>
public class CreateMenuFromFormWizard : System.Windows.Forms.Form
{
	private System.Windows.Forms.Button btnOK;
	private System.Windows.Forms.Button btnCancel;
	private System.Windows.Forms.Label label1;
	private System.Windows.Forms.TextBox txtRole;
	private System.Windows.Forms.Label label2;
	private System.Windows.Forms.TextBox txtCaption;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;
	public CreateMenuFromFormWizard()
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
		this.btnOK = new System.Windows.Forms.Button();
		this.btnCancel = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.txtRole = new System.Windows.Forms.TextBox();
		this.txtCaption = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.SuspendLayout();
		// 
		// btnOK
		// 
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.btnOK.Location = new System.Drawing.Point(16, 64);
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
		this.btnCancel.Location = new System.Drawing.Point(120, 64);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(96, 24);
		this.btnCancel.TabIndex = 3;
		this.btnCancel.Text = strings.Cancel_Button;
		this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
		// 
		// label1
		// 
		this.label1.Location = new System.Drawing.Point(8, 32);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(40, 16);
		this.label1.TabIndex = 10;
		this.label1.Text = strings.Role_Label;
		// 
		// txtRole
		// 
		this.txtRole.Location = new System.Drawing.Point(64, 32);
		this.txtRole.Name = "txtRole";
		this.txtRole.Size = new System.Drawing.Size(160, 20);
		this.txtRole.TabIndex = 1;
		this.txtRole.Text = "";
		// 
		// txtCaption
		// 
		this.txtCaption.Location = new System.Drawing.Point(64, 8);
		this.txtCaption.Name = "txtCaption";
		this.txtCaption.Size = new System.Drawing.Size(160, 20);
		this.txtCaption.TabIndex = 0;
		this.txtCaption.Text = "";
		// 
		// label2
		// 
		this.label2.Location = new System.Drawing.Point(8, 8);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(48, 16);
		this.label2.TabIndex = 12;
		this.label2.Text = strings.Caption_Label;
		// 
		// CreateFormFromEntityWizard
		// 
		this.AcceptButton = this.btnOK;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.CancelButton = this.btnCancel;
		this.ClientSize = new System.Drawing.Size(238, 102);
		this.ControlBox = false;
		this.Controls.Add(this.txtCaption);
		this.Controls.Add(this.label2);
		this.Controls.Add(this.txtRole);
		this.Controls.Add(this.label1);
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
	public string Caption
	{
		get
		{
			return txtCaption.Text;
		}
		set
		{
			txtCaption.Text = value;
		}
	}
	#endregion
}
