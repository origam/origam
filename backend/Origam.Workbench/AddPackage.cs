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

using Origam.UI;

namespace Origam.Workbench;

/// <summary>
/// Summary description for AddPackage.
/// </summary>
public class AddPackage : System.Windows.Forms.Form
{
	private System.Windows.Forms.Label label1;
	private System.Windows.Forms.TextBox txtName;
	private System.Windows.Forms.Button btnOK;
	private System.Windows.Forms.Button btnCancel;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public AddPackage()
	{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.BackColor = OrigamColorScheme.FormBackgroundColor;
			this.btnOK.BackColor = OrigamColorScheme.ButtonBackColor;
			this.btnOK.ForeColor = OrigamColorScheme.ButtonForeColor;
			this.btnCancel.BackColor = OrigamColorScheme.ButtonBackColor;
			this.btnCancel.ForeColor = OrigamColorScheme.ButtonForeColor;
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
			this.txtName = new System.Windows.Forms.TextBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 		// label1
			// 		this.label1.Location = new System.Drawing.Point(8, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Name";
			// 		// txtName
			// 		this.txtName.Location = new System.Drawing.Point(104, 24);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(264, 20);
			this.txtName.TabIndex = 1;
			this.txtName.Text = "";
			// 		// btnOK
			// 		this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOK.Location = new System.Drawing.Point(264, 64);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(104, 24);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "OK";
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 		// btnCancel
			// 		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnCancel.Location = new System.Drawing.Point(152, 64);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(104, 24);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			// 		// AddPackage
			// 		this.AcceptButton = this.btnOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(378, 107);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.txtName);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddPackage";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Add Package";
			this.ResumeLayout(false);

		}
	#endregion

	private void btnOK_Click(object sender, System.EventArgs e)
	{
			if(txtName.Text.Trim() == "")
			{
				Origam.UI.AsMessageBox.ShowError(null, "Enter package name.", "Add Package Error", null);
			}
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

	public string PackageName
	{
		get
		{
				return txtName.Text;
			}
	}
}