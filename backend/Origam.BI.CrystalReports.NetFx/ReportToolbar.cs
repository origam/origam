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

using Origam.UI;

using CrystalDecisions.Windows.Forms;

namespace Origam.BI.CrystalReports;

/// <summary>
/// Summary description for ReportToolbar.
/// </summary>
public class ReportToolbar : System.Windows.Forms.UserControl
{
	public event EventHandler ReportRefreshRequested;

	private System.Windows.Forms.ImageList imagelist;
	private Origam.Gui.Win.AsPanelTitle pnlToolbar;
	private System.Windows.Forms.Panel panel1;
	private System.Windows.Forms.ToolBar toolbar;
	private System.Windows.Forms.TextBox txtFindText;
	private System.Windows.Forms.ToolBarButton btnFirst;
	private System.Windows.Forms.ToolBarButton btnPrevious;
	private System.Windows.Forms.ToolBarButton btnNext;
	private System.Windows.Forms.ToolBarButton btnLast;
	private System.Windows.Forms.ToolBarButton toolBarButton1;
	private System.Windows.Forms.ToolBarButton btnGroupTree;
	private System.Windows.Forms.ToolBarButton btnClose;
	private System.Windows.Forms.ToolBarButton toolBarButton2;
	private System.Windows.Forms.ToolBarButton btnSave;
	private System.Windows.Forms.ToolBarButton btnRefresh;
	private System.Windows.Forms.ToolBarButton btnPrint;
	private System.Windows.Forms.ToolBarButton btnZoom;
	private System.Windows.Forms.ContextMenu zoomMenu;
	private System.Windows.Forms.MenuItem mnuZoom400;
	private System.Windows.Forms.MenuItem mnuZoom300;
	private System.Windows.Forms.MenuItem mnuZoom200;
	private System.Windows.Forms.MenuItem mnuZoom150;
	private System.Windows.Forms.MenuItem mnuZoom100;
	private System.Windows.Forms.MenuItem mnuZoom75;
	private System.Windows.Forms.MenuItem mnuZoom50;
	private System.Windows.Forms.MenuItem mnuZoom25;
	private System.Windows.Forms.MenuItem mnuZoomPageWidth;
	private System.Windows.Forms.MenuItem mnuZoomWholePage;
	private System.ComponentModel.IContainer components;
	private System.Windows.Forms.ToolBar toolBar1;
	private System.Windows.Forms.ToolBarButton btnFind;
	private System.Windows.Forms.ToolBarButton toolBarButton3;

	private bool _searchEmpty = true;

	public ReportToolbar()
	{
		// This call is required by the Windows.Forms Form Designer.
		InitializeComponent();

		pnlToolbar.StartColor = OrigamColorScheme.TitleActiveStartColor;
		pnlToolbar.EndColor = OrigamColorScheme.TitleActiveEndColor;
		pnlToolbar.ForeColor = OrigamColorScheme.TitleActiveForeColor;
		pnlToolbar.MiddleStartColor = OrigamColorScheme.TitleActiveMiddleStartColor;
		pnlToolbar.MiddleEndColor = OrigamColorScheme.TitleActiveMiddleEndColor;
	}

	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
		if(_crViewer != null)
		{
			_crViewer.Navigate -= new CrystalDecisions.Windows.Forms.NavigateEventHandler(value_Navigate);
		}

		this.mnuZoom400.Click -= new System.EventHandler(this.mnuZoom400_Click);
		this.mnuZoom300.Click -= new System.EventHandler(this.mnuZoom300_Click);
		this.mnuZoom200.Click -= new System.EventHandler(this.mnuZoom200_Click);
		this.mnuZoom150.Click -= new System.EventHandler(this.mnuZoom150_Click);
		this.mnuZoom100.Click -= new System.EventHandler(this.mnuZoom100_Click);
		this.mnuZoom75.Click -= new System.EventHandler(this.mnuZoom75_Click);
		this.mnuZoom50.Click -= new System.EventHandler(this.mnuZoom50_Click);
		this.mnuZoom25.Click -= new System.EventHandler(this.mnuZoom25_Click);
		this.mnuZoomPageWidth.Click -= new System.EventHandler(this.mnuZoomPageWidth_Click);
		this.mnuZoomWholePage.Click -= new System.EventHandler(this.mnuZoomWholePage_Click);

		this.toolbar.ButtonClick -= new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolbar_ButtonClick);
		this.toolBar1.ButtonClick -= new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);

		if(this.zoomMenu != null)
		{
			this.zoomMenu.Dispose();
		}

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
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ReportToolbar));
		this.imagelist = new System.Windows.Forms.ImageList(this.components);
		this.pnlToolbar = new Origam.Gui.Win.AsPanelTitle();
		this.panel1 = new System.Windows.Forms.Panel();
		this.toolbar = new System.Windows.Forms.ToolBar();
		this.toolBarButton3 = new System.Windows.Forms.ToolBarButton();
		this.btnFirst = new System.Windows.Forms.ToolBarButton();
		this.btnPrevious = new System.Windows.Forms.ToolBarButton();
		this.btnNext = new System.Windows.Forms.ToolBarButton();
		this.btnLast = new System.Windows.Forms.ToolBarButton();
		this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
		this.btnGroupTree = new System.Windows.Forms.ToolBarButton();
		this.btnClose = new System.Windows.Forms.ToolBarButton();
		this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
		this.btnSave = new System.Windows.Forms.ToolBarButton();
		this.btnPrint = new System.Windows.Forms.ToolBarButton();
		this.btnZoom = new System.Windows.Forms.ToolBarButton();
		this.zoomMenu = new System.Windows.Forms.ContextMenu();
		this.mnuZoom400 = new System.Windows.Forms.MenuItem();
		this.mnuZoom300 = new System.Windows.Forms.MenuItem();
		this.mnuZoom200 = new System.Windows.Forms.MenuItem();
		this.mnuZoom150 = new System.Windows.Forms.MenuItem();
		this.mnuZoom100 = new System.Windows.Forms.MenuItem();
		this.mnuZoom75 = new System.Windows.Forms.MenuItem();
		this.mnuZoom50 = new System.Windows.Forms.MenuItem();
		this.mnuZoom25 = new System.Windows.Forms.MenuItem();
		this.mnuZoomPageWidth = new System.Windows.Forms.MenuItem();
		this.mnuZoomWholePage = new System.Windows.Forms.MenuItem();
		this.btnRefresh = new System.Windows.Forms.ToolBarButton();
		this.txtFindText = new System.Windows.Forms.TextBox();
		this.toolBar1 = new System.Windows.Forms.ToolBar();
		this.btnFind = new System.Windows.Forms.ToolBarButton();
		this.pnlToolbar.SuspendLayout();
		this.panel1.SuspendLayout();
		this.SuspendLayout();
		// 
		// imagelist
		// 
		this.imagelist.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
		this.imagelist.ImageSize = new System.Drawing.Size(16, 16);
		this.imagelist.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imagelist.ImageStream")));
		this.imagelist.TransparentColor = System.Drawing.Color.Magenta;
		// 
		// pnlToolbar
		// 
		this.pnlToolbar.Controls.Add(this.panel1);
		this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
		this.pnlToolbar.DockPadding.Top = 2;
		this.pnlToolbar.EndColor =  Color.FromArgb(254, 225, 122);
		this.pnlToolbar.ForeColor = System.Drawing.Color.Black;
		this.pnlToolbar.Location = new System.Drawing.Point(0, 0);
		this.pnlToolbar.MiddleEndColor = Color.FromArgb(255, 187, 132);
		this.pnlToolbar.MiddleStartColor = Color.FromArgb(255, 171, 63);
		this.pnlToolbar.Name = "pnlToolbar";
		this.pnlToolbar.PanelIcon = ((System.Drawing.Bitmap)(resources.GetObject("pnlToolbar.PanelIcon")));
		this.pnlToolbar.PanelTitle = "";
		this.pnlToolbar.Size = new System.Drawing.Size(560, 24);
		this.pnlToolbar.StartColor = Color.FromArgb(255, 217, 170);
		this.pnlToolbar.TabIndex = 5;
		// 
		// panel1
		// 
		this.panel1.BackColor = System.Drawing.Color.Transparent;
		this.panel1.Controls.Add(this.toolbar);
		this.panel1.Controls.Add(this.txtFindText);
		this.panel1.Controls.Add(this.toolBar1);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
		this.panel1.Location = new System.Drawing.Point(128, 2);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(432, 22);
		this.panel1.TabIndex = 2;
		// 
		// toolbar
		// 
		this.toolbar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
		this.toolbar.AutoSize = false;
		this.toolbar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
			this.toolBarButton3,
			this.btnFirst,
			this.btnPrevious,
			this.btnNext,
			this.btnLast,
			this.toolBarButton1,
			this.btnGroupTree,
			this.btnClose,
			this.toolBarButton2,
			this.btnSave,
			this.btnPrint,
			this.btnZoom,
			this.btnRefresh});
		this.toolbar.ButtonSize = new System.Drawing.Size(17, 17);
		this.toolbar.Divider = false;
		this.toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
		this.toolbar.DropDownArrows = true;
		this.toolbar.ImageList = this.imagelist;
		this.toolbar.Location = new System.Drawing.Point(160, 0);
		this.toolbar.Name = "toolbar";
		this.toolbar.ShowToolTips = true;
		this.toolbar.Size = new System.Drawing.Size(272, 224);
		this.toolbar.TabIndex = 6;
		this.toolbar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolbar_ButtonClick);
		// 
		// toolBarButton3
		// 
		this.toolBarButton3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
		// 
		// btnFirst
		// 
		this.btnFirst.ImageIndex = 0;
		this.btnFirst.ToolTipText = ResourceUtils.GetString("TooltipFirstPage");
		// 
		// btnPrevious
		// 
		this.btnPrevious.ImageIndex = 1;
		this.btnPrevious.ToolTipText = ResourceUtils.GetString("TooltipPrevPage");
		// 
		// btnNext
		// 
		this.btnNext.ImageIndex = 3;
		this.btnNext.ToolTipText = ResourceUtils.GetString("TooltipNextPage");
		// 
		// btnLast
		// 
		this.btnLast.ImageIndex = 4;
		this.btnLast.ToolTipText = ResourceUtils.GetString("TooltipLastPage");
		// 
		// toolBarButton1
		// 
		this.toolBarButton1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
		// 
		// btnGroupTree
		// 
		this.btnGroupTree.ImageIndex = 10;
		this.btnGroupTree.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
		this.btnGroupTree.ToolTipText = ResourceUtils.GetString("TooltipGroup");
		// 
		// btnClose
		// 
		this.btnClose.ImageIndex = 2;
		this.btnClose.ToolTipText = ResourceUtils.GetString("TooltipReportClose");
		// 
		// toolBarButton2
		// 
		this.toolBarButton2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
		// 
		// btnSave
		// 
		this.btnSave.ImageIndex = 5;
		this.btnSave.ToolTipText = ResourceUtils.GetString("TooltipExportFile");
		// 
		// btnPrint
		// 
		this.btnPrint.ImageIndex = 7;
		this.btnPrint.ToolTipText = ResourceUtils.GetString("TooltipReportPrint");
		// 
		// btnZoom
		// 
		this.btnZoom.DropDownMenu = this.zoomMenu;
		this.btnZoom.ImageIndex = 8;
		this.btnZoom.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
		this.btnZoom.ToolTipText = ResourceUtils.GetString("TooltipZoom");
		// 
		// zoomMenu
		// 
		this.zoomMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.mnuZoom400,
			this.mnuZoom300,
			this.mnuZoom200,
			this.mnuZoom150,
			this.mnuZoom100,
			this.mnuZoom75,
			this.mnuZoom50,
			this.mnuZoom25,
			this.mnuZoomPageWidth,
			this.mnuZoomWholePage});
		// 
		// mnuZoom400
		// 
		this.mnuZoom400.Index = 0;
		this.mnuZoom400.Text = "400 %";
		this.mnuZoom400.Click += new System.EventHandler(this.mnuZoom400_Click);
		// 
		// mnuZoom300
		// 
		this.mnuZoom300.Index = 1;
		this.mnuZoom300.Text = "300 %";
		this.mnuZoom300.Click += new System.EventHandler(this.mnuZoom300_Click);
		// 
		// mnuZoom200
		// 
		this.mnuZoom200.Index = 2;
		this.mnuZoom200.Text = "200 %";
		this.mnuZoom200.Click += new System.EventHandler(this.mnuZoom200_Click);
		// 
		// mnuZoom150
		// 
		this.mnuZoom150.Index = 3;
		this.mnuZoom150.Text = "150 %";
		this.mnuZoom150.Click += new System.EventHandler(this.mnuZoom150_Click);
		// 
		// mnuZoom100
		// 
		this.mnuZoom100.Index = 4;
		this.mnuZoom100.Text = "100 %";
		this.mnuZoom100.Click += new System.EventHandler(this.mnuZoom100_Click);
		// 
		// mnuZoom75
		// 
		this.mnuZoom75.Index = 5;
		this.mnuZoom75.Text = "75 %";
		this.mnuZoom75.Click += new System.EventHandler(this.mnuZoom75_Click);
		// 
		// mnuZoom50
		// 
		this.mnuZoom50.Index = 6;
		this.mnuZoom50.Text = "50 %";
		this.mnuZoom50.Click += new System.EventHandler(this.mnuZoom50_Click);
		// 
		// mnuZoom25
		// 
		this.mnuZoom25.Index = 7;
		this.mnuZoom25.Text = "25 %";
		this.mnuZoom25.Click += new System.EventHandler(this.mnuZoom25_Click);
		// 
		// mnuZoomPageWidth
		// 
		this.mnuZoomPageWidth.Index = 8;
		this.mnuZoomPageWidth.Text = ResourceUtils.GetString("PageWidth");
		this.mnuZoomPageWidth.Click += new System.EventHandler(this.mnuZoomPageWidth_Click);
		// 
		// mnuZoomWholePage
		// 
		this.mnuZoomWholePage.Index = 9;
		this.mnuZoomWholePage.Text = ResourceUtils.GetString("WholePage");
		this.mnuZoomWholePage.Click += new System.EventHandler(this.mnuZoomWholePage_Click);
		// 
		// btnRefresh
		// 
		this.btnRefresh.ImageIndex = 6;
		this.btnRefresh.ToolTipText = ResourceUtils.GetString("TooltipReportRefresh");
		// 
		// txtFindText
		// 
		this.txtFindText.Dock = System.Windows.Forms.DockStyle.Left;
		this.txtFindText.ForeColor = System.Drawing.SystemColors.GrayText;
		this.txtFindText.Location = new System.Drawing.Point(24, 0);
		this.txtFindText.Name = "txtFindText";
		this.txtFindText.Size = new System.Drawing.Size(136, 20);
		this.txtFindText.TabIndex = 1;
		this.txtFindText.Text = ResourceUtils.GetString("TextToSearch");
		this.txtFindText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFindText_KeyDown);
		this.txtFindText.TextChanged += new System.EventHandler(this.txtFindText_TextChanged);
		this.txtFindText.Leave += new System.EventHandler(this.txtFindText_Leave);
		this.txtFindText.Enter += new System.EventHandler(this.txtFindText_Enter);
		// 
		// toolBar1
		// 
		this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
		this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
			this.btnFind});
		this.toolBar1.Divider = false;
		this.toolBar1.Dock = System.Windows.Forms.DockStyle.Left;
		this.toolBar1.DropDownArrows = true;
		this.toolBar1.ImageList = this.imagelist;
		this.toolBar1.Location = new System.Drawing.Point(0, 0);
		this.toolBar1.Name = "toolBar1";
		this.toolBar1.ShowToolTips = true;
		this.toolBar1.Size = new System.Drawing.Size(24, 22);
		this.toolBar1.TabIndex = 7;
		this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
		// 
		// btnFind
		// 
		this.btnFind.ImageIndex = 9;
		this.btnFind.ToolTipText = ResourceUtils.GetString("TooltipReportFind");
		// 
		// ReportToolbar
		// 
		this.Controls.Add(this.pnlToolbar);
		this.Name = "ReportToolbar";
		this.Size = new System.Drawing.Size(560, 24);
		this.pnlToolbar.ResumeLayout(false);
		this.panel1.ResumeLayout(false);
		this.ResumeLayout(false);

	}
	#endregion

	#region Public Properties
	private CrystalReportViewer _crViewer = null;
	public CrystalReportViewer ReportViewer
	{
		get
		{
			return _crViewer;
		}
		set
		{
			if(_crViewer != null)
			{
				_crViewer.Navigate -= new CrystalDecisions.Windows.Forms.NavigateEventHandler(value_Navigate);
			}

			_crViewer = value;

			if(value != null)
			{
				_crViewer.Navigate += new CrystalDecisions.Windows.Forms.NavigateEventHandler(value_Navigate);
			}
		}
	}

	public string Caption
	{
		get
		{
			return this.pnlToolbar.PanelTitle;
		}
		set
		{
			this.pnlToolbar.PanelTitle = value;
		}
	}


	public Color StartColor
	{
		get
		{
			return this.pnlToolbar.StartColor;
		}
		set
		{
			this.pnlToolbar.StartColor = value;
		}
	}

	public Color MiddleEndColor
	{
		get
		{
			return this.pnlToolbar.MiddleEndColor;
		}
		set
		{
			this.pnlToolbar.MiddleEndColor = value;
		}
	}

	public Color MiddleStartColor
	{
		get
		{
			return this.pnlToolbar.MiddleStartColor;
		}
		set
		{
			this.pnlToolbar.MiddleStartColor = value;
		}
	}

	public Color EndColor
	{
		get
		{
			return this.pnlToolbar.EndColor;
		}
		set
		{
			this.pnlToolbar.EndColor = value;
		}
	}

	public bool ShowRefreshButton
	{
		get
		{
			return this.btnRefresh.Visible;
		}
		set
		{
			this.btnRefresh.Visible = value;
		}
	}
	#endregion

	private void toolbar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
	{
		if(ReportViewer == null) return;

		if(e.Button == btnClose)
		{
			ReportViewer.CloseView(null);
		}
		else if(e.Button == btnFirst)
		{
			ReportViewer.ShowFirstPage();
		}
		else if(e.Button == btnLast)
		{
			ReportViewer.ShowLastPage();
			UpdateNavigationButtons(ReportViewer.GetCurrentPageNumber(), false);
		}
		else if(e.Button == btnNext)
		{
			int oldPageNumber = ReportViewer.GetCurrentPageNumber();
			ReportViewer.ShowNextPage();
			if(oldPageNumber == ReportViewer.GetCurrentPageNumber())
			{
				UpdateNavigationButtons(oldPageNumber, false);
			}
		}
		else if(e.Button == btnPrevious)
		{
			ReportViewer.ShowPreviousPage();
		}
		else if(e.Button == btnPrint)
		{
			ReportViewer.PrintReport();
		}
		else if(e.Button == btnRefresh)
		{
			OnReportRefreshRequested();
		}
		else if(e.Button == btnSave)
		{
			ReportViewer.ExportReport();
		}
		else if(e.Button == btnZoom)
		{
			ReportViewer.Zoom(100);
		}
		else if(e.Button == btnGroupTree)
		{
			if(btnGroupTree.Pushed)
			{
				ReportViewer.ToolPanelView = CrystalDecisions.Windows.Forms.ToolPanelViewType.GroupTree;
			}
			else
			{
				ReportViewer.ToolPanelView = CrystalDecisions.Windows.Forms.ToolPanelViewType.None;
			}
		}
	}

	private void txtFindText_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
	{
		if(e.KeyCode == Keys.Enter)
		{
			ReportViewer.SearchForText(txtFindText.Text);
			e.Handled = true;
		}
	}

	#region Events
	protected virtual void OnReportRefreshRequested() 
	{
		if (ReportRefreshRequested != null) 
		{
			//Invokes the delegates.
			ReportRefreshRequested(this, EventArgs.Empty); 
		}
	}
	#endregion

	private void mnuZoom400_Click(object sender, System.EventArgs e)
	{
		if(ReportViewer != null) ReportViewer.Zoom(400);
	}

	private void mnuZoom300_Click(object sender, System.EventArgs e)
	{
		if(ReportViewer != null) ReportViewer.Zoom(300);
	}

	private void mnuZoom200_Click(object sender, System.EventArgs e)
	{
		if(ReportViewer != null) ReportViewer.Zoom(200);
	}

	private void mnuZoom150_Click(object sender, System.EventArgs e)
	{
		if(ReportViewer != null) ReportViewer.Zoom(150);
	}

	private void mnuZoom100_Click(object sender, System.EventArgs e)
	{
		if(ReportViewer != null) ReportViewer.Zoom(100);
	}

	private void mnuZoom75_Click(object sender, System.EventArgs e)
	{
		if(ReportViewer != null) ReportViewer.Zoom(75);
	}

	private void mnuZoom50_Click(object sender, System.EventArgs e)
	{
		if(ReportViewer != null) ReportViewer.Zoom(50);
	}

	private void mnuZoom25_Click(object sender, System.EventArgs e)
	{
		if(ReportViewer != null) ReportViewer.Zoom(25);
	}

	private void mnuZoomPageWidth_Click(object sender, System.EventArgs e)
	{
		if(ReportViewer != null) ReportViewer.Zoom(1);
	}

	private void mnuZoomWholePage_Click(object sender, System.EventArgs e)
	{
		if(ReportViewer != null) ReportViewer.Zoom(2);
	}

	private void txtFindText_Enter(object sender, System.EventArgs e)
	{
		if(_searchEmpty)
		{
			txtFindText.Text = "";
			txtFindText.ForeColor = SystemColors.WindowText;
		}
	}

	private void txtFindText_TextChanged(object sender, System.EventArgs e)
	{
		_searchEmpty = false;
	}

	private void txtFindText_Leave(object sender, System.EventArgs e)
	{
		if(txtFindText.Text == "")
		{
			txtFindText.ForeColor = SystemColors.GrayText;
			txtFindText.Text = ResourceUtils.GetString("TextToSearch");
			_searchEmpty = true;
		}
	}

	private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
	{
		if(e.Button == btnFind & _searchEmpty == false)
		{
			ReportViewer.SearchForText(txtFindText.Text);
		}
	}

	public bool ProcessCommand(ref Message msg, Keys keyData)
	{
		if(keyData == (Keys.Control | Keys.P))
		{
			_crViewer.PrintReport();

			return true;
		}

		if(keyData == (Keys.Control | Keys.E))
		{
			_crViewer.ExportReport();

			return true;
		}

		if(keyData == (Keys.Control | Keys.F))
		{
			txtFindText.Focus();

			return true;
		}

		return false;
	}

	private void value_Navigate(object source, CrystalDecisions.Windows.Forms.NavigateEventArgs e)
	{
		UpdateNavigationButtons(e.NewPageNumber, true);
	}

	private void UpdateNavigationButtons(int pageNumber, bool showNextLast)
	{
		btnFirst.Enabled = (pageNumber > 1);
		btnPrevious.Enabled = (pageNumber > 1);
		btnNext.Enabled = showNextLast;
		btnLast.Enabled = showNextLast;
	}
}