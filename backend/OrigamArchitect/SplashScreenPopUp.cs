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

namespace OrigamArchitect;

/// <summary>
/// Summary description for SystemInformation.
/// </summary>
public abstract class SplashScreenPopUp : System.Windows.Forms.Form
{
	private System.Windows.Forms.TextBox txtInfo;
	private System.Windows.Forms.Button btnOK;
	private System.Windows.Forms.Panel panel1;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public SplashScreenPopUp()
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
		this.txtInfo = new System.Windows.Forms.TextBox();
		this.btnOK = new System.Windows.Forms.Button();
		this.panel1 = new System.Windows.Forms.Panel();
		this.panel1.SuspendLayout();
		this.SuspendLayout();
		// 
		// txtInfo
		// 
		this.txtInfo.BackColor = System.Drawing.Color.White;
		this.txtInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtInfo.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txtInfo.ForeColor = System.Drawing.Color.Black;
		this.txtInfo.Location = new System.Drawing.Point(10, 10);
		this.txtInfo.MaxLength = 0;
		this.txtInfo.Multiline = true;
		this.txtInfo.Name = "txtInfo";
		this.txtInfo.ReadOnly = true;
		this.txtInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.txtInfo.Size = new System.Drawing.Size(724, 427);
		this.txtInfo.TabIndex = 6;
		this.txtInfo.WordWrap = true;
		// 
		// btnOK
		// 
		this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
		this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnOK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
		this.btnOK.Location = new System.Drawing.Point(292, 466);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(161, 27);
		this.btnOK.TabIndex = 7;
		this.btnOK.Text = global::OrigamArchitect.strings.Ok_Button;
		this.btnOK.UseVisualStyleBackColor = false;
		// 
		// panel1
		// 
		this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.panel1.BackColor = System.Drawing.Color.White;
		this.panel1.Controls.Add(this.txtInfo);
		this.panel1.Location = new System.Drawing.Point(0, 0);
		this.panel1.Name = "panel1";
		this.panel1.Padding = new System.Windows.Forms.Padding(10);
		this.panel1.Size = new System.Drawing.Size(744, 447);
		this.panel1.TabIndex = 8;
		// 
		// SystemInformation
		// 
		this.AcceptButton = this.btnOK;
		this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
		this.CancelButton = this.btnOK;
		this.ClientSize = new System.Drawing.Size(744, 513);
		this.ControlBox = false;
		this.Controls.Add(this.panel1);
		this.Controls.Add(this.btnOK);
		this.Name = "SystemInformation";
		this.ShowInTaskbar = false;
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = GetTitle();
		this.Load += new System.EventHandler(this.SystemInformation_Load);
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		this.ResumeLayout(false);

	}
	#endregion
		
	protected abstract void SystemInformation_Load(object sender, System.EventArgs e);
	protected abstract string GetTitle();

	protected void SetText(string text)
	{
		txtInfo.Text = text;
	}
}