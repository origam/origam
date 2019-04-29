using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using MoreLinq;
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
        private readonly List<DeferedDependency> deferedDependencies = new List<DeferedDependency>();
        private readonly NodeSelector nodeSelector;

        public WorkFlowDiagramEditor(Guid graphParentId, GViewer gViewer, Form parentForm,
	        IPersistenceProvider persistenceProvider, WorkFlowDiagramFactory factory, NodeSelector nodeSelector)
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

			gViewer.EdgeInsertButtonVisible = true;
			
			this.graphParentId = graphParentId;
			this.gViewer = gViewer;
			this.parentForm = parentForm;
			this.persistenceProvider = persistenceProvider;
			this.factory = factory;
			this.nodeSelector = nodeSelector;

			ReDraw();

			persistenceProvider.InstancePersisted += OnInstancePersisted;
		}

        public void ReDraw()
        {
	        gViewer.Graph = factory.Draw(UpToDateGraphParent);
	        gViewer.DefaultDragObject = gViewer.ViewerGraph.Nodes()
		        .Single(x => x.Node == gViewer.Graph.RootSubgraph.Subgraphs.First());
	        gViewer.Transform = null;
	        gViewer.Invalidate();
        }

        private void OnMouseClick(object sender, MouseEventArgs args)
        {
	        TrySelectActiveNodeInModelView();
	        TrySelectActiveEdgeInModelView();
        }

        private void TrySelectActiveEdgeInModelView()
        {
	        if (gViewer.SelectedObject is Edge edge)
	        {
		        var dependencyItem = edge.UserData as WorkflowTaskDependency;
		        schemaService.SelectItem(dependencyItem);
	        }
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
	        edge.UserData = workflowTaskDependency;
	        schemaService.SchemaBrowser.EbrSchemaBrowser.RefreshItem(dependentItem);
        }

        private bool TrySelectActiveNodeInModelView()
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
				CreateDeferedDependency(persistedSchemaItem);
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

		private void CreateDeferedDependency(AbstractSchemaItem persistedSchemaItem)
		{
			DeferedDependency deferedDependency = deferedDependencies
				.SingleOrDefault(x => x.DependentItem.Id == persistedSchemaItem.Id);
			IWorkflowStep independentItem = deferedDependency
				?.IndependentItem 
				as IWorkflowStep;
			if (independentItem == null) return;
			
			var workflowTaskDependency = new WorkflowTaskDependency
			{
				SchemaExtensionId = persistedSchemaItem.SchemaExtensionId,
				PersistenceProvider = persistenceProvider,
				ParentItem = persistedSchemaItem,
				Task = independentItem
			};
			workflowTaskDependency.Persist();

			schemaService.SchemaBrowser.EbrSchemaBrowser.RefreshItem(persistedSchemaItem);
			deferedDependencies.Remove(deferedDependency);
		}

		private void UpdateNodeOf(AbstractSchemaItem persistedSchemaItem)
		{
			Node node = gViewer.Graph.FindNode(persistedSchemaItem.Id.ToString());
			if (node == null)
			{
				ReDraw();
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
				ReDraw();
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
	        }else if (e.LeftButtonIsPressed)
	        {
		        if (gViewer.SelectedObject is Node node)
		        {
					nodeSelector.Selected = node;
					gViewer.Invalidate();
		        }
	        }
	    }

		private bool IsDeleteMenuItemAvailable(DNode objectUnderMouse)
		{
			if (objectUnderMouse == null) return false;
			Subgraph topWorkFlowSubGraph = gViewer.Graph.RootSubgraph.Subgraphs.FirstOrDefault();
			if (nodeSelector.Selected == topWorkFlowSubGraph) return false;
			return objectUnderMouse.Node == nodeSelector.Selected;
		}

		private bool IsNewMenuAvailable(DNode dNodeUnderMouse)
		{
			if (dNodeUnderMouse == null) return false;
			var schemaItemUnderMouse = DNodeToSchemaItem(dNodeUnderMouse);
			if (!(dNodeUnderMouse.Node is Subgraph) &&
			    !(schemaItemUnderMouse is ServiceMethodCallTask)) return false;
			return dNodeUnderMouse.Node == nodeSelector.Selected;
		}

		private ContextMenuStrip BuildContextMenu()
        {
	        object objectUnderMouse = 
		        gViewer.GetObjectAt(_mouseRightButtonDownPoint.InScreenSystem);
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
			var deleteMenuItem = new ToolStripMenuItem();
			deleteMenuItem.Text = "Delete";
			deleteMenuItem.Image = ImageRes.icon_delete;
			deleteMenuItem.Click += (sender, args) => gViewer.RemoveEdge(edge, true); ;
		        
			var contextMenu = new ContextMenuStrip();
			contextMenu.Items.Add(deleteMenuItem);
			return contextMenu;
		}

		private ContextMenuStrip CreateContextMenuForNode(DNode dNodeUnderMouse)
		{
			var schemaItemUnderMouse = DNodeToSchemaItem(dNodeUnderMouse);

			var contextMenu = new AsContextMenu(WorkbenchSingleton.Workbench);

			var deleteMenuItem = new ToolStripMenuItem();
			deleteMenuItem.Text = "Delete";
			deleteMenuItem.Image = ImageRes.icon_delete;
			deleteMenuItem.Click += DeleteNode_Click;
			deleteMenuItem.Enabled = IsDeleteMenuItemAvailable(dNodeUnderMouse);
			contextMenu.AddSubItem(deleteMenuItem);

			ToolStripMenuItem newMenu = new ToolStripMenuItem("New");
			newMenu.Image = ImageRes.icon_new;
			newMenu.Enabled = IsNewMenuAvailable(dNodeUnderMouse);

			if (schemaItemUnderMouse is ServiceMethodCallTask)
			{
				schemaItemUnderMouse.ChildItems
					.ToEnumerable()
					.Where(item => !(item is WorkflowTaskDependency))
					.ForEach(schemaItem =>
					{
						var menuItem = new ToolStripMenuItem(schemaItem.Name);
						var builder = new SchemaItemEditorsMenuBuilder(true);
						var submenuItems = builder.BuildSubmenu(schemaItem);
						menuItem.DropDownItems.AddRange(submenuItems);
						newMenu.DropDownItems.Add(menuItem);
					});
			}
			else
			{
				var builder = new SchemaItemEditorsMenuBuilder(true);
				var submenuItems = builder.BuildSubmenu(schemaItemUnderMouse);
				newMenu.DropDownItems.AddRange(submenuItems);
			}

			contextMenu.AddSubItem(newMenu);

			if (!(dNodeUnderMouse?.Node is Subgraph))
			{
				ToolStripMenuItem addAfterMenu = new ToolStripMenuItem("Add After");
				addAfterMenu.Image = ImageRes.icon_new;
				var builder = new SchemaItemEditorsMenuBuilder(true);
				var submenuItems = builder.BuildSubmenu(UpToDateGraphParent);
				submenuItems[0].Click += (sender, args) =>
				{
					var command = ((AsMenuCommand) sender).Command as AddNewSchemaItem;
					if (command?.CreatedItem == null) return;
					deferedDependencies.Add(new DeferedDependency
					{
						DependentItem = command.CreatedItem,
						IndependentItem = schemaItemUnderMouse
					});
				};
				addAfterMenu.DropDownItems.AddRange(submenuItems);
				addAfterMenu.Enabled = IsDeleteMenuItemAvailable(dNodeUnderMouse);
				contextMenu.AddSubItem(addAfterMenu);
			}

			return contextMenu;
		}

		private AbstractSchemaItem DNodeToSchemaItem(DNode dNodeUnderMouse)
		{
			if (dNodeUnderMouse == null) return null;
			AbstractSchemaItem schemaItemUnderMouse = persistenceProvider
					.RetrieveInstance(
						typeof(AbstractSchemaItem),
						new Key(dNodeUnderMouse.Node.Id))
				as AbstractSchemaItem;
			return schemaItemUnderMouse;
		}

		private void DeleteNode_Click(object sender, EventArgs e)
        { 
	        bool nodeSelected = TrySelectActiveNodeInModelView();

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

	internal class DeferedDependency
	{
		public AbstractSchemaItem IndependentItem { get; set; }
		public AbstractSchemaItem DependentItem { get; set; }
	}
}