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
using System.Windows.Forms;

namespace Origam.UI;
/// <summary>
/// Summary description for ASMessageBox.
/// </summary>
public class ASMessageBoxForm : System.Windows.Forms.Form
{
	private System.Windows.Forms.Label lblMessage;
	private System.Windows.Forms.PictureBox pctIcon;
	private System.Windows.Forms.ImageList imageList1;
	private System.Windows.Forms.ImageList imageList2;
	private System.Windows.Forms.Panel panel1;
	private System.Windows.Forms.TextBox txtDetails;
	private System.Windows.Forms.Button btnCopy;
	private System.Windows.Forms.Button btnOK;
	private System.Windows.Forms.Button btnDetails;
	private System.Windows.Forms.Button btnMail;
	private System.ComponentModel.IContainer components;
	private System.Windows.Forms.ToolTip toolTip1;
	private IDebugInfoProvider _debugInfoProvider = null;
	public ASMessageBoxForm()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
		this.BackColor = OrigamColorScheme.FormBackgroundColor;
		this.btnDetails.BackColor = OrigamColorScheme.ButtonBackColor;
		this.btnDetails.ForeColor = OrigamColorScheme.ButtonForeColor;
		this.btnOK.BackColor = OrigamColorScheme.ButtonBackColor;
		this.btnOK.ForeColor = OrigamColorScheme.ButtonForeColor;
#if ORIGAM_CLIENT
#else
		this.btnDetails.Text = "&Details >>";
#endif
	}
	public ASMessageBoxForm(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, Exception exception, IDebugInfoProvider debugInfoProvider) : this()
	{
		_debugInfoProvider = debugInfoProvider;
		this.Text = caption;
		lblMessage.Text = text;
		
		txtDetails.Text = "========================================" + Environment.NewLine;
		txtDetails.Text += caption + Environment.NewLine;
		txtDetails.Text += text + Environment.NewLine;
		txtDetails.Text += "========================================" + Environment.NewLine;
		Image image;
		switch(icon)
		{
            case MessageBoxIcon.Asterisk:
                {
                    image = System.Drawing.SystemIcons.Asterisk.ToBitmap();
                    break;
                }

            case MessageBoxIcon.Error:
                {
                    image = imageList2.Images[0];
                    break;
                }

            case MessageBoxIcon.Exclamation:
                {
                    image = System.Drawing.SystemIcons.Exclamation.ToBitmap();
                    break;
                }

            case MessageBoxIcon.None:
                {
                    image = null;
                    break;
                }

            case MessageBoxIcon.Question:
                {
                    image = System.Drawing.SystemIcons.Question.ToBitmap();
                    break;
                }

            default:
				throw new ArgumentOutOfRangeException("icon", icon, ResourceUtils.GetString("ErrorUnknownIconType"));
		}
		pctIcon.Image = image;
		string stackTrace = "";
		Exception ex = exception;
		while(ex != null)
		{
			txtDetails.Text += ex.Message + Environment.NewLine;
			txtDetails.Text += "------------------------------------------" + Environment.NewLine;
			stackTrace = ex.StackTrace + stackTrace;
			ex = ex.InnerException;
		}
		if(stackTrace != "")
		{
			txtDetails.Text += "========================================" + Environment.NewLine;
			txtDetails.Text += " Stack trace" + Environment.NewLine;
			txtDetails.Text += "========================================" + Environment.NewLine;
			txtDetails.Text += stackTrace;
		}
		// size the dialog box
		System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(lblMessage.Handle);
		System.Drawing.SizeF size = g.MeasureString(text, lblMessage.Font);
		lblMessage.Size = size.ToSize();
		lblMessage.Width += 16;
		lblMessage.Height += 16;
		int messagePartHeight = lblMessage.Height > pctIcon.Height ? lblMessage.Height : pctIcon.Height;
		int width = lblMessage.Left + lblMessage.Width + 16;
		int height = lblMessage.Top + messagePartHeight + 32 + btnCopy.Height + 32;
		if(width < this.Width)
        {
            width = this.Width;
        }

        if (height < this.Height)
        {
            height = this.Height;
        }

        this.Width = width;
		this.Height = height;
		panel1.Top = lblMessage.Top + messagePartHeight + 16;
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
		this.components = new System.ComponentModel.Container();
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ASMessageBoxForm));
		this.pctIcon = new System.Windows.Forms.PictureBox();
		this.lblMessage = new System.Windows.Forms.Label();
		this.imageList1 = new System.Windows.Forms.ImageList(this.components);
		this.imageList2 = new System.Windows.Forms.ImageList(this.components);
		this.panel1 = new System.Windows.Forms.Panel();
		this.btnMail = new System.Windows.Forms.Button();
		this.btnCopy = new System.Windows.Forms.Button();
		this.btnDetails = new System.Windows.Forms.Button();
		this.btnOK = new System.Windows.Forms.Button();
		this.txtDetails = new System.Windows.Forms.TextBox();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.panel1.SuspendLayout();
		this.SuspendLayout();
		// 
		// pctIcon
		// 
		this.pctIcon.BackColor = System.Drawing.Color.Transparent;
		this.pctIcon.Location = new System.Drawing.Point(8, 16);
		this.pctIcon.Name = "pctIcon";
		this.pctIcon.Size = new System.Drawing.Size(40, 40);
		this.pctIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
		this.pctIcon.TabIndex = 0;
		this.pctIcon.TabStop = false;
		// 
		// lblMessage
		// 
		this.lblMessage.Location = new System.Drawing.Point(64, 16);
		this.lblMessage.Name = "lblMessage";
		this.lblMessage.Size = new System.Drawing.Size(480, 40);
		this.lblMessage.TabIndex = 1;
		this.toolTip1.SetToolTip(this.lblMessage, ResourceUtils.GetString("ErrorMessage"));
		// 
		// imageList1
		// 
		this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
		this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
		this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
		this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
		// 
		// imageList2
		// 
		this.imageList2.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
		this.imageList2.ImageSize = new System.Drawing.Size(32, 32);
		this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
		this.imageList2.TransparentColor = System.Drawing.Color.Magenta;
		// 
		// panel1
		// 
		this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.panel1.BackColor = System.Drawing.SystemColors.Control;
		this.panel1.Controls.Add(this.btnMail);
		this.panel1.Controls.Add(this.btnCopy);
		this.panel1.Controls.Add(this.btnDetails);
		this.panel1.Controls.Add(this.btnOK);
		this.panel1.Controls.Add(this.txtDetails);
		this.panel1.Location = new System.Drawing.Point(0, 80);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(648, 224);
		this.panel1.TabIndex = 4;
		// 
		// btnMail
		// 
		this.btnMail.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.btnMail.ImageIndex = 1;
		this.btnMail.ImageList = this.imageList1;
		this.btnMail.Location = new System.Drawing.Point(48, 8);
		this.btnMail.Name = "btnMail";
		this.btnMail.Size = new System.Drawing.Size(24, 24);
		this.btnMail.TabIndex = 4;
		this.toolTip1.SetToolTip(this.btnMail, ResourceUtils.GetString("TooltipMail"));
		this.btnMail.Click += new System.EventHandler(this.btnMail_Click);
		// 
		// btnCopy
		// 
		this.btnCopy.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.btnCopy.ImageIndex = 0;
		this.btnCopy.ImageList = this.imageList1;
		this.btnCopy.Location = new System.Drawing.Point(16, 8);
		this.btnCopy.Name = "btnCopy";
		this.btnCopy.Size = new System.Drawing.Size(24, 24);
		this.btnCopy.TabIndex = 1;
		this.toolTip1.SetToolTip(this.btnCopy, ResourceUtils.GetString("TooltipCopy"));
		this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
		// 
		// btnDetails
		// 
		this.btnDetails.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.btnDetails.Location = new System.Drawing.Point(80, 8);
		this.btnDetails.Name = "btnDetails";
		this.btnDetails.Size = new System.Drawing.Size(96, 24);
		this.btnDetails.TabIndex = 2;
		this.btnDetails.Text = "&Podrobnosti >>";
		this.toolTip1.SetToolTip(this.btnDetails, ResourceUtils.GetString("TooltipDetails"));
		this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
		// 
		// btnOK
		// 
		this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.btnOK.Location = new System.Drawing.Point(550, 8);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(90, 24);
		this.btnOK.TabIndex = 0;
		this.btnOK.Text = ResourceUtils.GetString("ButtonOK");
		this.toolTip1.SetToolTip(this.btnOK, ResourceUtils.GetString("TooltipOK"));
		this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
		// 
		// txtDetails
		// 
		this.txtDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtDetails.Location = new System.Drawing.Point(8, 56);
		this.txtDetails.Multiline = true;
		this.txtDetails.Name = "txtDetails";
		this.txtDetails.ReadOnly = true;
		this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
		this.txtDetails.Size = new System.Drawing.Size(638, 160);
		this.txtDetails.TabIndex = 3;
		this.txtDetails.Text = "";
		this.txtDetails.WordWrap = false;
		// 
		// ASMessageBoxForm
		// 
		this.AcceptButton = this.btnOK;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.CancelButton = this.btnOK;
		this.ClientSize = new System.Drawing.Size(650, 120);
		this.ControlBox = false;
		this.Controls.Add(this.panel1);
		this.Controls.Add(this.lblMessage);
		this.Controls.Add(this.pctIcon);
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.Name = "ASMessageBoxForm";
		this.ShowInTaskbar = false;
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "ASMessageBox";
		this.panel1.ResumeLayout(false);
		this.ResumeLayout(false);
	}
	#endregion
	private void btnDetails_Click(object sender, System.EventArgs e)
	{
		this.Height = panel1.Top + panel1.Height + 30;
	}
	private void btnOK_Click(object sender, System.EventArgs e)
	{
		this.Hide();
	}
	private void btnMail_Click(object sender, System.EventArgs e)
	{
		try
		{
			ErrorReportSendForm reportForm = new ErrorReportSendForm();
			if(reportForm.ShowDialog(this) == DialogResult.OK)
			{
				string text = reportForm.txtUserText.Text == "" ? txtDetails.Text : 
					reportForm.txtUserText.Text 
					+ Environment.NewLine 
					+ Environment.NewLine 
					+ "Expected user result:"
					+ Environment.NewLine 
					+ reportForm.txtExpectedResult.Text
					+ Environment.NewLine 
					+ txtDetails.Text;
				text += this.DebugInfo();
				Mapi ma = new Mapi();
				ma.Logon( IntPtr.Zero );
				ma.AddRecip("ORIGAM Support", "SMTP:support@advantages.cz", false ); 
//					ma.AddRecip("ORIGAM Support", "SMTP:tomas.vavrda@advantages.cz", false ); 
				ma.Send("Error Report: " + this.Text, text);
				ma.Logoff();
			}
		}
		catch(Exception ex)
		{
			MessageBox.Show(this, ex.Message, ResourceUtils.GetString("ErrorWhenSendMail"), MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
	private void btnCopy_Click(object sender, System.EventArgs e)
	{
		try
		{
			Clipboard.SetDataObject(txtDetails.Text + this.DebugInfo());
		}
		catch(Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Chyba pøi kopírování do schránky", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
	private string DebugInfo()
	{
		if(_debugInfoProvider != null)
		{
			return Environment.NewLine + Environment.NewLine + _debugInfoProvider.GetInfo();
		}

        return "";
    }
}
