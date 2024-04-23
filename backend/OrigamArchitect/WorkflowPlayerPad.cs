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
using System.Windows.Forms;
using OrigamArchitect;
using Origam.Workbench.Services;
using Origam.Schema.MenuModel;
using Origam.Schema;
using Origam.UI;

namespace Origam.Workbench.Pads;

/// <summary>
/// Summary description for WorkflowPlayerPad.
/// </summary>
public class WorkflowPlayerPad : AbstractPadContent, IBrowserPad
{
	//private System.Windows.Forms.TreeView treeView1;
	private System.ComponentModel.IContainer components = null;
		
	public Origam.Workbench.ExpressionBrowser ebrSchemaBrowser;
	private IDocumentationService _documentationService;

	private void InitializeComponent()
	{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WorkflowPlayerPad));
			this.ebrSchemaBrowser = new Origam.Workbench.ExpressionBrowser();
			this.SuspendLayout();
			// 		// ebrSchemaBrowser
			// 		this.ebrSchemaBrowser.AllowEdit = false;
			this.ebrSchemaBrowser.CheckSecurity = true;
			this.ebrSchemaBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ebrSchemaBrowser.Location = new System.Drawing.Point(0, 0);
			this.ebrSchemaBrowser.Name = "ebrSchemaBrowser";
			this.ebrSchemaBrowser.NodeUnderMouse = null;
			this.ebrSchemaBrowser.ShowFilter = false;
			this.ebrSchemaBrowser.DisableOtherExtensionNodes = false;
			this.ebrSchemaBrowser.Size = new System.Drawing.Size(288, 589);
			this.ebrSchemaBrowser.TabIndex = 3;
			this.ebrSchemaBrowser.NodeUnderMouseChanged += new System.EventHandler(this.ebrSchemaBrowser_NodeUnderMouseChanged);
			this.ebrSchemaBrowser.NodeDoubleClick += new System.EventHandler(this.ebrSchemaBrowser_NodeDoubleClick);
			this.ebrSchemaBrowser.QueryFilterNode += new FilterEventHandler(ebrSchemaBrowser_QueryFilterNode);
			// 		// WorkflowPlayerPad
			// 		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(288, 589);
			this.Controls.Add(this.ebrSchemaBrowser);
			this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft) 
				| WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight) 
				| WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop) 
				| WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
			this.HideOnClose = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "WorkflowPlayerPad";
			this.TabText = strings.ProcessesTabText;
			this.Text = strings.ProcessList;
			this.ResumeLayout(false);

		}
	
		
	public WorkflowPlayerPad()
	{
			InitializeComponent();
		}

	private void ebrSchemaBrowser_NodeDoubleClick(object sender, System.EventArgs e)
	{
			OrigamArchitect.Commands.ExecuteSchemaItem cmd = new OrigamArchitect.Commands.ExecuteSchemaItem();

			cmd.Owner = ebrSchemaBrowser.ActiveNode;

			cmd.Run();
		}
	

	Origam.Schema.MenuModel.Menu _menu;
	public Origam.Schema.MenuModel.Menu OrigamMenu
	{
		get
		{
				return _menu;
			}
		set
		{
				_menu = value;
				this.ebrSchemaBrowser.RemoveAllNodes();

				if(_menu != null)
				{
					this.ebrSchemaBrowser.AddRootNode(_menu);
				}
			}
	}

	public ImageList ImageList
	{
		get
		{
                return ebrSchemaBrowser.imgList;
            }
	}

	private void workflowForm_StatusChanged(object sender, EventArgs e)
	{
			WorkbenchSingleton.Workbench.UpdateToolbar();
		}

	protected override void Dispose(bool disposing)
	{
			if(disposing)
			{
				if(this.components != null)
				{
					this.components.Dispose();
				}
			}

			base.Dispose (disposing);
		}

	private void ebrSchemaBrowser_NodeUnderMouseChanged(object sender, EventArgs e)
	{
			_documentationService = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;

			if(ebrSchemaBrowser.NodeUnderMouse == null)
			{
				ebrSchemaBrowser.SetToolTip(null);
			}
			else
			{
				ISchemaItem item = null;
				if(ebrSchemaBrowser.NodeUnderMouse.Tag is FormReferenceMenuItem)
				{
					item = (ebrSchemaBrowser.NodeUnderMouse.Tag as FormReferenceMenuItem).Screen;
				}
				else if (ebrSchemaBrowser.NodeUnderMouse.Tag is WorkflowReferenceMenuItem)
				{
					item = (ebrSchemaBrowser.NodeUnderMouse.Tag as WorkflowReferenceMenuItem).Workflow;
				}

				if(item != null)
				{
					ebrSchemaBrowser.SetToolTip(_documentationService.GetDocumentation((Guid)item.PrimaryKey["Id"], DocumentationType.USER_LONG_HELP));
				}
				else
				{
					ebrSchemaBrowser.SetToolTip(null);
				}
			}
		}

	private void ebrSchemaBrowser_QueryFilterNode(object sender, ExpressionBrowserEventArgs e)
	{
			AbstractMenuItem menu = e.QueriedObject as AbstractMenuItem;
			Submenu submenu = e.QueriedObject as Submenu;

			if(submenu != null && submenu.IsHidden)
			{
				e.Filter = true;
			}
			else if(menu != null)
			{
				IParameterService param = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
				
				e.Filter = ! param.IsFeatureOn(menu.Features);
			}
		}

	public int ImageIndex(string icon)
	{
	        return this.ImageList.ImageIndex(icon);
        }
}