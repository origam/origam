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
using System.IO;
using System.Windows.Forms;
using Origam.Workbench.Services;

namespace OrigamArchitect;
/// <summary>
/// Summary description for SplashScreen.
/// </summary>
public class SplashScreen : System.Windows.Forms.Form
{
	private System.Windows.Forms.Label lblVersion;
	private System.Windows.Forms.Button btnOK;
	private System.Windows.Forms.PictureBox pictureBox1;
	private System.Windows.Forms.LinkLabel origamLink;
	private System.Windows.Forms.Button btnSystemInformation;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;
	public SplashScreen()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
		lblVersion.Text = string.Format(strings.AppVersion_Label, Application.ProductVersion);
		SchemaService schemaService = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
		if(schemaService != null && schemaService.ActiveExtension != null)
		{
			lblVersion.Text += string.Format(strings.AppVersionModel_Label, schemaService.ActiveExtension.Name);
		}
#if ORIGAM_CLIENT
		string fileName = "splash.png";
		
		if (!File.Exists(fileName))
        {
            return;
        }

        using (Stream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
		{
			Bitmap bitmap = (Bitmap)Bitmap.FromStream(file);
			pictureBox1.Image = bitmap;
		}
#endif
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreen));
        this.lblVersion = new System.Windows.Forms.Label();
        this.btnOK = new System.Windows.Forms.Button();
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        this.origamLink = new System.Windows.Forms.LinkLabel();
        this.btnSystemInformation = new System.Windows.Forms.Button();
        this.btnAttributions = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        this.SuspendLayout();
        // 
        // lblVersion
        // 
        this.lblVersion.BackColor = System.Drawing.Color.Transparent;
        this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
        this.lblVersion.Location = new System.Drawing.Point(392, 550);
        this.lblVersion.Name = "lblVersion";
        this.lblVersion.Size = new System.Drawing.Size(320, 24);
        this.lblVersion.TabIndex = 3;
        this.lblVersion.Text = "<< version info >>";
        this.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
        // 
        // btnOK
        // 
        this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
        this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnOK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
        this.btnOK.Location = new System.Drawing.Point(586, 483);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(124, 24);
        this.btnOK.TabIndex = 4;
        this.btnOK.Text = global::OrigamArchitect.strings.Ok_Button;
        this.btnOK.UseVisualStyleBackColor = false;
        this.btnOK.Visible = false;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        // 
        // pictureBox1
        // 
        this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
        this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
        this.pictureBox1.Location = new System.Drawing.Point(0, 0);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(738, 540);
        this.pictureBox1.TabIndex = 2;
        this.pictureBox1.TabStop = false;
        // 
        // origamLink
        // 
        this.origamLink.AutoSize = true;
        this.origamLink.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
        this.origamLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
        this.origamLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
        this.origamLink.Location = new System.Drawing.Point(29, 550);
        this.origamLink.Name = "origamLink";
        this.origamLink.Size = new System.Drawing.Size(313, 13);
        this.origamLink.TabIndex = 6;
        this.origamLink.TabStop = true;
        this.origamLink.Text = "Powered by ORIGAM® a product of Advantage Solutions, s. r. o.";
        this.origamLink.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        this.origamLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.origamLink_LinkClicked);
        // 
        // btnSystemInformation
        // 
        this.btnSystemInformation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
        this.btnSystemInformation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSystemInformation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
        this.btnSystemInformation.Location = new System.Drawing.Point(438, 483);
        this.btnSystemInformation.Name = "btnSystemInformation";
        this.btnSystemInformation.Size = new System.Drawing.Size(125, 24);
        this.btnSystemInformation.TabIndex = 7;
        this.btnSystemInformation.Text = "System &Information";
        this.btnSystemInformation.UseVisualStyleBackColor = false;
        this.btnSystemInformation.Visible = false;
        this.btnSystemInformation.Click += new System.EventHandler(this.btnSystemInformation_Click);
        // 
        // btnAttributions
        // 
        this.btnAttributions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
        this.btnAttributions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnAttributions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
        this.btnAttributions.Location = new System.Drawing.Point(438, 511);
        this.btnAttributions.Name = "btnAttributions";
        this.btnAttributions.Size = new System.Drawing.Size(125, 24);
        this.btnAttributions.TabIndex = 8;
        this.btnAttributions.Text = "Attributions";
        this.btnAttributions.UseVisualStyleBackColor = false;
        this.btnAttributions.Visible = false;
        this.btnAttributions.Click += new System.EventHandler(this.btnAttributions_Click);
        // 
        // SplashScreen
        // 
        this.AcceptButton = this.btnOK;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
        this.CancelButton = this.btnOK;
        this.ClientSize = new System.Drawing.Size(736, 576);
        this.Controls.Add(this.btnAttributions);
        this.Controls.Add(this.btnSystemInformation);
        this.Controls.Add(this.origamLink);
        this.Controls.Add(this.lblVersion);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.pictureBox1);
        this.ForeColor = System.Drawing.SystemColors.ControlText;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "SplashScreen";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "SplashScreen";
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
	}
	private System.Windows.Forms.Button btnAttributions;
	
	#endregion
	private void btnOK_Click(object sender, System.EventArgs e)
	{
		this.Close();
	}
	private void origamLink_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
	{
		System.Diagnostics.Process.Start("https://www.origam.com");
	}
	private void btnSystemInformation_Click(object sender, System.EventArgs e)
	{
		SystemInformation sysInfo = new SystemInformation();
		sysInfo.ShowDialog();			
	}
	
	private void btnAttributions_Click(object sender, EventArgs e)
	{
		Attributions sysInfo = new Attributions();
		sysInfo.ShowDialog();
	}

	public bool ShowOkButton
	{
		get
		{
			return btnOK.Visible;
		}
		set
		{
			btnOK.Visible = value;
			btnSystemInformation.Visible = value;
            btnAttributions.Visible = value;
        }
	}
}
