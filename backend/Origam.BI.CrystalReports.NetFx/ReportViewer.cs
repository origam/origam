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
using System.Collections;
using System.Windows.Forms;

using Origam.UI;
using Origam.Workbench;
using Origam.Schema.GuiModel;

namespace Origam.BI.CrystalReports;

/// <summary>
/// Summary description for ReportViewer.
/// </summary>
public class ReportViewer : AbstractViewContent
{
	private CrystalDecisions.Windows.Forms.CrystalReportViewer crViewer;
	private Origam.BI.CrystalReports.ReportToolbar pnlToolbar;
	private CrystalReport _reportElement;

	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public ReportViewer()
	{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.TitleNameChanged += new EventHandler(ReportViewer_TitleNameChanged);

			this.BackColor = OrigamColorScheme.FormBackgroundColor;
			this.crViewer.BackColor = this.BackColor;
			
		}

	public ReportViewer(CrystalReport reportElement, string titleName) : this()
	{
			_reportElement = reportElement;
			this.TitleName = titleName;
			pnlToolbar.ShowRefreshButton = true;
			pnlToolbar.ReportRefreshRequested += new EventHandler(pnlToolbar_ReportRefreshRequested);
		}

	public ReportViewer(CrystalReport reportElement, string titleName, Hashtable parameters) : this(reportElement, titleName)
	{
			foreach(DictionaryEntry entry in parameters)
			{
				_parameters.Add(entry.Key, entry.Value);
			}
		}

	public override bool CanRefreshContent
	{
		get
		{
				return (_reportElement != null);
			}
		set
		{
				base.CanRefreshContent = value;
			}
	}

	public override void RefreshContent()
	{
			try
			{
				Cursor.Current = Cursors.WaitCursor;

				if(_reportElement != null)
				{
					if(_reportElement.DataStructure == null)
					{
						// crViewer.RefreshReport(); - cannot do this, CR will require to enter all parameters, even default parameters
						LoadReport();
					}
					else
					{
						LoadReport();
					}
				}
			}
			catch(Exception ex)
			{
				Origam.UI.AsMessageBox.ShowError(this, ex.Message,
                    Origam.BI.CrystalReports.ResourceUtils.GetString("ErrorReportUpdate"), ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

	public void LoadReport()
	{
			Cursor currentCursor = Cursor.Current;

			try
			{
				Cursor.Current = Cursors.WaitCursor;

				CrystalReportHelper helper = new CrystalReportHelper();

				this.ReportSource = helper.CreateReport(_reportElement.Id, _parameters, null);
				this.crViewer.Zoom(1);
			}
			finally
			{
				Cursor.Current = currentCursor;
			}
		}


	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
			if( disposing )
			{
				crViewer.Dispose();

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ReportViewer));
			this.crViewer = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
			this.pnlToolbar = new Origam.BI.CrystalReports.ReportToolbar();
			this.SuspendLayout();
			// 		// crViewer
			// 		this.crViewer.ActiveViewIndex = -1;
			this.crViewer.ToolPanelView = CrystalDecisions.Windows.Forms.ToolPanelViewType.None;
			this.crViewer.DisplayToolbar = false;
			this.crViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.crViewer.Location = new System.Drawing.Point(0, 24);
			this.crViewer.Name = "crViewer";
			this.crViewer.ReportSource = null;
			this.crViewer.ShowRefreshButton = false;
			this.crViewer.Size = new System.Drawing.Size(712, 445);
			this.crViewer.TabIndex = 0;
			// 		// pnlToolbar
			// 		this.pnlToolbar.Caption = "";
			this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlToolbar.EndColor = System.Drawing.Color.FromArgb(((System.Byte)(254)), ((System.Byte)(225)), ((System.Byte)(122)));
			this.pnlToolbar.Location = new System.Drawing.Point(0, 0);
			this.pnlToolbar.MiddleEndColor = System.Drawing.Color.FromArgb(((System.Byte)(255)), ((System.Byte)(187)), ((System.Byte)(132)));
			this.pnlToolbar.MiddleStartColor = System.Drawing.Color.FromArgb(((System.Byte)(255)), ((System.Byte)(171)), ((System.Byte)(63)));
			this.pnlToolbar.Name = "pnlToolbar";
			this.pnlToolbar.ReportViewer = this.crViewer;
			this.pnlToolbar.ShowRefreshButton = false;
			this.pnlToolbar.Size = new System.Drawing.Size(712, 24);
			this.pnlToolbar.StartColor = System.Drawing.Color.FromArgb(((System.Byte)(255)), ((System.Byte)(217)), ((System.Byte)(170)));
			this.pnlToolbar.TabIndex = 1;
			// 		// ReportViewer
			// 		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(712, 469);
			this.Controls.Add(this.crViewer);
			this.Controls.Add(this.pnlToolbar);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ReportViewer";
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);

		}
	#endregion

	#region Properties
	public object ReportSource
	{
		get
		{
				return crViewer.ReportSource;
			}
		set
		{
				ClearReportSource();

				crViewer.ReportSource = value;
				this.crViewer.Zoom(1);
			}
	}

	private Hashtable _parameters = new Hashtable();
	public Hashtable Parameters
	{
		get
		{
				return _parameters;
			}
	}
	#endregion

	private void ClearReportSource()
	{
			IDisposable rs = this.crViewer.ReportSource as IDisposable;
				
			if(rs != null)
			{
				// sometimes this throws an exception
				try
				{
					this.crViewer.ReportSource = null;
				}
				catch
				{
				}

				rs.Dispose();
			}
		}

	private void ReportViewer_TitleNameChanged(object sender, EventArgs e)
	{
			pnlToolbar.Caption = this.TitleName;
		}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
			if(pnlToolbar.ProcessCommand (ref msg, keyData))
			{
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

	private void pnlToolbar_ReportRefreshRequested(object sender, EventArgs e)
	{
			RefreshContent();
		}
}