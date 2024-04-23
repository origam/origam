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

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for StoredFilterProperties.
/// </summary>
public class StoredFilterProperties : System.Windows.Forms.Form
{
	private System.Windows.Forms.Button btnOK;
	private System.Windows.Forms.Button btnCancel;
	private System.Windows.Forms.Label lblName;
	public System.Windows.Forms.TextBox txtFilterName;
	public System.Windows.Forms.CheckBox chkIsGlobal;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public StoredFilterProperties()
	{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

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
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtFilterName = new System.Windows.Forms.TextBox();
			this.lblName = new System.Windows.Forms.Label();
			this.chkIsGlobal = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 		// btnOK
			// 		this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnOK.Location = new System.Drawing.Point(184, 96);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(88, 24);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = ResourceUtils.GetString("OK1");
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 		// btnCancel
			// 		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnCancel.Location = new System.Drawing.Point(280, 96);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(88, 24);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = ResourceUtils.GetString("Cancel1");
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 		// txtFilterName
			// 		this.txtFilterName.Location = new System.Drawing.Point(96, 16);
			this.txtFilterName.MaxLength = 300;
			this.txtFilterName.Name = "txtFilterName";
			this.txtFilterName.Size = new System.Drawing.Size(272, 20);
			this.txtFilterName.TabIndex = 0;
			this.txtFilterName.Text = "";
			// 		// lblName
			// 		this.lblName.Location = new System.Drawing.Point(8, 16);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(72, 16);
			this.lblName.TabIndex = 3;
			this.lblName.Text = ResourceUtils.GetString("FilterName");
			// 		// chkIsGlobal
			// 		this.chkIsGlobal.Location = new System.Drawing.Point(8, 56);
			this.chkIsGlobal.Name = "chkIsGlobal";
			this.chkIsGlobal.Size = new System.Drawing.Size(352, 16);
			this.chkIsGlobal.TabIndex = 1;
			this.chkIsGlobal.Text = ResourceUtils.GetString("GlobalFilter");
			// 		// StoredFilterProperties
			// 		this.AcceptButton = this.btnOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(386, 136);
			this.ControlBox = false;
			this.Controls.Add(this.chkIsGlobal);
			this.Controls.Add(this.lblName);
			this.Controls.Add(this.txtFilterName);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StoredFilterProperties";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = ResourceUtils.GetString("SaveFilter"); 
			this.ResumeLayout(false);

		}
	#endregion

	private void btnOK_Click(object sender, System.EventArgs e)
	{
			if(txtFilterName.Text == "")
			{
				MessageBox.Show(this, ResourceUtils.GetString("EnterFileName"), ResourceUtils.GetString("SaveFilter"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				this.Hide();
			}
		}

	private void btnCancel_Click(object sender, System.EventArgs e)
	{
			this.Canceled = true;
			this.Hide(); 
		}

	private bool _canceled = false;
	public bool Canceled
	{
		get
		{
				return _canceled;
			}
		set
		{
				_canceled = value;
			}
	}
}