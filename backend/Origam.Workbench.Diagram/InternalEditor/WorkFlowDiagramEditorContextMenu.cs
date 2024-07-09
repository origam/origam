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
using System.Linq;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using MoreLinq;
using Origam.Extensions;
using Origam.Gui.Win.Commands;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.UI;
using Origam.Workbench.BaseComponents;
using Origam.Workbench.Commands;
using Origam.Workbench.Diagram.Extensions;
using Origam.Workbench.Diagram.Graphs;
using Origam.Workbench.Diagram.NodeDrawing;

namespace Origam.Workbench.Diagram.InternalEditor;
public partial class WorkFlowDiagramEditor : IDiagramEditor
{
    	private ContextMenuStrip BuildContextMenu()
    {
        object objectUnderMouse = gViewer.GetObjectAt(mouseRightButtonDownPoint);
        if (objectUnderMouse is DEdge edge)
        {
	        return CreateContextMenuForEdge(edge);
        }
        if(objectUnderMouse is DNode node)
        {
	        return CreateContextMenuForNode(node);
        } 
        return  new ContextMenuStrip();
    }
	
	private ContextMenuStrip CreateContextMenuForEdge(DEdge edge)
	{
		if(edge.Edge.UserData == null) return new ContextMenuStrip();
		
		var deleteMenuItem = new ToolStripMenuItem();
		deleteMenuItem.Text = Strings.WorkFlowDiagramEditor_ContextMenuEdge_Delete;
		deleteMenuItem.Image = ImageRes.icon_delete;
		deleteMenuItem.Click += (sender, args) => gViewer.RemoveEdge(edge, true);
	        
		ToolStripMenuItem addBetweenMenu = new ToolStripMenuItem(Strings.WorkFlowDiagramEditor_ContextMenuEdge_Add_Between);
		addBetweenMenu.Image = ImageRes.icon_new;
		var builder = new SchemaItemEditorsMenuBuilder(true);
		var dependency = (WorkflowTaskDependency)edge.Edge.UserData;
		var targetStep = (IWorkflowStep)dependency.ParentItem;
		var parentBlock = targetStep.ParentItem;
		var submenuItems = builder.BuildSubmenu(parentBlock);
		foreach (AsMenuCommand submenuItem in submenuItems)
        {
         	if (!(submenuItem.Command is AddNewSchemaItem addNewCommand))
         	{
         		continue;
         	}
            addNewCommand.ItemCreated += (sender, newItem) =>
         	{ 
                ISchemaItem sourceItem = RetrieveItem(edge.Edge.Source);
                ISchemaItem targetItem = RetrieveItem(edge.Edge.Target);
                taskRunner.AddDependencyTask(
	                independentItem: (IWorkflowStep)newItem, 
	                dependentItem: targetItem, 
	                triggerItemId: newItem.Id);
                taskRunner.AddDependencyTask(
	                independentItem: (IWorkflowStep) sourceItem,
	                dependentItem: newItem,
	                triggerItemId: newItem.Id);
                var existingDependency = (WorkflowTaskDependency)edge.Edge.UserData;
                if (existingDependency.PackageName == newItem.PackageName)
                {
	                taskRunner.RemoveDependencyTask(
		                dependency: existingDependency,
		                triggerItemId: newItem.Id);
                }
            };
        }
		addBetweenMenu.DropDownItems.AddRange(submenuItems);
	
		var contextMenu = new ContextMenuStrip();
		contextMenu.Items.Add(deleteMenuItem);
		contextMenu.Items.Add(addBetweenMenu);
		return contextMenu;
	}
	private ContextMenuStrip CreateContextMenuForNode(DNode dNodeUnderMouse)
	{
		var contextMenu = new AsContextMenu(WorkbenchSingleton.Workbench);
		var schemaItemUnderMouse = RetrieveItem(dNodeUnderMouse.Node);
		if (IsObjectSelectionInconsistent(schemaItemUnderMouse) ||
		    schemaItemUnderMouse is EntityUIAction ||
		    schemaItemUnderMouse is DataStructureEntity)
		{
			return contextMenu;
		}
		
		var deleteMenuItem = MakeDeleteItem(dNodeUnderMouse, schemaItemUnderMouse);
		contextMenu.AddSubItem(deleteMenuItem);
		if (schemaItemUnderMouse is IContextStore contextStore)
		{
			ToolStripMenuItem hideDataFlowItem = MakeHideDatFlowItem(contextStore);
			ToolStripMenuItem showDataFlowItem = 
				MakeShowDataFlowItem(dNodeUnderMouse.Node, contextStore);
			contextMenu.AddSubItem(hideDataFlowItem);
			contextMenu.AddSubItem(showDataFlowItem);
		}
		var newMenu = MakeNewItem(dNodeUnderMouse, schemaItemUnderMouse);
		if (newMenu.DropDownItems.Count > 0)
		{
			contextMenu.AddSubItem(newMenu);
		}
		
		var actionsMenu = MakeActionsItem(schemaItemUnderMouse);
		if (actionsMenu.DropDownItems.Count > 0)
		{
			contextMenu.AddSubItem(actionsMenu);
		}
		if (Graph.IsWorkFlowItemSubGraph(dNodeUnderMouse.Node))
		{
			ToolStripMenuItem addAfterMenu =
				MakeAddAfterItem(dNodeUnderMouse, schemaItemUnderMouse);
			contextMenu.AddSubItem(addAfterMenu);
		}
		return contextMenu;
	}
	
	private bool IsObjectSelectionInconsistent(
		ISchemaItem schemaItemUnderMouse)
	{
		bool objectSelectionIsInconsistent =
			schemaItemUnderMouse != null &&
			(schemaItemUnderMouse.Id != nodeSelector.SelectedNodeId ||
			 schemaItemUnderMouse.Id !=
			 Guid.Parse(schemaService.ActiveNode.NodeId));
		return objectSelectionIsInconsistent;
	}
	
	private bool IsDeleteMenuItemAvailable(DNode objectUnderMouse,
		ISchemaItem schemaItemUnderMouse)
	{
		if (objectUnderMouse == null) return false;
		if (Equals(nodeSelector.Selected, Graph.MainDrawingSubgraf)) return false;
		if (schemaItemUnderMouse?.Package?.Id !=
		    schemaService.ActiveSchemaExtensionId) return false;
		return Equals(objectUnderMouse.Node, nodeSelector.Selected);
	}
	
	private bool IsAddAfterMenuItemAvailable(DNode objectUnderMouse)
	{
		if (objectUnderMouse == null) return false;
		if (Equals(nodeSelector.Selected, Graph.MainDrawingSubgraf)) return false;
		return Equals(objectUnderMouse.Node, nodeSelector.Selected);
	}
	private bool IsNewMenuAvailable(DNode dNodeUnderMouse)
	{
		if (dNodeUnderMouse?.Node is Subgraph &&
		    nodeSelector.Selected is Subgraph &&
		    !Graph.IsWorkFlowItemSubGraph(dNodeUnderMouse.Node) && 
		    !Graph.IsWorkFlowItemSubGraph(nodeSelector.Selected))
		{
			return true;
		}
		if (dNodeUnderMouse == null) return false;
		var schemaItem = dNodeUnderMouse.Node is InfrastructureSubgraph infrastructureGraph
			? RetrieveItem(infrastructureGraph.WorkflowItemId) 
			: RetrieveItem(dNodeUnderMouse.Node);
		if (!(dNodeUnderMouse.Node is Subgraph) && 
		    !(schemaItem is ServiceMethodCallParameter)) return false;
		Guid nodeId = IdTranslator.ToSchemaId(dNodeUnderMouse.Node);
		return nodeId == nodeSelector.SelectedNodeId;
	}
	private ToolStripMenuItem MakeAddAfterItem(DNode dNodeUnderMouse,
		ISchemaItem schemaItemUnderMouse)
	{
		ToolStripMenuItem addAfterMenu = new ToolStripMenuItem(Strings.WorkFlowDiagramEditor_ContextMenuNode_Add_After);
		addAfterMenu.Image = ImageRes.icon_new;
		var builder = new SchemaItemEditorsMenuBuilder(true);
		var submenuItems = builder.BuildSubmenu(schemaItemUnderMouse.ParentItem);
		foreach (AsMenuCommand submenuItem in submenuItems)
		{
			if (!(submenuItem.Command is AddNewSchemaItem addNewCommand) ||
			    !(schemaItemUnderMouse is IWorkflowStep workflowStep))
			{
				continue;
			}
			addNewCommand.ItemCreated += (sender, item) =>
			{
				taskRunner.AddDependencyTask(
					independentItem: workflowStep,
					dependentItem: item,
					triggerItemId: item.Id);
			};
		}
		addAfterMenu.DropDownItems.AddRange(submenuItems);
		addAfterMenu.Enabled = IsAddAfterMenuItemAvailable(dNodeUnderMouse);
		return addAfterMenu;
	}
	private static ToolStripMenuItem MakeActionsItem(
		ISchemaItem schemaItemUnderMouse)
	{
		ToolStripMenuItem actionsMenu = new ToolStripMenuItem(Strings.WorkFlowDiagramEditor_ContextMenuNode_Actions);
		actionsMenu.Image = ImageRes.icon_actions;
		AsMenuCommand dummyMenu = new AsMenuCommand("dummy", schemaItemUnderMouse);
		var builder1 = new SchemaActionsMenuBuilder();
		dummyMenu.PopulateMenu(builder1);
		actionsMenu.DropDownItems.AddRange(dummyMenu.DropDownItems
			.ToArray<ToolStripItem>());
		return actionsMenu;
	}
	private ToolStripMenuItem MakeNewItem(DNode dNodeUnderMouse,
		ISchemaItem schemaItemUnderMouse)
	{
		ToolStripMenuItem newMenu = new ToolStripMenuItem(Strings.WorkFlowDiagramEditor_ContextMenuNode_New);
		newMenu.Image = ImageRes.icon_new;
		newMenu.Enabled = IsNewMenuAvailable(dNodeUnderMouse);
		if (schemaItemUnderMouse is ServiceMethodCallTask)
		{
			schemaItemUnderMouse.ChildItems
				.Where(item => !(item is WorkflowTaskDependency))
				.ForEach(schemaItem =>
				{
					var menuItem = new AsMenuCommand(schemaItem.Name, schemaItem);
					var builder = new SchemaItemEditorsMenuBuilder(true);
					menuItem.PopulateMenu(builder);
					newMenu.DropDownItems.Add(menuItem);
				});
		}
		else
		{
			AsMenuCommand menuItem = new AsMenuCommand(Strings.WorkFlowDiagramEditor_ContextMenuNode_New, schemaItemUnderMouse);
			var builder = new SchemaItemEditorsMenuBuilder(true);
			menuItem.PopulateMenu(builder);
			menuItem.SubItems.AddRange(menuItem.DropDownItems);
			new AsContextMenu(WorkbenchSingleton.Workbench).AddSubItem(menuItem);
			menuItem.ShowDropDown();
			newMenu.DropDownItems.AddRange(menuItem.DropDownItems
				.ToArray<ToolStripItem>());
		}
		return newMenu;
	}
	
	private ToolStripMenuItem MakeShowDataFlowItem(Node nodeUnderMouse,
		IContextStore contextStore)
	{
		var showDataFlowItem = new ToolStripMenuItem();
		showDataFlowItem.Text = Strings.WorkFlowDiagramEditor_ContextMenuNode_Show_data_flow;
		showDataFlowItem.Image = ImageRes.icon_execute;
		showDataFlowItem.Click += (sender, args) =>
		{
			dependencyPainter.Activate(contextStore);
			nodeSelector.Selected = nodeUnderMouse;
			ReDrawAndKeepFocus();
		};
		showDataFlowItem.Enabled =
			!contextStore.Equals(dependencyPainter.CurrentContextStore);
		return showDataFlowItem;
	}
	private ToolStripMenuItem MakeHideDatFlowItem(IContextStore contextStore)
	{
		var hideDataFlowItem = new ToolStripMenuItem();
		hideDataFlowItem.Text = Strings.WorkFlowDiagramEditor_ContextMenuNode_Hide_data_flow;
		hideDataFlowItem.Image = ImageRes.icon_delete;
		hideDataFlowItem.Click += (sender, args) => dependencyPainter.DeActivate();
		hideDataFlowItem.Enabled = contextStore.Equals(dependencyPainter.CurrentContextStore);
		return hideDataFlowItem;
	}
	private ToolStripMenuItem MakeDeleteItem(DNode dNodeUnderMouse,
		ISchemaItem schemaItemUnderMouse)
	{
		var deleteMenuItem = new ToolStripMenuItem();
		deleteMenuItem.Text = Strings.WorkFlowDiagramEditor_ContextMenuNode_Delete;
		deleteMenuItem.Image = ImageRes.icon_delete;
		deleteMenuItem.Click += DeleteNode_Click;
		deleteMenuItem.Enabled =
			IsDeleteMenuItemAvailable(dNodeUnderMouse, schemaItemUnderMouse);
		return deleteMenuItem;
	}
	
	private void DeleteNode_Click(object sender, EventArgs e)
	{ 
		bool nodeSelected = TrySelectActiveNodeInModelView();
		if (nodeSelected)
		{
			DeleteActiveNodeWithDependencies();
		}
		else
		{
			throw new Exception(Strings.WorkFlowDiagramEditor_Node_Not_Selected);
		}
	}
	private void DeleteActiveNodeWithDependencies()
	{
		Node nodeToDelete =
			Graph.FindNodeOrSubgraph(IdTranslator.SchemaToFirstNode(schemaService.ActiveNode.NodeId));
		var deleteNodeCommand = new DeleteActiveNode();
		deleteNodeCommand.BeforeDelete += (o, args) =>
		{
			nodeToDelete.OutEdges.ToArray()
				.Where(ConnectsSchemaItems)
				.Where(x=> !(x.UserData is IContextStore))
				.ForEach(DeleteDependency);
		};
		deleteNodeCommand.AfterDelete += (o, args) =>
		{
			var sourceIds = nodeToDelete.InEdges.Select(edge => edge.Source);
			var targetIds = nodeToDelete.OutEdges.Select(edge => edge.Target);
			foreach (string sourceId in sourceIds)
			{
				if (IdTranslator.NodeToSchema(sourceId) == Guid.Empty) continue;
				foreach (string targetId in targetIds)
				{
					if (IdTranslator.NodeToSchema(targetId) == Guid.Empty) continue;
					AddDependency(sourceId, targetId);
				}
			}
		};
		deleteNodeCommand.Run();
	}
}
