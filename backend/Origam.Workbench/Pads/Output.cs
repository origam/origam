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
using System.Text;
using System.Windows.Forms;
using Origam.UI.Common;

namespace Origam.Workbench.Pads;
/// <summary>
/// Summary description for Output.
/// </summary>
public class OutputPad : AbstractPadContent, IOutputPad
{
	private System.Windows.Forms.TextBox txtOutput;
    private System.ComponentModel.IContainer components;
    private StringBuilder _queue = new StringBuilder();
    public Panel toolBar;
    private Timer timer;
    private object _lock = new object();
	public OutputPad()
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
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OutputPad));
        this.txtOutput = new System.Windows.Forms.TextBox();
        this.toolBar = new System.Windows.Forms.Panel();
        this.timer = new System.Windows.Forms.Timer(this.components);
        this.SuspendLayout();
        // 
        // txtOutput
        // 
        this.txtOutput.AcceptsTab = true;
        this.txtOutput.BackColor = System.Drawing.SystemColors.Window;
        this.txtOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtOutput.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        this.txtOutput.Location = new System.Drawing.Point(0, 27);
        this.txtOutput.MaxLength = 0;
        this.txtOutput.Multiline = true;
        this.txtOutput.Name = "txtOutput";
        this.txtOutput.ReadOnly = true;
        this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        this.txtOutput.Size = new System.Drawing.Size(352, 244);
        this.txtOutput.TabIndex = 0;
        this.txtOutput.WordWrap = false;
        this.txtOutput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtOutput_KeyDown);
        // 
        // toolBar
        // 
        this.toolBar.Dock = System.Windows.Forms.DockStyle.Top;
        this.toolBar.Location = new System.Drawing.Point(0, 0);
        this.toolBar.Name = "toolBar";
        this.toolBar.Size = new System.Drawing.Size(352, 27);
        this.toolBar.TabIndex = 1;
        this.toolBar.Visible = false;
        // 
        // timer
        // 
        this.timer.Interval = 200;
        this.timer.Tick += new System.EventHandler(this.timer_Tick);
        // 
        // OutputPad
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(352, 271);
        this.Controls.Add(this.txtOutput);
        this.Controls.Add(this.toolBar);
        this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft) 
        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight) 
        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop) 
        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
        this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
        this.HideOnClose = true;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "OutputPad";
        this.TabText = "Output";
        this.Text = "Output";
        this.ResumeLayout(false);
        this.PerformLayout();
	}
	#endregion
	public void SetOutputText(string sText)
	{
		txtOutput.Text = sText;
	}
    private void AddTextInternal(string sText)
    {
        if (IsDisposed)
        {
            return;
        }

        int startPosition = 0;
        if (sText.StartsWith("Origam."))
        {
            startPosition = 19;
        }
        txtOutput.AppendText(sText.Substring(startPosition));
        int pos = txtOutput.Text.Length;
        int originalPosition = txtOutput.SelectionStart;
        bool shouldScroll = txtOutput.SelectionStart == pos;
        // scroll to the end after adding the text in case the caret 
        // was at the end before adding
        if (shouldScroll)
        {
            txtOutput.SelectionStart = txtOutput.Text.Length;
        }
        else
        {
            txtOutput.SelectionStart = originalPosition;
        }
        txtOutput.ScrollToCaret();
    }
	public void AppendText(string sText)
	{
		AddText(sText + Environment.NewLine);
	}
	public void AddText(string sText)
	{
		if(this.txtOutput.InvokeRequired)
		{
			lock(_lock)
			{
				_queue.Append(sText);
			}
		}
		else
		{
			FlushQueue();
			AddTextInternal(sText);
		}
	}
	private void txtOutput_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
	{
		if(e.KeyCode == Keys.Delete)
		{
			txtOutput.Text = "";
		}
		else if((e.KeyCode == Keys.A) && e.Control)
		{
			txtOutput.SelectAll();
		}
	}
	private void FlushQueue()
	{
		lock(_lock)
		{
			if(_queue.Length != 0)
			{
				AddTextInternal(_queue.ToString());
				_queue = new StringBuilder();
			}
		}
	}
    private void timer_Tick(object sender, EventArgs e)
    {
        FlushQueue();
    }
}
