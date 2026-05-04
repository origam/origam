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
        object objectUnderMouse = gViewer.GetObjectAt(point: mouseRightButtonDownPoint);
        if (objectUnderMouse is DEdge edge)
        {
            return CreateContextMenuForEdge(edge: edge);
        }
        if (objectUnderMouse is DNode node)
        {
            return CreateContextMenuForNode(dNodeUnderMouse: node);
        }
        return new ContextMenuStrip();
    }

    private ContextMenuStrip CreateContextMenuForEdge(DEdge edge)
    {
        if (edge.Edge.UserData == null)
        {
            return new ContextMenuStrip();
        }

        var deleteMenuItem = new ToolStripMenuItem();
        deleteMenuItem.Text = Strings.WorkFlowDiagramEditor_ContextMenuEdge_Delete;
        deleteMenuItem.Image = ImageRes.icon_delete;
        deleteMenuItem.Click += (sender, args) =>
            gViewer.RemoveEdge(edge: edge, registerForUndo: true);

        ToolStripMenuItem addBetweenMenu = new ToolStripMenuItem(
            text: Strings.WorkFlowDiagramEditor_ContextMenuEdge_Add_Between
        );
        addBetweenMenu.Image = ImageRes.icon_new;
        var builder = new SchemaItemEditorsMenuBuilder(showDialog: true);
        var dependency = (WorkflowTaskDependency)edge.Edge.UserData;
        var targetStep = (IWorkflowStep)dependency.ParentItem;
        var parentBlock = targetStep.ParentItem;
        var submenuItems = builder.BuildSubmenu(owner: parentBlock);
        foreach (AsMenuCommand submenuItem in submenuItems)
        {
            if (!(submenuItem.Command is AddNewSchemaItem addNewCommand))
            {
                continue;
            }
            addNewCommand.ItemCreated += (sender, newItem) =>
            {
                ISchemaItem sourceItem = RetrieveItem(strId: edge.Edge.Source);
                ISchemaItem targetItem = RetrieveItem(strId: edge.Edge.Target);
                taskRunner.AddDependencyTask(
                    independentItem: (IWorkflowStep)newItem,
                    dependentItem: targetItem,
                    triggerItemId: newItem.Id
                );
                taskRunner.AddDependencyTask(
                    independentItem: (IWorkflowStep)sourceItem,
                    dependentItem: newItem,
                    triggerItemId: newItem.Id
                );
                var existingDependency = (WorkflowTaskDependency)edge.Edge.UserData;
                if (existingDependency.PackageName == newItem.PackageName)
                {
                    taskRunner.RemoveDependencyTask(
                        dependency: existingDependency,
                        triggerItemId: newItem.Id
                    );
                }
            };
        }
        addBetweenMenu.DropDownItems.AddRange(toolStripItems: submenuItems);

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(value: deleteMenuItem);
        contextMenu.Items.Add(value: addBetweenMenu);
        return contextMenu;
    }

    private ContextMenuStrip CreateContextMenuForNode(DNode dNodeUnderMouse)
    {
        var contextMenu = new AsContextMenu(caller: WorkbenchSingleton.Workbench);
        var schemaItemUnderMouse = RetrieveItem(node: dNodeUnderMouse.Node);
        if (
            IsObjectSelectionInconsistent(schemaItemUnderMouse: schemaItemUnderMouse)
            || schemaItemUnderMouse is EntityUIAction
            || schemaItemUnderMouse is DataStructureEntity
        )
        {
            return contextMenu;
        }

        var deleteMenuItem = MakeDeleteItem(
            dNodeUnderMouse: dNodeUnderMouse,
            schemaItemUnderMouse: schemaItemUnderMouse
        );
        contextMenu.AddSubItem(subItem: deleteMenuItem);
        if (schemaItemUnderMouse is IContextStore contextStore)
        {
            ToolStripMenuItem hideDataFlowItem = MakeHideDatFlowItem(contextStore: contextStore);
            ToolStripMenuItem showDataFlowItem = MakeShowDataFlowItem(
                nodeUnderMouse: dNodeUnderMouse.Node,
                contextStore: contextStore
            );
            contextMenu.AddSubItem(subItem: hideDataFlowItem);
            contextMenu.AddSubItem(subItem: showDataFlowItem);
        }
        var newMenu = MakeNewItem(
            dNodeUnderMouse: dNodeUnderMouse,
            schemaItemUnderMouse: schemaItemUnderMouse
        );
        if (newMenu.DropDownItems.Count > 0)
        {
            contextMenu.AddSubItem(subItem: newMenu);
        }

        var actionsMenu = MakeActionsItem(schemaItemUnderMouse: schemaItemUnderMouse);
        if (actionsMenu.DropDownItems.Count > 0)
        {
            contextMenu.AddSubItem(subItem: actionsMenu);
        }
        if (Graph.IsWorkFlowItemSubGraph(node: dNodeUnderMouse.Node))
        {
            ToolStripMenuItem addAfterMenu = MakeAddAfterItem(
                dNodeUnderMouse: dNodeUnderMouse,
                schemaItemUnderMouse: schemaItemUnderMouse
            );
            contextMenu.AddSubItem(subItem: addAfterMenu);
        }
        return contextMenu;
    }

    private bool IsObjectSelectionInconsistent(ISchemaItem schemaItemUnderMouse)
    {
        bool objectSelectionIsInconsistent =
            schemaItemUnderMouse != null
            && (
                schemaItemUnderMouse.Id != nodeSelector.SelectedNodeId
                || schemaItemUnderMouse.Id != Guid.Parse(input: schemaService.ActiveNode.NodeId)
            );
        return objectSelectionIsInconsistent;
    }

    private bool IsDeleteMenuItemAvailable(DNode objectUnderMouse, ISchemaItem schemaItemUnderMouse)
    {
        if (objectUnderMouse == null)
        {
            return false;
        }

        if (Equals(objA: nodeSelector.Selected, objB: Graph.MainDrawingSubgraf))
        {
            return false;
        }

        if (schemaItemUnderMouse?.Package?.Id != schemaService.ActiveSchemaExtensionId)
        {
            return false;
        }

        return Equals(objA: objectUnderMouse.Node, objB: nodeSelector.Selected);
    }

    private bool IsAddAfterMenuItemAvailable(DNode objectUnderMouse)
    {
        if (objectUnderMouse == null)
        {
            return false;
        }

        if (Equals(objA: nodeSelector.Selected, objB: Graph.MainDrawingSubgraf))
        {
            return false;
        }

        return Equals(objA: objectUnderMouse.Node, objB: nodeSelector.Selected);
    }

    private bool IsNewMenuAvailable(DNode dNodeUnderMouse)
    {
        if (
            dNodeUnderMouse?.Node is Subgraph
            && nodeSelector.Selected is Subgraph
            && !Graph.IsWorkFlowItemSubGraph(node: dNodeUnderMouse.Node)
            && !Graph.IsWorkFlowItemSubGraph(node: nodeSelector.Selected)
        )
        {
            return true;
        }
        if (dNodeUnderMouse == null)
        {
            return false;
        }

        var schemaItem = dNodeUnderMouse.Node is InfrastructureSubgraph infrastructureGraph
            ? RetrieveItem(id: infrastructureGraph.WorkflowItemId)
            : RetrieveItem(node: dNodeUnderMouse.Node);
        if (!(dNodeUnderMouse.Node is Subgraph) && !(schemaItem is ServiceMethodCallParameter))
        {
            return false;
        }

        Guid nodeId = IdTranslator.ToSchemaId(node: dNodeUnderMouse.Node);
        return nodeId == nodeSelector.SelectedNodeId;
    }

    private ToolStripMenuItem MakeAddAfterItem(
        DNode dNodeUnderMouse,
        ISchemaItem schemaItemUnderMouse
    )
    {
        ToolStripMenuItem addAfterMenu = new ToolStripMenuItem(
            text: Strings.WorkFlowDiagramEditor_ContextMenuNode_Add_After
        );
        addAfterMenu.Image = ImageRes.icon_new;
        var builder = new SchemaItemEditorsMenuBuilder(showDialog: true);
        var submenuItems = builder.BuildSubmenu(owner: schemaItemUnderMouse.ParentItem);
        foreach (AsMenuCommand submenuItem in submenuItems)
        {
            if (
                !(submenuItem.Command is AddNewSchemaItem addNewCommand)
                || !(schemaItemUnderMouse is IWorkflowStep workflowStep)
            )
            {
                continue;
            }
            addNewCommand.ItemCreated += (sender, item) =>
            {
                taskRunner.AddDependencyTask(
                    independentItem: workflowStep,
                    dependentItem: item,
                    triggerItemId: item.Id
                );
            };
        }
        addAfterMenu.DropDownItems.AddRange(toolStripItems: submenuItems);
        addAfterMenu.Enabled = IsAddAfterMenuItemAvailable(objectUnderMouse: dNodeUnderMouse);
        return addAfterMenu;
    }

    private static ToolStripMenuItem MakeActionsItem(ISchemaItem schemaItemUnderMouse)
    {
        ToolStripMenuItem actionsMenu = new ToolStripMenuItem(
            text: Strings.WorkFlowDiagramEditor_ContextMenuNode_Actions
        );
        actionsMenu.Image = ImageRes.icon_actions;
        AsMenuCommand dummyMenu = new AsMenuCommand(label: "dummy", caller: schemaItemUnderMouse);
        var builder1 = new SchemaActionsMenuBuilder();
        dummyMenu.PopulateMenu(item: builder1);
        actionsMenu.DropDownItems.AddRange(
            toolStripItems: dummyMenu.DropDownItems.ToArray<ToolStripItem>()
        );
        return actionsMenu;
    }

    private ToolStripMenuItem MakeNewItem(DNode dNodeUnderMouse, ISchemaItem schemaItemUnderMouse)
    {
        ToolStripMenuItem newMenu = new ToolStripMenuItem(
            text: Strings.WorkFlowDiagramEditor_ContextMenuNode_New
        );
        newMenu.Image = ImageRes.icon_new;
        newMenu.Enabled = IsNewMenuAvailable(dNodeUnderMouse: dNodeUnderMouse);
        if (schemaItemUnderMouse is ServiceMethodCallTask)
        {
            schemaItemUnderMouse
                .ChildItems.Where(predicate: item => !(item is WorkflowTaskDependency))
                .ForEach(action: schemaItem =>
                {
                    var menuItem = new AsMenuCommand(label: schemaItem.Name, caller: schemaItem);
                    var builder = new SchemaItemEditorsMenuBuilder(showDialog: true);
                    menuItem.PopulateMenu(item: builder);
                    newMenu.DropDownItems.Add(value: menuItem);
                });
        }
        else
        {
            AsMenuCommand menuItem = new AsMenuCommand(
                label: Strings.WorkFlowDiagramEditor_ContextMenuNode_New,
                caller: schemaItemUnderMouse
            );
            var builder = new SchemaItemEditorsMenuBuilder(showDialog: true);
            menuItem.PopulateMenu(item: builder);
            menuItem.SubItems.AddRange(collection: menuItem.DropDownItems.Cast<object>());
            new AsContextMenu(caller: WorkbenchSingleton.Workbench).AddSubItem(subItem: menuItem);
            menuItem.ShowDropDown();
            newMenu.DropDownItems.AddRange(
                toolStripItems: menuItem.DropDownItems.ToArray<ToolStripItem>()
            );
        }
        return newMenu;
    }

    private ToolStripMenuItem MakeShowDataFlowItem(Node nodeUnderMouse, IContextStore contextStore)
    {
        var showDataFlowItem = new ToolStripMenuItem();
        showDataFlowItem.Text = Strings.WorkFlowDiagramEditor_ContextMenuNode_Show_data_flow;
        showDataFlowItem.Image = ImageRes.icon_execute;
        showDataFlowItem.Click += (sender, args) =>
        {
            dependencyPainter.Activate(contextStore: contextStore);
            nodeSelector.Selected = nodeUnderMouse;
            ReDrawAndKeepFocus();
        };
        showDataFlowItem.Enabled = !contextStore.Equals(obj: dependencyPainter.CurrentContextStore);
        return showDataFlowItem;
    }

    private ToolStripMenuItem MakeHideDatFlowItem(IContextStore contextStore)
    {
        var hideDataFlowItem = new ToolStripMenuItem();
        hideDataFlowItem.Text = Strings.WorkFlowDiagramEditor_ContextMenuNode_Hide_data_flow;
        hideDataFlowItem.Image = ImageRes.icon_delete;
        hideDataFlowItem.Click += (sender, args) => dependencyPainter.DeActivate();
        hideDataFlowItem.Enabled = contextStore.Equals(obj: dependencyPainter.CurrentContextStore);
        return hideDataFlowItem;
    }

    private ToolStripMenuItem MakeDeleteItem(
        DNode dNodeUnderMouse,
        ISchemaItem schemaItemUnderMouse
    )
    {
        var deleteMenuItem = new ToolStripMenuItem();
        deleteMenuItem.Text = Strings.WorkFlowDiagramEditor_ContextMenuNode_Delete;
        deleteMenuItem.Image = ImageRes.icon_delete;
        deleteMenuItem.Click += DeleteNode_Click;
        deleteMenuItem.Enabled = IsDeleteMenuItemAvailable(
            objectUnderMouse: dNodeUnderMouse,
            schemaItemUnderMouse: schemaItemUnderMouse
        );
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
            throw new Exception(message: Strings.WorkFlowDiagramEditor_Node_Not_Selected);
        }
    }

    private void DeleteActiveNodeWithDependencies()
    {
        Node nodeToDelete = Graph.FindNodeOrSubgraph(
            id: IdTranslator.SchemaToFirstNode(schemaItemId: schemaService.ActiveNode.NodeId)
        );
        var deleteNodeCommand = new DeleteActiveNode();
        deleteNodeCommand.BeforeDelete += (o, args) =>
        {
            nodeToDelete
                .OutEdges.ToArray()
                .Where(predicate: ConnectsSchemaItems)
                .Where(predicate: x => !(x.UserData is IContextStore))
                .ForEach(action: DeleteDependency);
        };
        deleteNodeCommand.AfterDelete += (o, args) =>
        {
            var sourceIds = nodeToDelete.InEdges.Select(selector: edge => edge.Source);
            var targetIds = nodeToDelete.OutEdges.Select(selector: edge => edge.Target);
            foreach (string sourceId in sourceIds)
            {
                if (IdTranslator.NodeToSchema(nodeId: sourceId) == Guid.Empty)
                {
                    continue;
                }

                foreach (string targetId in targetIds)
                {
                    if (IdTranslator.NodeToSchema(nodeId: targetId) == Guid.Empty)
                    {
                        continue;
                    }

                    AddDependency(source: sourceId, target: targetId);
                }
            }
        };
        deleteNodeCommand.Run();
    }
}
