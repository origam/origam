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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Commands;
using Origam.Workbench.Diagram.Extensions;
using Origam.Workbench.Diagram.Graphs;
using Origam.Workbench.Diagram.NodeDrawing;
using Origam.Workbench.Services;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Origam.Workbench.Diagram.InternalEditor;

public partial class WorkFlowDiagramEditor : IDiagramEditor
{
    private readonly WorkFlowDiagramFactory factory;
    private readonly GViewer gViewer;
    private readonly Form parentForm;
    private System.Drawing.Point mouseRightButtonDownPoint;
    private readonly IPersistenceProvider persistenceProvider;
    private readonly WorkbenchSchemaService schemaService;
    private readonly Guid graphParentId;
    private readonly EdgeInsertionRule edgeInsertionRule;
    private readonly NodeSelector nodeSelector;
    private readonly DependencyTaskRunner taskRunner;
    private readonly ContextStoreDependencyPainter dependencyPainter;
    private WorkFlowGraph Graph => (WorkFlowGraph)gViewer.Graph;

    public WorkFlowDiagramEditor(
        Guid graphParentId,
        GViewer gViewer,
        Form parentForm,
        IPersistenceProvider persistenceProvider,
        WorkFlowDiagramFactory factory,
        NodeSelector nodeSelector
    )
    {
        (gViewer as IViewer).MouseDown += OnMouseDown;
        schemaService = ServiceManager.Services.GetService<WorkbenchSchemaService>();
        taskRunner = new DependencyTaskRunner(persistenceProvider);
        gViewer.EdgeAdded += OnEdgeAdded;
        gViewer.EdgeRemoved += OnEdgeRemoved;
        edgeInsertionRule = new EdgeInsertionRule(
            viewerToImposeOn: gViewer,
            predicate: (sourceNode, targetNode) =>
            {
                if (IdTranslator.ToSchemaId(sourceNode) == Guid.Empty)
                    return false;
                if (IdTranslator.ToSchemaId(targetNode) == Guid.Empty)
                    return false;
                if (RetrieveItem(sourceNode) is ContextStore)
                    return false;
                if (RetrieveItem(targetNode) is ContextStore)
                    return false;
                var sourcesParent = gViewer.Graph.FindParentSubGraph(sourceNode);
                var targetsParent = gViewer.Graph.FindParentSubGraph(targetNode);
                return Equals(sourcesParent, targetsParent);
            }
        );
        gViewer.DoubleClick += GViewerOnDoubleClick;
        gViewer.EdgeInsertButtonVisible = true;

        this.graphParentId = graphParentId;
        this.gViewer = gViewer;
        this.parentForm = parentForm;
        this.persistenceProvider = persistenceProvider;
        this.factory = factory;
        this.nodeSelector = nodeSelector;

        persistenceProvider.InstancePersisted += OnInstancePersisted;

        dependencyPainter = new ContextStoreDependencyPainter(
            gViewer,
            graphParentItemGetter: () => (ISchemaItem)UpToDateGraphParent
        );

        ReDrawAndKeepFocus();
    }

    public void ReDrawAndKeepFocus()
    {
        var selectedNodeTracker = new NodePositionTracker(gViewer, nodeSelector.Selected?.Id);
        Subgraph parentSubGraph = Graph.FindParentSubGraph(nodeSelector.Selected);
        var parentNodeTracker = new NodePositionTracker(gViewer, parentSubGraph?.Id);

        ReDraw();
        ReSelectNode();
        var nodeTracker = nodeSelector.Selected == null ? parentNodeTracker : selectedNodeTracker;
        nodeTracker.LoadUpdatedState();
        gViewer.Transform = nodeTracker.NodeExists ? nodeTracker.UpdatedTransformation : null;
        gViewer.Invalidate();
    }

    private void ReSelectNode()
    {
        Node nodeToSelect = Graph.FindNodeOrSubgraph(nodeSelector.Selected?.Id);
        if (
            nodeToSelect == null
            && nodeSelector.SelectedNodeId != Guid.Empty
            && UpToDateGraphParent.Id == nodeSelector.SelectedNodeId
        )
        {
            nodeToSelect = Graph.MainDrawingSubgraf;
        }
        nodeSelector.Selected = nodeToSelect;
    }

    private void ReDraw()
    {
        bool expandSelected = nodeSelector.Selected != null && nodeSelector.MarkedForExpansion;
        List<string> nodesToExpand = expandSelected
            ? new List<string> { nodeSelector.Selected?.Id }
            : new List<string>();
        nodesToExpand.AddRange(dependencyPainter.GetNodesToExpand());
        gViewer.Graph = factory.Draw(UpToDateGraphParent, nodesToExpand);
        dependencyPainter.Draw();
        if (dependencyPainter.DidDrawSomeEdges)
        {
            gViewer.Redraw();
        }
    }

    private void GViewerOnDoubleClick(object sender, EventArgs e)
    {
        GViewer viewer = sender as GViewer;
        Guid schemaItemId = Guid.Empty;
        if (viewer.SelectedObject is DrawingNode node)
        {
            schemaItemId = node is IWorkflowSubgraph wfSubgraph
                ? wfSubgraph.WorkflowItemId
                : IdTranslator.NodeToSchema(node.Id);
            if (schemaItemId == Guid.Empty)
                return;
        }
        else if (viewer.SelectedObject is Edge edge)
        {
            Guid? id = (edge.UserData as WorkflowTaskDependency)?.Id;
            if (id == null)
                return;
            schemaItemId = id.Value;
        }
        ShowEditor(schemaItemId);
    }

    private void ShowEditor(Guid schemaItemId)
    {
        ISchemaItem clickedItem = RetrieveItem(schemaItemId);
        if (clickedItem != null)
        {
            EditSchemaItem cmd = new EditSchemaItem { ShowDialog = true, Owner = clickedItem };
            cmd.Run();
        }
    }

    private void TrySelectActiveEdgeInModelView()
    {
        if (gViewer.SelectedObject is Edge edge)
        {
            if (edge.UserData is WorkflowTaskDependency dependencyItem)
            {
                schemaService.SelectItem(dependencyItem);
            }
        }
    }

    private void OnEdgeRemoved(object sender, EventArgs e)
    {
        Edge edge = (Edge)sender;
        if (IsContextStoreDependencyArrow(edge) || !ConnectsSchemaItems(edge))
        {
            return;
        }
        DeleteDependency(edge);
    }

    private bool IsContextStoreDependencyArrow(Edge edge)
    {
        return Graph.AllContextStoreSubgraphs.Any(contextStoreSubgraph =>
            contextStoreSubgraph.Nodes.Contains(edge.SourceNode)
            || contextStoreSubgraph.Nodes.Contains(edge.TargetNode)
        );
    }

    private bool ConnectsSchemaItems(Edge edge)
    {
        return IdTranslator.NodeToSchema(edge.Source) != Guid.Empty
            && IdTranslator.NodeToSchema(edge.Target) != Guid.Empty;
    }

    private void DeleteDependency(Edge edge)
    {
        ISchemaItem dependentItem = RetrieveItem(edge.Target);
        Guid sourceId = IdTranslator.NodeToSchema(edge.Source);
        var workflowTaskDependency = dependentItem
            .ChildItems.OfType<WorkflowTaskDependency>()
            .First(x => x.WorkflowTaskId == sourceId);
        workflowTaskDependency.IsDeleted = true;
        workflowTaskDependency.Persist();
    }

    private void OnEdgeAdded(object sender, EventArgs e)
    {
        Edge edge = (Edge)sender;
        if (IsContextStoreDependencyArrow(edge) || !ConnectsSchemaItems(edge))
        {
            return;
        }
        var workflowTaskDependency = AddDependency(edge.Source, edge.Target);
        edge.UserData = workflowTaskDependency;
    }

    private WorkflowTaskDependency AddDependency(string source, string target)
    {
        var independentItem = RetrieveItem(source) as IWorkflowStep;
        ISchemaItem dependentItem = RetrieveItem(target);
        var workflowTaskDependency = new WorkflowTaskDependency
        {
            SchemaExtensionId = dependentItem.SchemaExtensionId,
            PersistenceProvider = persistenceProvider,
            ParentItem = dependentItem,
            Task = independentItem,
        };
        workflowTaskDependency.Persist();
        return workflowTaskDependency;
    }

    private bool TrySelectActiveNodeInModelView()
    {
        if (gViewer.SelectedObject is Node node)
        {
            Guid schemaItemId = node is IWorkflowSubgraph workflowSubgraph
                ? workflowSubgraph.WorkflowItemId
                : IdTranslator.ToSchemaId(node);
            if (schemaItemId == Guid.Empty)
                return false;
            var schemaItem = RetrieveItem(schemaItemId);
            if (schemaItem != null)
            {
                schemaService.SelectItem(schemaItem);
                Guid activeNodeId = Guid.Parse(schemaService.ActiveNode.NodeId);
                return schemaItemId == activeNodeId;
            }
        }
        return false;
    }

    private void OnInstancePersisted(object sender, IPersistent persistedObject)
    {
        if (!(persistedObject is ISchemaItem persistedSchemaItem))
            return;

        bool childPersisted =
            UpToDateGraphParent?.ChildrenRecursive.Any(x => x.Id == persistedSchemaItem.Id)
            ?? false;

        if (childPersisted)
        {
            UpdateNodeOf(persistedSchemaItem);
            taskRunner.UpdateDependencies(persistedSchemaItem);
            return;
        }

        if (persistedSchemaItem.IsDeleted)
        {
            if (persistedSchemaItem is WorkflowTaskDependency taskDependency)
            {
                RemoveEdge(taskDependency.WorkflowTaskId);
            }
            else
            {
                RemoveNode(persistedSchemaItem.Id);
            }
        }
    }

    private void UpdateNodeOf(ISchemaItem persistedSchemaItem)
    {
        Node node = gViewer.Graph.FindNodeOrSubgraph(
            IdTranslator.SchemaToFirstNode(persistedSchemaItem.Id)
        );
        if (node == null)
        {
            TrySelectParentStep(persistedSchemaItem);
            ReDrawAndKeepFocus();
        }
        else
        {
            node.LabelText = persistedSchemaItem.Name;
            ReDrawAndKeepFocus();
        }
    }

    private void TrySelectParentStep(ISchemaItem persistedSchemaItem)
    {
        if (!(persistedSchemaItem is IWorkflowStep))
        {
            ISchemaItem parentStep = persistedSchemaItem.FirstParentOfType<WorkflowTask>();
            if (parentStep == null)
                return;
            Node stepNode = gViewer.Graph.FindNodeOrSubgraph(
                IdTranslator.SchemaToFirstNode(parentStep.Id)
            );
            nodeSelector.Selected = stepNode;
        }
    }

    private IWorkflowBlock UpToDateGraphParent =>
        persistenceProvider.RetrieveInstance(typeof(ISchemaItem), new Key(graphParentId))
        as IWorkflowBlock;

    private void RemoveEdge(Guid sourceId)
    {
        bool edgeWasRemovedOutsideDiagram = gViewer
            .Graph.RootSubgraph.GetAllNodes()
            .SelectMany(node => node.Edges.Select(edge => edge.Source))
            .Select(IdTranslator.NodeToSchema)
            .Where(id => id != Guid.Empty)
            .Any(targetNodeId => targetNodeId == sourceId);
        if (edgeWasRemovedOutsideDiagram)
        {
            ReDrawAndKeepFocus();
        }
    }

    private void RemoveNode(Guid schemaItemId)
    {
        bool deletedNodeWasSelected = nodeSelector.SelectedNodeId == schemaItemId;
        if (deletedNodeWasSelected)
        {
            nodeSelector.Selected = null;
        }
        Node node = gViewer.Graph.FindNodeOrSubgraph(IdTranslator.SchemaToFirstNode(schemaItemId));
        if (node == null)
        {
            return;
        }

        Subgraph parentSubgraph = gViewer.Graph.FindParentSubGraph(node);
        if (deletedNodeWasSelected)
        {
            nodeSelector.Selected = parentSubgraph;
        }
        gViewer.Graph.RemoveNodeEverywhere(node);
        IViewerNode viewerNode = gViewer.FindViewerNode(node);
        if (viewerNode == null)
            return;

        gViewer.RemoveNode(viewerNode, true);
        bool deletedNodeItem = !(node is Subgraph);
        bool deletedLastNode = !parentSubgraph.Nodes.Any();
        if (deletedLastNode || deletedNodeItem)
        {
            ReDrawAndKeepFocus();
        }
    }

    void OnMouseDown(object sender, MsaglMouseEventArgs e)
    {
        gViewer.SuspendLayout();
        if (gViewer.InsertingEdge)
            return;
        HandleSelectAndRedraw(e);
        TrySelectActiveNodeInModelView();
        TrySelectActiveEdgeInModelView();
        if (e.RightButtonIsPressed && !e.Handled)
        {
            mouseRightButtonDownPoint = new System.Drawing.Point(e.X, e.Y);
            ContextMenuStrip cm = BuildContextMenu();
            cm.Show(parentForm, mouseRightButtonDownPoint);
        }
        gViewer.ResumeLayout();
    }

    private void HandleSelectAndRedraw(MsaglMouseEventArgs eventArgs)
    {
        gViewer.Hit(new MouseEventArgs(MouseButtons.None, 0, eventArgs.X, eventArgs.Y, 0));
        if (gViewer.SelectedObject is Node node)
        {
            if (Equals(nodeSelector.Selected, node))
            {
                if (nodeSelector.MarkedForExpansion == false && eventArgs.LeftButtonIsPressed)
                {
                    nodeSelector.MarkedForExpansion = true;
                    RedrawAndSelect(node);
                }
                return;
            }

            nodeSelector.MarkedForExpansion = eventArgs.LeftButtonIsPressed;
            if (Graph.AreRelatives(nodeSelector.Selected, node) && !(node is Subgraph))
            {
                nodeSelector.Selected = node;
                gViewer.Invalidate();
                return;
            }

            RedrawAndSelect(node);
        }
    }

    private void RedrawAndSelect(Node node)
    {
        nodeSelector.Selected = node;
        ReDrawAndKeepFocus();
    }

    private ISchemaItem RetrieveItem(Node node)
    {
        return RetrieveItem(IdTranslator.ToSchemaId(node));
    }

    private ISchemaItem RetrieveItem(Guid id)
    {
        return persistenceProvider.RetrieveInstance(typeof(ISchemaItem), new Key(id))
            as ISchemaItem;
    }

    private ISchemaItem RetrieveItem(string strId)
    {
        Guid id = IdTranslator.NodeToSchema(strId);
        return persistenceProvider.RetrieveInstance(typeof(ISchemaItem), new Key(id))
            as ISchemaItem;
    }

    public void Dispose()
    {
        persistenceProvider.InstancePersisted -= OnInstancePersisted;
        (gViewer as IViewer).MouseDown -= OnMouseDown;
        gViewer.EdgeAdded -= OnEdgeAdded;
        gViewer.EdgeRemoved -= OnEdgeRemoved;
        edgeInsertionRule?.Dispose();
        gViewer?.Dispose();
    }
}
