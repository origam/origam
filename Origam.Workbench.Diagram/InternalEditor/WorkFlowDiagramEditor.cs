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
using Origam.Workbench.Services;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Origam.Workbench.Diagram.InternalEditor
{
	public partial class WorkFlowDiagramEditor: IDiagramEditor
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

        public WorkFlowDiagramEditor(Guid graphParentId, GViewer gViewer, Form parentForm,
	        IPersistenceProvider persistenceProvider, WorkFlowDiagramFactory factory, NodeSelector nodeSelector)
        {
		    (gViewer as IViewer).MouseDown += OnMouseDown;
		    schemaService = ServiceManager.Services.GetService<WorkbenchSchemaService>();
	        taskRunner = new DependencyTaskRunner(persistenceProvider);
			gViewer.EdgeAdded += OnEdgeAdded;
			gViewer.EdgeRemoved += OnEdgeRemoved;
			gViewer.MouseDoubleClick += OnDoubleClick;
			edgeInsertionRule = new EdgeInsertionRule(
				viewerToImposeOn: gViewer,
				predicate: (sourceNode, targetNode) =>
				{
					if (!Graph.IsWorkFlowItemSubGraph(sourceNode)) return false;
					if (!Graph.IsWorkFlowItemSubGraph(targetNode)) return false;
					if (RetrieveItem(sourceNode.Id) is ContextStore ) return false;
					if (RetrieveItem(targetNode.Id) is ContextStore ) return false;
					var sourcesParent = gViewer.Graph.FindParentSubGraph(sourceNode);
					var targetsParent = gViewer.Graph.FindParentSubGraph(targetNode);
					return Equals(sourcesParent, targetsParent);
				});

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
				graphParentItemGetter: () => (AbstractSchemaItem)UpToDateGraphParent);
			
			ReDrawAndReselect();
        }
        
        public void ReDrawAndReselect()
        {
			ReDraw();
			Node nodeToSelect = Graph.FindNodeOrSubgraph(nodeSelector.Selected?.Id);
			if (nodeToSelect == null &&
			    Guid.TryParse(nodeSelector.Selected?.Id, out var id) &&
			    UpToDateGraphParent.Id == id)
			{
				nodeToSelect = Graph.MainDrawingSubgraf;
			}

			nodeSelector.Selected = nodeToSelect;
        }

        private void ReDraw()
        {
	        var originalTransform = gViewer.Transform;

	        List<string> nodesToExpand = nodeSelector.Selected == null
		        ? new List<string>()
		        : new List<string> {nodeSelector.Selected?.Id};
	        nodesToExpand.AddRange(dependencyPainter.GetNodesToExpand());
	        gViewer.Graph = factory.Draw(UpToDateGraphParent, nodesToExpand);
	        dependencyPainter.Draw();

	        if (originalTransform.IsIdentity)
	        {
		        gViewer.Transform =  null;
	        }
	        else
	        {
		        gViewer.Transform = originalTransform;
		        gViewer.Redraw();
	        }
        }
        
        private void OnDoubleClick(object sender, EventArgs e)
        {
	        GViewer viewer = sender as GViewer;
	        if (!(viewer.SelectedObject is Edge edge)) return;
	        Guid? id = (edge.UserData as WorkflowTaskDependency)?.Id;
	        if (id == null) return;
	        AbstractSchemaItem clickedItem = RetrieveItem(id.Value.ToString());
	        if(clickedItem != null)
	        {
		        EditSchemaItem cmd = new EditSchemaItem
		        {
			        ShowDialog = true,
			        Owner = clickedItem
		        };
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
	        Edge edge  = (Edge)sender;
	        if (IsContextStoreDependencyArrow(edge) || !ConnectsSchemaItems(edge))
	        {
		        return;
	        }
	        DeleteDependency(edge);
        }

        private bool IsContextStoreDependencyArrow(Edge edge)
        {
	        return Graph
		        .AllContextStoreSubgraphs
		        .Any(contextStoreSubgraph =>
			        contextStoreSubgraph.Nodes.Contains(edge.SourceNode) ||
			        contextStoreSubgraph.Nodes.Contains(edge.TargetNode));
        }

        private bool ConnectsSchemaItems(Edge edge)
        {
	        return Guid.TryParse(edge.Source, out Guid sourceId) &&
		           Guid.TryParse(edge.Target, out Guid targetId);
        }

        private void DeleteDependency(Edge edge)
        {
	        AbstractSchemaItem dependentItem = RetrieveItem(edge.Target);
	        var workflowTaskDependency = dependentItem.ChildItems
		        .ToGeneric()
		        .OfType<WorkflowTaskDependency>()
		        .Single(x => x.WorkflowTaskId == Guid.Parse(edge.Source));
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
	        var independentItem = persistenceProvider.RetrieveInstance(
		        typeof(IWorkflowStep),
		        new Key(source)) as IWorkflowStep;
	        AbstractSchemaItem dependentItem = RetrieveItem(target);
	        var workflowTaskDependency = new WorkflowTaskDependency
	        {
		        SchemaExtensionId = dependentItem.SchemaExtensionId,
		        PersistenceProvider = persistenceProvider,
		        ParentItem = dependentItem,
		        Task = independentItem
	        };
	        workflowTaskDependency.Persist();
	        return workflowTaskDependency;
        }

        private bool TrySelectActiveNodeInModelView()
        {
	        if (gViewer.SelectedObject is Node node)
	        {
		        string id = node is IWorkflowSubgraph workflowSubgraph
			        ? workflowSubgraph.WorkflowItemId
			        : node.Id;
		        bool idParsed = Guid.TryParse(id, out Guid nodeId);
		        if (!idParsed) return false;
		        var schemaItem = RetrieveItem(nodeId.ToString());
		        if (schemaItem != null)
		        {
			        schemaService.SelectItem(schemaItem);
			        Guid activeNodeId = Guid.Parse(schemaService.ActiveNode.NodeId);
			        return nodeId == activeNodeId;
		        }
	        }
	        return false;
        }

		private void OnInstancePersisted(object sender,IPersistent persistedObject)
		{
			if (!(persistedObject is AbstractSchemaItem persistedSchemaItem)) return;
			
			bool childPersisted = (UpToDateGraphParent as AbstractSchemaItem)
				?.ChildrenRecursive
				.Any(x => x.Id == persistedSchemaItem.Id)
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
		
		private void UpdateNodeOf(AbstractSchemaItem persistedSchemaItem)
		{
			Node node = gViewer.Graph.FindNodeOrSubgraph(persistedSchemaItem.Id.ToString());
			if (node == null)
			{
				TrySelectParentStep(persistedSchemaItem);
				ReDrawAndReselect();
			}
			else
			{
				node.LabelText = persistedSchemaItem.Name;
				ReDraw();
			}
		}

		private void TrySelectParentStep(AbstractSchemaItem persistedSchemaItem)
		{
			if (!(persistedSchemaItem is IWorkflowStep))
			{
				AbstractSchemaItem parentStep =
					persistedSchemaItem.FirstParentOfType<WorkflowTask>();
				if (parentStep == null) return;
				Node stepNode =
					gViewer.Graph.FindNodeOrSubgraph(parentStep.Id.ToString());
				nodeSelector.Selected = stepNode;
			}
		}

		private IWorkflowBlock UpToDateGraphParent =>
			persistenceProvider.RetrieveInstance(
					typeof(AbstractSchemaItem),
					new Key(graphParentId))
				as IWorkflowBlock;


		private void RemoveEdge(Guid sourceId)
		{
			bool edgeWasRemovedOutsideDiagram = gViewer.Graph.RootSubgraph
				.GetAllNodes()
				.SelectMany(node => node.Edges.Select(edge => edge.Source))
				.Select(source =>
				{
					Guid.TryParse(source, out Guid id);
					return id;
				})
				.Where(id => id != Guid.Empty)
				.Any(targetNodeId => targetNodeId == sourceId);

			if (edgeWasRemovedOutsideDiagram)
			{
				ReDrawAndReselect();
			}
		}

		private void RemoveNode(Guid nodeId)
		{
			bool deletedNodeWasSelected = nodeSelector.SelectedNodeId == nodeId;
			if (deletedNodeWasSelected)
			{
				nodeSelector.Selected = null;
			}
			Node node = gViewer.Graph.FindNodeOrSubgraph(nodeId.ToString());
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
			if (viewerNode == null) return;
			
			gViewer.RemoveNode(viewerNode, true);
			bool deletedNodeItem = !(node is Subgraph);
			bool deletedLastNode = !parentSubgraph.Nodes.Any();
			if (deletedLastNode || deletedNodeItem)
			{
				ReDrawAndReselect();
			}
		}

		void OnMouseDown(object sender, MsaglMouseEventArgs e)
		{
			if (gViewer.InsertingEdge) return;
			LeftMouseButtonDown(e);
			TrySelectActiveNodeInModelView();
			TrySelectActiveEdgeInModelView();
	        if (e.RightButtonIsPressed && !e.Handled)
	        {
		        mouseRightButtonDownPoint  = new System.Drawing.Point(e.X, e.Y);
		        ContextMenuStrip cm = BuildContextMenu();
                cm.Show(parentForm,mouseRightButtonDownPoint);
	        }
	    }

		private void LeftMouseButtonDown(MsaglMouseEventArgs eventArgs)
		{
			gViewer.Hit(new MouseEventArgs(MouseButtons.None, 0, eventArgs.X,eventArgs.Y,0));
			if (gViewer.SelectedObject is Node node)
			{
				if (Equals(nodeSelector.Selected, node))
				{
					return;
				}

				if (nodeSelector.Selected?.Id == node.Id)
				{
					nodeSelector.Selected = node;
					return;
				}

				bool currentOrPreviousNodeIsContextStore =
					RetrieveItem(node) is ContextStore ||
					RetrieveItem(nodeSelector.Selected) is ContextStore;
				if (Graph.AreRelatives(nodeSelector.Selected, node) &&
				    !(node is Subgraph) &&
				    !currentOrPreviousNodeIsContextStore)
				{
					nodeSelector.Selected = node;
					gViewer.Invalidate();
					return;
				}

				if (currentOrPreviousNodeIsContextStore)
				{
					ReDrawAndKeepFocus(node);
				}
				else
				{
					nodeSelector.Selected = node;
					var origTransform = gViewer.Transform;
					ReDrawAndReselect();
					gViewer.Transform = origTransform;
					gViewer.Invalidate();
				}
			}
		}

		private void ReDrawAndKeepFocus(Node node)
		{
			var newNodeTracker = new NodePositionTracker(gViewer, node.Id);
			var oldNodeTracker = new NodePositionTracker(gViewer, nodeSelector.Selected?.Id);
			var originalTransform = gViewer.Transform;
			nodeSelector.Selected = node;

			ReDrawAndReselect();
			newNodeTracker.LoadUpdatedState();
			oldNodeTracker.LoadUpdatedState();
			
			if (oldNodeTracker.NodeExists && oldNodeTracker.NodeWasNotResized)
			{
				gViewer.Transform = oldNodeTracker.UpdatedTransformation;
			}
			else if (newNodeTracker.NodeExists && newNodeTracker.NodeWasNotResized)
			{
				gViewer.Transform = newNodeTracker.UpdatedTransformation;
			}
			else
			{
				gViewer.Transform = originalTransform;
			}

			gViewer.Invalidate();
		}

		private AbstractSchemaItem RetrieveItem(Node node)
		{
			if (node == null) return null;
			if (!Guid.TryParse(node.Id, out Guid _)) return null;
			return RetrieveItem(node.Id);
		}

		private AbstractSchemaItem RetrieveItem(string id)
		{
			if (string.IsNullOrWhiteSpace(id)) return null;
			return persistenceProvider
					.RetrieveInstance(
						typeof(AbstractSchemaItem),
						new Key(id))
				as AbstractSchemaItem;
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
}