using System;
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
        private readonly NodeSelector nodeSelector;
        private readonly DependencyTaskRunner taskRunner;

        private WorkFlowGraph Graph => (WorkFlowGraph)gViewer.Graph;

        public WorkFlowDiagramEditor(Guid graphParentId, GViewer gViewer, Form parentForm,
	        IPersistenceProvider persistenceProvider, WorkFlowDiagramFactory factory, NodeSelector nodeSelector)
        {
		    (gViewer as IViewer).MouseDown += OnMouseDown;
		    gViewer.MouseClick += OnMouseClick;
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
	        factory.AlignContextStoreSubgraph();
	        gViewer.DefaultDragObject = gViewer.ViewerGraph.Nodes()
		        .Single(x => x.Node == gViewer.Graph.RootSubgraph.Subgraphs.First());
	        gViewer.Transform = null;
	        gViewer.Invalidate();
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

	        AbstractSchemaItem dependentItem = RetrieveItem(edge.Target);
	        var workflowTaskDependency = dependentItem.ChildItems
		        .ToEnumerable()
		        .OfType<WorkflowTaskDependency>()
		        .Single(x => x.WorkflowTaskId == Guid.Parse(edge.Source));
	        workflowTaskDependency.IsDeleted = true;
	        workflowTaskDependency.Persist();
        }

        private void OnEdgeAdded(object sender, EventArgs e)
        {
	        Edge edge = (Edge)sender;
			 
	        var independentItem = persistenceProvider.RetrieveInstance(
		        typeof(IWorkflowStep),
		        new Key(edge.Source)) as IWorkflowStep;
	        AbstractSchemaItem dependentItem = RetrieveItem(edge.Target);
	        var workflowTaskDependency = new WorkflowTaskDependency
	        {
		        SchemaExtensionId = dependentItem.SchemaExtensionId,
		        PersistenceProvider = persistenceProvider,
		        ParentItem = dependentItem,
		        Task = independentItem
	        };
	        workflowTaskDependency.Persist();
	        edge.UserData = workflowTaskDependency;
        }

        private bool TrySelectActiveNodeInModelView()
        {
	        if (gViewer.SelectedObject is Node node)
	        {
		        bool idParsed = Guid.TryParse(node.Id, out Guid nodeId);
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
			Subgraph parentSubgraph = gViewer.Graph.FindParentSubGraph(node);

			IViewerNode viewerNode = gViewer.FindViewerNode(node);
			gViewer.RemoveNode(viewerNode, true);
			gViewer.Graph.RemoveNodeEverywhere(node);
			if (parentSubgraph != null && !parentSubgraph.Nodes.Any())
			{
				ReDraw();
			}

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
			Subgraph topWorkFlowSubGraph = gViewer.Graph.RootSubgraph.Subgraphs
				.FirstOrDefault(x=>x.Id == UpToDateGraphParent.Id.ToString());
			if (nodeSelector.Selected == topWorkFlowSubGraph) return false;
			return objectUnderMouse.Node == nodeSelector.Selected;
		}

		private bool IsNewMenuAvailable(DNode dNodeUnderMouse)
		{
			if (dNodeUnderMouse == null) return false;
			var schemaItem = DNodeToSchemaItem(dNodeUnderMouse);
			if (!(dNodeUnderMouse.Node is Subgraph) && 
			    !(schemaItem is ServiceMethodCallParameter)) return false;
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
			if (Graph.IsInfrastructureSubGraph(dNodeUnderMouse?.Node))
			{
				return new ContextMenuStrip();
			}

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
						var menuItem = new AsMenuCommand(schemaItem.Name, schemaItem);
						var builder = new SchemaItemEditorsMenuBuilder(true);
						menuItem.PopulateMenu(builder);
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

			if (Graph.IsWorkFlowItemSubGraph(dNodeUnderMouse?.Node)) 
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
				addAfterMenu.Enabled = IsDeleteMenuItemAvailable(dNodeUnderMouse);
				contextMenu.AddSubItem(addAfterMenu);
			}

			return contextMenu;
		}

		private AbstractSchemaItem DNodeToSchemaItem(DNode dNodeUnderMouse)
		{
			if (!Guid.TryParse(dNodeUnderMouse.Node.Id, out Guid _)) return null;
			return RetrieveItem(dNodeUnderMouse.Node.Id);
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
}