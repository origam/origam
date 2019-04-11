#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Workbench.Diagram;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.DA.ObjectPersistence;
using Origam.Schema.WorkflowModel;
using Origam.UI;
using Origam.Workbench.BaseComponents;
using Origam.Workbench.Commands;
using Origam.Workbench.Diagram.Extensions;
using Origam.Workbench.Services;
using Point = Microsoft.Msagl.Core.Geometry.Point;
using DrawingNode = Microsoft.Msagl.Drawing.Node;

namespace Origam.Workbench.Editors
{
	public class DiagramEditor : AbstractViewContent
	{
		DiagramFactory _factory;
        private GViewer gViewer;

        private ClickPoint _mouseRightButtonDownPoint;
        private readonly IPersistenceProvider persistenceProvider;
        private readonly WorkbenchSchemaService schemaService;

        public DiagramEditor()
		{
			persistenceProvider = ServiceManager.Services
				.GetService<IPersistenceService>()
				.SchemaProvider;
			InitializeComponent();
		    (gViewer as IViewer).MouseDown += Form1_MouseDown;
		    gViewer.MouseClick += (sender, args) =>
		    {
			    SelectActiveNodeInModelView();
		    };
		    schemaService = ServiceManager.Services.GetService<WorkbenchSchemaService>();
			gViewer.EdgeAdded += OnEdgeAdded;
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
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            Microsoft.Msagl.Core.Geometry.Curves.PlaneTransformation planeTransformation1 = new Microsoft.Msagl.Core.Geometry.Curves.PlaneTransformation();
            this.gViewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            
            gViewer.DoubleClick += GViewerOnDoubleClick;
            this.SuspendLayout();
            // 
            // gViewer1
            // 
            this.gViewer.ArrowheadLength = 10D;
            this.gViewer.AsyncLayout = false;
            this.gViewer.AutoScroll = true;
            this.gViewer.BackwardEnabled = false;
            this.gViewer.BuildHitTree = true;
            this.gViewer.CurrentLayoutMethod = Microsoft.Msagl.GraphViewerGdi.LayoutMethod.UseSettingsOfTheGraph;
            this.gViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gViewer.EdgeInsertButtonVisible = true;
            this.gViewer.FileName = "";
            this.gViewer.ForwardEnabled = false;
            this.gViewer.Graph = null;
            this.gViewer.InsertingEdge = false;
            this.gViewer.LayoutAlgorithmSettingsButtonVisible = true;
            this.gViewer.LayoutEditingEnabled = true;
            this.gViewer.Location = new System.Drawing.Point(0, 0);
            this.gViewer.LooseOffsetForRouting = 0.25D;
            this.gViewer.MouseHitDistance = 0.05D;
            this.gViewer.Name = "gViewer";
            this.gViewer.NavigationVisible = true;
            this.gViewer.NeedToCalculateLayout = true;
            this.gViewer.OffsetForRelaxingInRouting = 0.6D;
            this.gViewer.PaddingForEdgeRouting = 8D;
            this.gViewer.PanButtonPressed = false;
            this.gViewer.SaveAsImageEnabled = true;
            this.gViewer.SaveAsMsaglEnabled = true;
            this.gViewer.SaveButtonVisible = true;
            this.gViewer.SaveGraphButtonVisible = true;
            this.gViewer.SaveInVectorFormatEnabled = true;
            this.gViewer.Size = new System.Drawing.Size(656, 445);
            this.gViewer.TabIndex = 0;
            this.gViewer.TightOffsetForRouting = 0.125D;
            this.gViewer.ToolBarIsVisible = true;
            this.gViewer.Transform = planeTransformation1;
            this.gViewer.UndoRedoButtonsVisible = true;
            this.gViewer.WindowZoomButtonPressed = false;
            this.gViewer.ZoomF = 1D;
            this.gViewer.ZoomWindowThreshold = 0.05D;
            // 
            // DiagramEditor
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(656, 445);
            this.Controls.Add(this.gViewer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Name = "DiagramEditor";
            this.ResumeLayout(false);

		}

		private void GViewerOnDoubleClick(object sender, EventArgs e)
		{
			GViewer viewer = sender as GViewer;
			if (viewer.SelectedObject is Node node)
			{
				AbstractSchemaItem clickedItem = 
					(AbstractSchemaItem)persistenceProvider
						.RetrieveInstance(typeof(AbstractSchemaItem), new Key(node.Id));
				if(clickedItem != null)
				{
					EditSchemaItem cmd = new EditSchemaItem();
					cmd.Owner = clickedItem;
					cmd.Run();
				}
			}
		}

		#endregion

		protected override void ViewSpecificLoad(object objectToLoad)
		{
			if (!(objectToLoad is ISchemaItem graphParent)) return;
			
			_factory= new DiagramFactory(graphParent);
			gViewer.Graph = _factory.Draw();
			Guid graphParentId = graphParent.Id;
			
			persistenceProvider.InstancePersisted += (sender, persistedObject) =>
			{
				OnInstancePersisted(graphParentId, persistedObject);
			};
		}

		private void OnInstancePersisted(Guid graphParentId, IPersistent persistedObject)
		{
			if (!(persistedObject is AbstractSchemaItem persistedSchemaItem)) return;
			
			var upToDateGraphParent =
				persistenceProvider.RetrieveInstance(
						typeof(AbstractSchemaItem),
						new Key(graphParentId))
					as AbstractSchemaItem;

			bool childPersisted = upToDateGraphParent
				.ChildrenRecursive
				.Any(x => x.Id == persistedSchemaItem.Id);
			
			if (childPersisted)
			{
				Node node = gViewer.Graph.FindNode(persistedSchemaItem.Id.ToString());
				if (node == null)
				{
					_factory = new DiagramFactory(upToDateGraphParent);
					gViewer.Graph = _factory.Draw();
				}
				else
				{
					node.LabelText = persistedSchemaItem.Name;
					gViewer.Redraw();
				}
				return;
			}
			
			if (persistedSchemaItem.IsDeleted)
			{
				Node node = gViewer.Graph.FindNode(persistedSchemaItem.Id.ToString());
				if (node == null) return;
				
				IViewerNode viewerNode = gViewer.FindViewerNode(node);
				gViewer.RemoveNode(viewerNode, true);
				gViewer.Graph.RemoveNodeEverywhere(node);
			}
		}

		void Form1_MouseDown(object sender, MsaglMouseEventArgs e)
	    {
	        if (e.RightButtonIsPressed && !e.Handled)
	        {
		        _mouseRightButtonDownPoint = new ClickPoint( gViewer, e);

	            ContextMenuStrip cm = BuildContextMenu();
                cm.Show(this,_mouseRightButtonDownPoint.InScreenSystem);
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

		protected virtual ContextMenuStrip BuildContextMenu()
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
	}
}
