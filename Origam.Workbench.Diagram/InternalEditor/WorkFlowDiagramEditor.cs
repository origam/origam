using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
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
        private readonly ContextStoreDependencyPainter dependencyPainter;

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
			
			dependencyPainter = new ContextStoreDependencyPainter(
				nodeSelector,
				persistenceProvider,
				gViewer, 
				() => (AbstractSchemaItem)UpToDateGraphParent);
		}

        public void ReDraw()
        {
	        gViewer.Graph = factory.Draw(UpToDateGraphParent);
	        factory.AlignContextStoreSubgraph();
	        gViewer.Transform = null;
	        gViewer.Invalidate();
	        nodeSelector.Selected = Graph.FindNodeOrSubgraph(nodeSelector.Selected?.Id);
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
	        return Graph.ContextStoreSubgraph.Nodes.Contains(edge.SourceNode) ||
	               Graph.ContextStoreSubgraph.Nodes.Contains(edge.TargetNode);
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
		        .ToEnumerable()
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
				ReDraw();
			}
			else
			{
				node.LabelText = persistedSchemaItem.Name;
				gViewer.Redraw();
			}
		}

		private void TrySelectParentStep(AbstractSchemaItem persistedSchemaItem)
		{
			if (!(persistedSchemaItem is IWorkflowStep))
			{
				AbstractSchemaItem parentStep =
					persistedSchemaItem.FirstParentOfType<IWorkflowStep>();
				Node stepNode =
					gViewer.Graph.FindNodeOrSubgraph(parentStep.Id.ToString());
				nodeSelector.Selected = stepNode;
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
				.Select(source =>
				{
					Guid.TryParse(source, out Guid id);
					return id;
				})
				.Where(id => id != Guid.Empty)
				.Any(targetNodeId => targetNodeId == sourceId);

			if (edgeWasRemovedOutsideDiagram)
			{
				ReDraw();
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
				ReDraw();
			}
		}

		void OnMouseDown(object sender, MsaglMouseEventArgs e)
		{
			if (gViewer.InsertingEdge) return;
	        if (e.RightButtonIsPressed && !e.Handled)
	        {
		        _mouseRightButtonDownPoint = new ClickPoint( gViewer, e);
		        ContextMenuStrip cm = BuildContextMenu();
                cm.Show(parentForm,_mouseRightButtonDownPoint.InScreenSystem);
	        }else if (e.LeftButtonIsPressed)
	        {
		        if(gViewer.SelectedObject is Node node)
		        {
			        if (nodeSelector.Selected == node)
			        {
				        return;
			        }
			        else if (nodeSelector.Selected?.Id == node.Id)
			        {
				        nodeSelector.Selected=node;
			        }
			        else if (Graph.AreRelatives(nodeSelector.Selected, node))
			        {
				        nodeSelector.Selected=node;
				        gViewer.Invalidate();
			        }
			        else 
			        {
				        nodeSelector.Selected=node;
				        var origTransform = gViewer.Transform;
				        ReDraw();
				        gViewer.Transform = origTransform;
				        gViewer.Invalidate();
			        }
		        }
	        }
	    }

		private bool IsDeleteMenuItemAvailable(DNode objectUnderMouse)
		{
			if (objectUnderMouse == null) return false;
			if (nodeSelector.Selected == Graph.MainDrawingSubgraf) return false;
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
				AsMenuCommand menuItem = new AsMenuCommand("New");
				var builder = new SchemaItemEditorsMenuBuilder(true);
				menuItem.PopulateMenu(builder);
				menuItem.SubItems.AddRange(menuItem.DropDownItems);
				new AsContextMenu(WorkbenchSingleton.Workbench).AddSubItem(menuItem);
				menuItem.ShowDropDown();
				newMenu.DropDownItems.AddRange(menuItem.DropDownItems.ToArray<ToolStripItem>());
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