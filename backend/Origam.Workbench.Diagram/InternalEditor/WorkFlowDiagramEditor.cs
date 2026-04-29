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
        taskRunner = new DependencyTaskRunner(persistenceProvider: persistenceProvider);
        gViewer.EdgeAdded += OnEdgeAdded;
        gViewer.EdgeRemoved += OnEdgeRemoved;
        edgeInsertionRule = new EdgeInsertionRule(
            viewerToImposeOn: gViewer,
            predicate: (sourceNode, targetNode) =>
            {
                if (IdTranslator.ToSchemaId(node: sourceNode) == Guid.Empty)
                {
                    return false;
                }

                if (IdTranslator.ToSchemaId(node: targetNode) == Guid.Empty)
                {
                    return false;
                }

                if (RetrieveItem(node: sourceNode) is ContextStore)
                {
                    return false;
                }

                if (RetrieveItem(node: targetNode) is ContextStore)
                {
                    return false;
                }

                var sourcesParent = gViewer.Graph.FindParentSubGraph(node: sourceNode);
                var targetsParent = gViewer.Graph.FindParentSubGraph(node: targetNode);
                return Equals(objA: sourcesParent, objB: targetsParent);
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
            gViewer: gViewer,
            graphParentItemGetter: () => (ISchemaItem)UpToDateGraphParent
        );

        ReDrawAndKeepFocus();
    }

    public void ReDrawAndKeepFocus()
    {
        var selectedNodeTracker = new NodePositionTracker(
            gViewer: gViewer,
            nodeId: nodeSelector.Selected?.Id
        );
        Subgraph parentSubGraph = Graph.FindParentSubGraph(node: nodeSelector.Selected);
        var parentNodeTracker = new NodePositionTracker(
            gViewer: gViewer,
            nodeId: parentSubGraph?.Id
        );

        ReDraw();
        ReSelectNode();
        var nodeTracker = nodeSelector.Selected == null ? parentNodeTracker : selectedNodeTracker;
        nodeTracker.LoadUpdatedState();
        gViewer.Transform = nodeTracker.NodeExists ? nodeTracker.UpdatedTransformation : null;
        gViewer.Invalidate();
    }

    private void ReSelectNode()
    {
        Node nodeToSelect = Graph.FindNodeOrSubgraph(id: nodeSelector.Selected?.Id);
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
        nodesToExpand.AddRange(collection: dependencyPainter.GetNodesToExpand());
        gViewer.Graph = factory.Draw(
            graphParent: UpToDateGraphParent,
            expandedSubgraphNodeIds: nodesToExpand
        );
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
                : IdTranslator.NodeToSchema(nodeId: node.Id);
            if (schemaItemId == Guid.Empty)
            {
                return;
            }
        }
        else if (viewer.SelectedObject is Edge edge)
        {
            Guid? id = (edge.UserData as WorkflowTaskDependency)?.Id;
            if (id == null)
            {
                return;
            }

            schemaItemId = id.Value;
        }
        ShowEditor(schemaItemId: schemaItemId);
    }

    private void ShowEditor(Guid schemaItemId)
    {
        ISchemaItem clickedItem = RetrieveItem(id: schemaItemId);
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
                schemaService.SelectItem(schemaItem: dependencyItem);
            }
        }
    }

    private void OnEdgeRemoved(object sender, EventArgs e)
    {
        Edge edge = (Edge)sender;
        if (IsContextStoreDependencyArrow(edge: edge) || !ConnectsSchemaItems(edge: edge))
        {
            return;
        }
        DeleteDependency(edge: edge);
    }

    private bool IsContextStoreDependencyArrow(Edge edge)
    {
        return Graph.AllContextStoreSubgraphs.Any(predicate: contextStoreSubgraph =>
            contextStoreSubgraph.Nodes.Contains(value: edge.SourceNode)
            || contextStoreSubgraph.Nodes.Contains(value: edge.TargetNode)
        );
    }

    private bool ConnectsSchemaItems(Edge edge)
    {
        return IdTranslator.NodeToSchema(nodeId: edge.Source) != Guid.Empty
            && IdTranslator.NodeToSchema(nodeId: edge.Target) != Guid.Empty;
    }

    private void DeleteDependency(Edge edge)
    {
        ISchemaItem dependentItem = RetrieveItem(strId: edge.Target);
        Guid sourceId = IdTranslator.NodeToSchema(nodeId: edge.Source);
        var workflowTaskDependency = dependentItem
            .ChildItems.OfType<WorkflowTaskDependency>()
            .First(predicate: x => x.WorkflowTaskId == sourceId);
        workflowTaskDependency.IsDeleted = true;
        workflowTaskDependency.Persist();
    }

    private void OnEdgeAdded(object sender, EventArgs e)
    {
        Edge edge = (Edge)sender;
        if (IsContextStoreDependencyArrow(edge: edge) || !ConnectsSchemaItems(edge: edge))
        {
            return;
        }
        var workflowTaskDependency = AddDependency(source: edge.Source, target: edge.Target);
        edge.UserData = workflowTaskDependency;
    }

    private WorkflowTaskDependency AddDependency(string source, string target)
    {
        var independentItem = RetrieveItem(strId: source) as IWorkflowStep;
        ISchemaItem dependentItem = RetrieveItem(strId: target);
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
                : IdTranslator.ToSchemaId(node: node);
            if (schemaItemId == Guid.Empty)
            {
                return false;
            }

            var schemaItem = RetrieveItem(id: schemaItemId);
            if (schemaItem != null)
            {
                schemaService.SelectItem(schemaItem: schemaItem);
                Guid activeNodeId = Guid.Parse(input: schemaService.ActiveNode.NodeId);
                return schemaItemId == activeNodeId;
            }
        }
        return false;
    }

    private void OnInstancePersisted(object sender, IPersistent persistedObject)
    {
        if (!(persistedObject is ISchemaItem persistedSchemaItem))
        {
            return;
        }

        bool childPersisted =
            UpToDateGraphParent?.ChildrenRecursive.Any(predicate: x =>
                x.Id == persistedSchemaItem.Id
            ) ?? false;

        if (childPersisted)
        {
            UpdateNodeOf(persistedSchemaItem: persistedSchemaItem);
            taskRunner.UpdateDependencies(persistedSchemaItem: persistedSchemaItem);
            return;
        }

        if (persistedSchemaItem.IsDeleted)
        {
            if (persistedSchemaItem is WorkflowTaskDependency taskDependency)
            {
                RemoveEdge(sourceId: taskDependency.WorkflowTaskId);
            }
            else
            {
                RemoveNode(schemaItemId: persistedSchemaItem.Id);
            }
        }
    }

    private void UpdateNodeOf(ISchemaItem persistedSchemaItem)
    {
        Node node = gViewer.Graph.FindNodeOrSubgraph(
            id: IdTranslator.SchemaToFirstNode(schemaItemId: persistedSchemaItem.Id)
        );
        if (node == null)
        {
            TrySelectParentStep(persistedSchemaItem: persistedSchemaItem);
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
            {
                return;
            }

            Node stepNode = gViewer.Graph.FindNodeOrSubgraph(
                id: IdTranslator.SchemaToFirstNode(schemaItemId: parentStep.Id)
            );
            nodeSelector.Selected = stepNode;
        }
    }

    private IWorkflowBlock UpToDateGraphParent =>
        persistenceProvider.RetrieveInstance(
            type: typeof(ISchemaItem),
            primaryKey: new Key(id: graphParentId)
        ) as IWorkflowBlock;

    private void RemoveEdge(Guid sourceId)
    {
        bool edgeWasRemovedOutsideDiagram = gViewer
            .Graph.RootSubgraph.GetAllNodes()
            .SelectMany(selector: node => node.Edges.Select(selector: edge => edge.Source))
            .Select(selector: IdTranslator.NodeToSchema)
            .Where(predicate: id => id != Guid.Empty)
            .Any(predicate: targetNodeId => targetNodeId == sourceId);
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
        Node node = gViewer.Graph.FindNodeOrSubgraph(
            id: IdTranslator.SchemaToFirstNode(schemaItemId: schemaItemId)
        );
        if (node == null)
        {
            return;
        }

        Subgraph parentSubgraph = gViewer.Graph.FindParentSubGraph(node: node);
        if (deletedNodeWasSelected)
        {
            nodeSelector.Selected = parentSubgraph;
        }
        gViewer.Graph.RemoveNodeEverywhere(node: node);
        IViewerNode viewerNode = gViewer.FindViewerNode(node: node);
        if (viewerNode == null)
        {
            return;
        }

        gViewer.RemoveNode(node: viewerNode, registerForUndo: true);
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
        {
            return;
        }

        HandleSelectAndRedraw(eventArgs: e);
        TrySelectActiveNodeInModelView();
        TrySelectActiveEdgeInModelView();
        if (e.RightButtonIsPressed && !e.Handled)
        {
            mouseRightButtonDownPoint = new System.Drawing.Point(x: e.X, y: e.Y);
            ContextMenuStrip cm = BuildContextMenu();
            cm.Show(control: parentForm, position: mouseRightButtonDownPoint);
        }
        gViewer.ResumeLayout();
    }

    private void HandleSelectAndRedraw(MsaglMouseEventArgs eventArgs)
    {
        gViewer.Hit(
            args: new MouseEventArgs(
                button: MouseButtons.None,
                clicks: 0,
                x: eventArgs.X,
                y: eventArgs.Y,
                delta: 0
            )
        );
        if (gViewer.SelectedObject is Node node)
        {
            if (Equals(objA: nodeSelector.Selected, objB: node))
            {
                if (nodeSelector.MarkedForExpansion == false && eventArgs.LeftButtonIsPressed)
                {
                    nodeSelector.MarkedForExpansion = true;
                    RedrawAndSelect(node: node);
                }
                return;
            }

            nodeSelector.MarkedForExpansion = eventArgs.LeftButtonIsPressed;
            if (
                Graph.AreRelatives(node1: nodeSelector.Selected, node2: node) && !(node is Subgraph)
            )
            {
                nodeSelector.Selected = node;
                gViewer.Invalidate();
                return;
            }

            RedrawAndSelect(node: node);
        }
    }

    private void RedrawAndSelect(Node node)
    {
        nodeSelector.Selected = node;
        ReDrawAndKeepFocus();
    }

    private ISchemaItem RetrieveItem(Node node)
    {
        return RetrieveItem(id: IdTranslator.ToSchemaId(node: node));
    }

    private ISchemaItem RetrieveItem(Guid id)
    {
        return persistenceProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new Key(id: id)
            ) as ISchemaItem;
    }

    private ISchemaItem RetrieveItem(string strId)
    {
        Guid id = IdTranslator.NodeToSchema(nodeId: strId);
        return persistenceProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new Key(id: id)
            ) as ISchemaItem;
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
