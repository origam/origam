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

using System.Collections;
using System.Windows.Forms;
using Origam.Workbench;
using Origam.Schema.WorkflowModel;

namespace Origam.Workflow.Gui.Win;
/// <summary>
/// Summary description for WorkflowWatch.
/// </summary>
public class WorkflowWatchPad : AbstractPadContent
{
	private System.Windows.Forms.ToolBar toolbar;
	private System.Windows.Forms.TreeView tvwWorkflows;
	private System.Windows.Forms.ImageList imageList1;
	private System.Windows.Forms.ToolBarButton btnRefresh;
	private System.ComponentModel.IContainer components;
	public WorkflowWatchPad()
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkflowWatchPad));
        this.toolbar = new System.Windows.Forms.ToolBar();
        this.btnRefresh = new System.Windows.Forms.ToolBarButton();
        this.imageList1 = new System.Windows.Forms.ImageList(this.components);
        this.tvwWorkflows = new System.Windows.Forms.TreeView();
        this.SuspendLayout();
        // 
        // toolbar
        // 
        this.toolbar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
        this.toolbar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
        this.btnRefresh});
        this.toolbar.DropDownArrows = true;
        this.toolbar.ImageList = this.imageList1;
        this.toolbar.Location = new System.Drawing.Point(0, 0);
        this.toolbar.Name = "toolbar";
        this.toolbar.ShowToolTips = true;
        this.toolbar.Size = new System.Drawing.Size(284, 28);
        this.toolbar.TabIndex = 0;
        this.toolbar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolbar_ButtonClick);
        // 
        // btnRefresh
        // 
        this.btnRefresh.ImageIndex = 0;
        this.btnRefresh.Name = "btnRefresh";
        // 
        // imageList1
        // 
        this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
        this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
        this.imageList1.Images.SetKeyName(0, "");
        // 
        // tvwWorkflows
        // 
        this.tvwWorkflows.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tvwWorkflows.Location = new System.Drawing.Point(0, 28);
        this.tvwWorkflows.Name = "tvwWorkflows";
        this.tvwWorkflows.Size = new System.Drawing.Size(284, 236);
        this.tvwWorkflows.Sorted = true;
        this.tvwWorkflows.TabIndex = 1;
        // 
        // WorkflowWatchPad
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(284, 264);
        this.Controls.Add(this.tvwWorkflows);
        this.Controls.Add(this.toolbar);
        this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft) 
        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight) 
        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop) 
        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
        this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        this.HideOnClose = true;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "WorkflowWatchPad";
        this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
        this.TabText = "Workflow Watch";
        this.Text = "Workflow Watch";
        this.ResumeLayout(false);
        this.PerformLayout();
	}
	#endregion
	private void toolbar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
	{
		if(e.Button == btnRefresh)
		{
			RenderTree(WorkflowHost.DefaultHost);
		}
	}
	private void RenderTree(WorkflowHost host)
	{
		tvwWorkflows.BeginUpdate();
		try
		{
			tvwWorkflows.Nodes.Clear();
			TreeNode hostNode = RenderHostNode(host);
			tvwWorkflows.Nodes.Add(hostNode);
			foreach(WorkflowEngine engine in host.RunningWorkflows)
			{
				if(engine.CallingWorkflow == null)
				{
					hostNode.Nodes.Add(RenderWorkflowEngineNode(engine));
				}
			}
		}
		finally
		{
			tvwWorkflows.EndUpdate();
			tvwWorkflows.ExpandAll();
		}
	}
	private TreeNode RenderHostNode(WorkflowHost host)
	{
		return new TreeNode("DefaultHost");
	}
	private TreeNode RenderWorkflowEngineNode(WorkflowEngine engine)
	{
		TreeNode result = new TreeNode(engine.WorkflowBlock.Name + "(" + engine.WorkflowInstanceId + ")");
		TreeNode childWorkflows = new TreeNode("Child Workflows");
		foreach(WorkflowEngine childEngine in engine.Host.RunningWorkflows)
		{
			if(childEngine.CallingWorkflow == engine)
			{
				childWorkflows.Nodes.Add(RenderWorkflowEngineNode(childEngine));
			}
		}
		if(childWorkflows.Nodes.Count > 0) 
		{
			result.Nodes.Add(childWorkflows);
		}
		TreeNode tasks = new TreeNode("Tasks");
		foreach(DictionaryEntry entry in engine.TaskResults)
		{
			IWorkflowStep step = engine.Step(entry.Key as Key);
			tasks.Nodes.Add(RenderWorkflowTask(engine, step, (WorkflowStepResult)entry.Value));
		}
		result.Nodes.Add(tasks);
		return result;
	}
	private TreeNode RenderWorkflowTask(WorkflowEngine engine, IWorkflowStep step, WorkflowStepResult status)
	{
		string iteration = "";
		if(engine.IterationTotal > 0)
		{
			iteration = engine.IterationNumber.ToString() + "/" + engine.IterationTotal.ToString();
		}
		TreeNode result = new TreeNode(step.Name + " " + iteration + " (" + status.ToString() + ")");
		return result;
	}
}
