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

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for NotificationList.
/// </summary>
public class NotificationList : System.Windows.Forms.UserControl
{
	private System.Windows.Forms.ListView listView1;
	private System.Windows.Forms.ImageList imageList1;
	private System.Windows.Forms.ColumnHeader columnHeader1;
	private System.ComponentModel.IContainer components;
	public NotificationList()
	{
		// This call is required by the Windows.Forms Form Designer.
		InitializeComponent();
		this.SizeChanged += new EventHandler(NotificationList_SizeChanged);
	}
	public void SetList(string text)
	{
		this.listView1.Items.Clear();
		if(text != null)
		{
			string[] items = text.Split("\n".ToCharArray());
			foreach(string item in items)
			{
				if(item.Substring(0, 2) == "! ")
				{
					listView1.Items.Add(item.Substring(2), 1);
				}
				else if(item.Substring(0, 3) == "!! ")
				{
					listView1.Items.Add(item.Substring(3), 2);
				}
				else
				{
					listView1.Items.Add(item, 0);
				}
			}
		}
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
	#region Component Designer generated code
	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NotificationList));
		this.listView1 = new System.Windows.Forms.ListView();
		this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
		this.imageList1 = new System.Windows.Forms.ImageList(this.components);
		this.SuspendLayout();
		// 
		// listView1
		// 
		this.listView1.AutoArrange = false;
		this.listView1.BackColor = System.Drawing.Color.LemonChiffon;
		this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					this.columnHeader1});
		this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listView1.FullRowSelect = true;
		this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
		this.listView1.LargeImageList = this.imageList1;
		this.listView1.Location = new System.Drawing.Point(0, 0);
		this.listView1.MultiSelect = false;
		this.listView1.Name = "listView1";
		this.listView1.Size = new System.Drawing.Size(336, 19);
		this.listView1.SmallImageList = this.imageList1;
		this.listView1.TabIndex = 0;
		this.listView1.View = System.Windows.Forms.View.Details;
		// 
		// imageList1
		// 
		this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
		this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
		this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
		this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
		// 
		// NotificationList
		// 
		this.Controls.Add(this.listView1);
		this.Name = "NotificationList";
		this.Size = new System.Drawing.Size(336, 19);
		this.ResumeLayout(false);
	}
	#endregion
	private void NotificationList_SizeChanged(object sender, EventArgs e)
	{
		this.listView1.Columns[0].Width = this.Width - (this.VScroll ? System.Windows.Forms.SystemInformation.VerticalScrollBarWidth : 0);
	}
}
