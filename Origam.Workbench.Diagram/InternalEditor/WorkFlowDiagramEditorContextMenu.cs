using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using MoreLinq.Extensions;
using Origam.Extensions;
using Origam.Gui.Win.Commands;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.UI;
using Origam.Workbench.BaseComponents;
using Origam.Workbench.Commands;
using Origam.Workbench.Diagram.Extensions;

namespace Origam.Workbench.Diagram.InternalEditor
{
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
			deleteMenuItem.Text = "Delete";
			deleteMenuItem.Image = ImageRes.icon_delete;
			deleteMenuItem.Click += (sender, args) => gViewer.RemoveEdge(edge, true);
		        
			ToolStripMenuItem addBetweenMenu = new ToolStripMenuItem("Add Between");
			addBetweenMenu.Image = ImageRes.icon_new;
			var builder = new SchemaItemEditorsMenuBuilder(true);
			var submenuItems = builder.BuildSubmenu(UpToDateGraphParent);
			foreach (AsMenuCommand submenuItem in submenuItems)
            {
             	if (!(submenuItem.Command is AddNewSchemaItem addNewCommand))
             	{
             		continue;
             	}
                addNewCommand.ItemCreated += (sender, newItem) =>
             	{ 
	                AbstractSchemaItem sourceItem = RetrieveItem(edge.Edge.Source);
	                AbstractSchemaItem targetItem = RetrieveItem(edge.Edge.Target);

	                taskRunner.AddDependencyTask(
		                independentItem: (IWorkflowStep)newItem, 
		                dependentItem: targetItem, 
		                triggerItemId: newItem.Id);

	                taskRunner.AddDependencyTask(
		                independentItem: (IWorkflowStep) sourceItem,
		                dependentItem: newItem,
		                triggerItemId: newItem.Id);
	                
	                taskRunner.RemoveDependencyTask(
		               dependency: (WorkflowTaskDependency)edge.Edge.UserData, 
		               triggerItemId: newItem.Id);
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
			    schemaItemUnderMouse is EntityUIAction)
			{
				return contextMenu;
			}
			
			var deleteMenuItem = MakeDeleteItem(dNodeUnderMouse, schemaItemUnderMouse);
			contextMenu.AddSubItem(deleteMenuItem);

			if (schemaItemUnderMouse is IContextStore)
			{
				var hideDataFlowItem = MakeHideDatFlowItem();
				contextMenu.AddSubItem(hideDataFlowItem);
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
				var addAfterMenu = MakeAddAfterItem(dNodeUnderMouse, schemaItemUnderMouse);
				contextMenu.AddSubItem(addAfterMenu);
			}

			return contextMenu;
		}
		
		private bool IsObjectSelectionInconsistent(
			AbstractSchemaItem schemaItemUnderMouse)
		{
			bool objectSelectionIsInconsistent =
				schemaItemUnderMouse != null &&
				(schemaItemUnderMouse.Id != nodeSelector.SelectedNodeId ||
				 schemaItemUnderMouse.Id !=
				 Guid.Parse(schemaService.ActiveNode.NodeId));
			return objectSelectionIsInconsistent;
		}
		
		private bool IsDeleteMenuItemAvailable(DNode objectUnderMouse,
			AbstractSchemaItem schemaItemUnderMouse)
		{
			if (objectUnderMouse == null) return false;
			if (Equals(nodeSelector.Selected, Graph.MainDrawingSubgraf)) return false;
			if (schemaItemUnderMouse?.SchemaExtension?.Id !=
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
			var schemaItem = RetrieveItem(dNodeUnderMouse.Node);
			if (!(dNodeUnderMouse.Node is Subgraph) && 
			    !(schemaItem is ServiceMethodCallParameter)) return false;
			return Equals(dNodeUnderMouse.Node, nodeSelector.Selected);
		}

		private ToolStripMenuItem MakeAddAfterItem(DNode dNodeUnderMouse,
			AbstractSchemaItem schemaItemUnderMouse)
		{
			ToolStripMenuItem addAfterMenu = new ToolStripMenuItem("Add After");
			addAfterMenu.Image = ImageRes.icon_new;
			var builder = new SchemaItemEditorsMenuBuilder(true);
			var submenuItems = builder.BuildSubmenu(UpToDateGraphParent);
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
			AbstractSchemaItem schemaItemUnderMouse)
		{
			ToolStripMenuItem actionsMenu = new ToolStripMenuItem("Actions");
			actionsMenu.Image = ImageRes.icon_actions;
			AsMenuCommand dummyMenu = new AsMenuCommand("dummy", schemaItemUnderMouse);
			var builder1 = new SchemaActionsMenuBuilder();
			dummyMenu.PopulateMenu(builder1);
			actionsMenu.DropDownItems.AddRange(dummyMenu.DropDownItems
				.ToArray<ToolStripItem>());
			return actionsMenu;
		}

		private ToolStripMenuItem MakeNewItem(DNode dNodeUnderMouse,
			AbstractSchemaItem schemaItemUnderMouse)
		{
			ToolStripMenuItem newMenu = new ToolStripMenuItem("New");
			newMenu.Image = ImageRes.icon_new;
			newMenu.Enabled = IsNewMenuAvailable(dNodeUnderMouse);

			if (schemaItemUnderMouse is ServiceMethodCallTask)
			{
				schemaItemUnderMouse.ChildItems
					.ToGeneric()
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
				AsMenuCommand menuItem = new AsMenuCommand("New", schemaItemUnderMouse);
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

		private ToolStripMenuItem MakeHideDatFlowItem()
		{
			var hideDataFlowItem = new ToolStripMenuItem();
			hideDataFlowItem.Text = "Hide data flow";
			hideDataFlowItem.Image = ImageRes.icon_delete;
			hideDataFlowItem.Click += (sender, args) => dependencyPainter.DeActivate();
			hideDataFlowItem.Enabled = dependencyPainter.IsActive;
			return hideDataFlowItem;
		}

		private ToolStripMenuItem MakeDeleteItem(DNode dNodeUnderMouse,
			AbstractSchemaItem schemaItemUnderMouse)
		{
			var deleteMenuItem = new ToolStripMenuItem();
			deleteMenuItem.Text = "Delete";
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
				throw new Exception("Could not delete node because it is not selected in the tree.");
			}
		}

		private void DeleteActiveNodeWithDependencies()
		{
			Node nodeToDelete =
				Graph.FindNodeOrSubgraph(schemaService.ActiveNode.NodeId);

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
					if (!Guid.TryParse(sourceId, out _)) continue;
					foreach (string targetId in targetIds)
					{
						if (!Guid.TryParse(targetId, out _)) continue;
						AddDependency(sourceId, targetId);
					}
				}
			};
			deleteNodeCommand.Run();
		}

    }
}