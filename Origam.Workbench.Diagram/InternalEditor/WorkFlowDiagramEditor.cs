using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.UI;
using Origam.Workbench.BaseComponents;
using Origam.Workbench.Commands;
using Origam.Workbench.Diagram.Extensions;
using Origam.Workbench.Editors;
using Origam.Workbench.Services;

namespace Origam.Workbench.Diagram.InternalEditor
{
	public class WorkFlowDiagramEditor: IDiagramEditor
    {
      	private readonly WorkFlowDiagramFactory factory;
        private readonly GViewer gViewer;
        private readonly Form parentForm;

        private ClickPoint _mouseRightButtonDownPoint;
        private readonly IPersistenceProvider persistenceProvider;
        private readonly WorkbenchSchemaService schemaService;
        private readonly Guid graphParentId;
        private readonly EdgeInsertionRule edgeInsertionRule;

        public WorkFlowDiagramEditor(Guid graphParentId, GViewer gViewer, Form parentForm,
	        IPersistenceProvider persistenceProvider, WorkFlowDiagramFactory factory)
		{
		    (gViewer as IViewer).MouseDown += OnMouseDown;
		    gViewer.MouseClick += OnMouseClick;
		    schemaService = ServiceManager.Services.GetService<WorkbenchSchemaService>();
			gViewer.EdgeAdded += OnEdgeAdded;
			gViewer.EdgeRemoved += OnEdgeRemoved;
			edgeInsertionRule = new EdgeInsertionRule(
				viewerToImposeOn: gViewer,
				predicate: (sourceNode, targetNode) =>
				{
					if (sourceNode is Subgraph) return false;
					if (targetNode is Subgraph) return false;
					var sourcesParent = gViewer.Graph.FindParentSubGraph(sourceNode);
					var targetsParent = gViewer.Graph.FindParentSubGraph(targetNode);
					return sourcesParent == targetsParent;
				});

			this.graphParentId = graphParentId;
			this.gViewer = gViewer;
			this.parentForm = parentForm;
			this.persistenceProvider = persistenceProvider;
			this.factory = factory;

			gViewer.Graph = factory.Draw(UpToDateGraphParent);

			persistenceProvider.InstancePersisted += OnInstancePersisted;
		}

        private void OnMouseClick(object sender, MouseEventArgs args)
        {
	        SelectActiveNodeInModelView();
        }

        private void OnEdgeRemoved(object sender, EventArgs e)
        {
	        Edge edge = (Edge)sender;

	        var dependentItem = persistenceProvider.RetrieveInstance(
		        typeof(AbstractSchemaItem),
		        new Key(edge.Target)) as AbstractSchemaItem;

	        var workflowTaskDependency = dependentItem.ChildItems
		        .ToEnumerable()
		        .OfType<WorkflowTaskDependency>()
		        .SingleOrDefault(x => x.WorkflowTaskId == Guid.Parse(edge.Source));
	        workflowTaskDependency.IsDeleted = true;
	        workflowTaskDependency.Persist();
	        
	        schemaService.SchemaBrowser.EbrSchemaBrowser.RefreshItem(dependentItem);
        }

        private void OnEdgeAdded(object sender, EventArgs e)
        {
	        Edge edge = (Edge)sender;
			 
	        var independentItem = persistenceProvider.RetrieveInstance(
		        typeof(IWorkflowStep),
		        new Key(edge.Source)) as IWorkflowStep;
	        var dependentItem = persistenceProvider.RetrieveInstance(
		        typeof(AbstractSchemaItem),
		        new Key(edge.Target)) as AbstractSchemaItem;


	        var workflowTaskDependency = new WorkflowTaskDependency
	        {
		        SchemaExtensionId = dependentItem.SchemaExtensionId,
		        PersistenceProvider = persistenceProvider,
		        ParentItem = dependentItem,
		        Task = independentItem
	        };
	        workflowTaskDependency.Persist();

	        schemaService.SchemaBrowser.EbrSchemaBrowser.RefreshItem(dependentItem);
        }

        private bool SelectActiveNodeInModelView()
        {
	        if (gViewer.SelectedObject is Node node)
	        {
		        Guid nodeId = Guid.Parse(node.Id);
		        var schemaItem = persistenceProvider.RetrieveInstance(
			        typeof(AbstractSchemaItem),
			        new Key(nodeId))
			        as AbstractSchemaItem;
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
				return;
			}
			
			if (persistedSchemaItem.IsDeleted)
			{
				switch (persistedSchemaItem)
				{
					case IWorkflowStep _:
						RemoveNode(persistedSchemaItem.Id);
						break;
					case WorkflowTaskDependency taskDependency:
						RemoveEdge(taskDependency.WorkflowTaskId);
						break;
				}
			}
		}

		private void UpdateNodeOf(AbstractSchemaItem persistedSchemaItem)
		{
			Node node = gViewer.Graph.FindNode(persistedSchemaItem.Id.ToString());
			if (node == null)
			{
				gViewer.Graph = factory.Draw(UpToDateGraphParent);
			}
			else
			{
				node.LabelText = persistedSchemaItem.Name;
				gViewer.Redraw();
			}
		}

		private IWorkflowBlock UpToDateGraphParent =>
			persistenceProvider.RetrieveInstance(
					typeof(IWorkflowBlock),
					new Key(graphParentId))
				as IWorkflowBlock;
		

		private void RemoveEdge(Guid sourceId)
		{
			bool edgeWasRemovedOutsideDiagram = gViewer.Graph.RootSubgraph
				.GetAllNodes()
				.SelectMany(node => node.Edges.Select(edge => edge.Source))
				.Select(Guid.Parse)
				.Any(targetNodeId => targetNodeId == sourceId);

			if (edgeWasRemovedOutsideDiagram)
			{
				gViewer.Graph = factory.Draw(UpToDateGraphParent);
			}
		}

		private bool RemoveNode(Guid nodeId)
		{
			Node node = gViewer.Graph.FindNode(nodeId.ToString());
			if (node == null) return true;

			IViewerNode viewerNode = gViewer.FindViewerNode(node);
			gViewer.RemoveNode(viewerNode, true);
			gViewer.Graph.RemoveNodeEverywhere(node);
			return false;
		}

		void OnMouseDown(object sender, MsaglMouseEventArgs e)
	    {
	        if (e.RightButtonIsPressed && !e.Handled)
	        {
		        _mouseRightButtonDownPoint = new ClickPoint( gViewer, e);

	            ContextMenuStrip cm = BuildContextMenu();
                cm.Show(parentForm,_mouseRightButtonDownPoint.InScreenSystem);
	        }
	    }

		private bool IsDeleteMenuItemAvailable(DNode objectUnderMouse)
		{
			if (objectUnderMouse == null) return false;

			List<IViewerObject> highLightedEntities = gViewer.Entities
				.Where(x => x.MarkedForDragging)
				.ToList();
			if (highLightedEntities.Count != 1) return false;
			if (!(highLightedEntities[0] is IViewerNode viewerNode))return false;
			
			Subgraph topWorkFlowSubGraph = gViewer.Graph.RootSubgraph.Subgraphs.FirstOrDefault();
			if (viewerNode.Node == topWorkFlowSubGraph) return false;
			return objectUnderMouse.Node == viewerNode.Node;
		}

		private bool IsNewMenuAvailable(DNode objectUnderMouse)
		{
			if (objectUnderMouse == null) return false;
			if (!(objectUnderMouse.Node is Subgraph)) return false;
			List<IViewerObject> highLightedEntities = gViewer.Entities
				.Where(x => x.MarkedForDragging)
				.ToList();
			if (highLightedEntities.Count != 1) return false;
			if (!(highLightedEntities[0] is IViewerNode viewerNode))return false;
			return objectUnderMouse.Node == viewerNode.Node;
		}

		private ContextMenuStrip BuildContextMenu()
        {
	        var objectUnderMouse = gViewer.GetObjectAt(_mouseRightButtonDownPoint.InScreenSystem) as DNode;

	        var contextMenu = new AsContextMenu(WorkbenchSingleton.Workbench);
	        
            var deleteMenuItem = new ToolStripMenuItem();
            deleteMenuItem.Text = "Delete";
            deleteMenuItem.Image = ImageRes.icon_delete;
            deleteMenuItem.Click += DeleteNode_Click;
            deleteMenuItem.Enabled = IsDeleteMenuItemAvailable(objectUnderMouse);

            ToolStripMenuItem newMenu = new ToolStripMenuItem("New");
            var builder = new SchemaItemEditorsMenuBuilder();
            var submenuItems = builder.BuildSubmenu(null);
	        newMenu.DropDownItems.AddRange(submenuItems);
            newMenu.Image = ImageRes.icon_new;
            newMenu.Enabled = IsNewMenuAvailable(objectUnderMouse);
			
	        contextMenu.AddSubItem(newMenu);
	        contextMenu.AddSubItem(deleteMenuItem);
	        
            return contextMenu;
	    }

        private void DeleteNode_Click(object sender, EventArgs e)
        { 
	        bool nodeSelected = SelectActiveNodeInModelView();

	        if (nodeSelected)
	        {
		        new DeleteActiveNode().Run();
	        }
	        else
	        {
		        throw new Exception("Could not delete node because it is not selected in the tree.");
	        }
        }
        
        class ClickPoint
        {
	        public Point InMsaglSystem { get; }
	        public System.Drawing.Point InScreenSystem { get;}

	        public ClickPoint(GViewer gViewer,  MsaglMouseEventArgs e)
	        {
		        InMsaglSystem = gViewer.ScreenToSource(e);
		        InScreenSystem = new System.Drawing.Point(e.X, e.Y);
	        }
        }

        public void Dispose()
        {
	        persistenceProvider.InstancePersisted -= OnInstancePersisted;
	        (gViewer as IViewer).MouseDown -= OnMouseDown;
	        gViewer.MouseClick -= OnMouseClick;
	        gViewer.EdgeAdded -= OnEdgeAdded;
	        gViewer.EdgeRemoved -= OnEdgeRemoved;
	        edgeInsertionRule?.Dispose();
	        gViewer?.Dispose();
        }
    }
}