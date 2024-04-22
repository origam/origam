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

namespace Origam.UI;

/// <summary>
/// Summary description for ErrorReportSendForm.
/// </summary>
public class ErrorReportSendForm : System.Windows.Forms.Form
{
	private System.Windows.Forms.Label label1;
	internal System.Windows.Forms.TextBox txtUserText;
	private System.Windows.Forms.Button btnSend;
	private System.Windows.Forms.Button btnCancel;
	private System.Windows.Forms.ImageList imageList1;
	internal System.Windows.Forms.TextBox txtExpectedResult;
	private System.Windows.Forms.Label label2;
	private System.ComponentModel.IContainer components;

	public ErrorReportSendForm()
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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ErrorReportSendForm));
			this.label1 = new System.Windows.Forms.Label();
			this.txtUserText = new System.Windows.Forms.TextBox();
			this.btnSend = new System.Windows.Forms.Button();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtExpectedResult = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 		// label1
			// 		this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(397, 16);
			this.label1.TabIndex = 8;
			this.label1.Text = ResourceUtils.GetString("DoingWhatLabel");
			// 		// txtUserText
			// 		this.txtUserText.AcceptsReturn = true;
			this.txtUserText.Location = new System.Drawing.Point(8, 24);
			this.txtUserText.Multiline = true;
			this.txtUserText.Name = "txtUserText";
			this.txtUserText.Size = new System.Drawing.Size(528, 128);
			this.txtUserText.TabIndex = 7;
			this.txtUserText.Text = "";
			// 		// btnSend
			// 		this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnSend.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnSend.ImageIndex = 0;
			this.btnSend.ImageList = this.imageList1;
			this.btnSend.Location = new System.Drawing.Point(176, 296);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(248, 24);
			this.btnSend.TabIndex = 9;
			this.btnSend.Text = ResourceUtils.GetString("ButtonSend");
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 		// imageList1
			// 		this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
			// 		// btnCancel
			// 		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnCancel.Location = new System.Drawing.Point(432, 296);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(104, 24);
			this.btnCancel.TabIndex = 10;
			this.btnCancel.Text = ResourceUtils.GetString("ButtonCancel");
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 		// txtExpectedResult
			// 		this.txtExpectedResult.AcceptsReturn = true;
			this.txtExpectedResult.Location = new System.Drawing.Point(8, 176);
			this.txtExpectedResult.Multiline = true;
			this.txtExpectedResult.Name = "txtExpectedResult";
			this.txtExpectedResult.Size = new System.Drawing.Size(528, 112);
			this.txtExpectedResult.TabIndex = 11;
			this.txtExpectedResult.Text = "";
			// 		// label2
			// 		this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 159);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(157, 16);
			this.label2.TabIndex = 12;
			this.label2.Text = ResourceUtils.GetString("ExpectingLabel");
			// 		// ErrorReportSendForm
			// 		this.AcceptButton = this.btnSend;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(544, 334);
			this.ControlBox = false;
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtExpectedResult);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtUserText);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ErrorReportSendForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = ResourceUtils.GetString("ErrorMessageSend");
			this.ResumeLayout(false);

		}
	#endregion

	private void btnCancel_Click(object sender, System.EventArgs e)
	{
			this.Hide();
		}

	private void btnSend_Click(object sender, System.EventArgs e)
	{
			if(txtUserText.Text == "" | txtExpectedResult.Text == "")
			{
				MessageBox.Show(this, ResourceUtils.GetString("EnterAll"), ResourceUtils.GetString("ErrorMessage"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			this.DialogResult = DialogResult.OK;
			this.Hide();
		}
}