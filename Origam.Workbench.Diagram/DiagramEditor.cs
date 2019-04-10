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
using System.Linq;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Workbench.Diagram;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.DA.ObjectPersistence;
using Origam.UI;
using Origam.Workbench.Commands;
using Origam.Workbench.Diagram.Extensions;
using Origam.Workbench.Services;
using Point = Microsoft.Msagl.Core.Geometry.Point;
using DrawingNode = Microsoft.Msagl.Drawing.Node;

namespace Origam.Workbench.Editors
{
	/// <summary>
	/// Summary description for DiagramEditor.
	/// </summary>
	public class DiagramEditor : AbstractViewContent
	{
        private Graph graph = new Graph();
		DiagramFactory _factory;
        private GViewer gViewer;
        private int idcounter = 0;

        private System.ComponentModel.Container components = null;

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

        private ClickPoint _mouseRightButtonDownPoint;

	    public DiagramEditor()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		    (gViewer as IViewer).MouseDown += Form1_MouseDown;
		}
	    
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
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
					(AbstractSchemaItem)ServiceManager
						.Services.GetService<IPersistenceService>()
						.SchemaProvider
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
			if(objectToLoad is ISchemaItem schemaItem)
			{
			    _factory= new DiagramFactory(graph, schemaItem);
                _factory.DrawDiagram();
                gViewer.Graph = _factory.Graph;
			}
		}
	    void Form1_MouseDown(object sender, MsaglMouseEventArgs e)
	    {
	        if (e.RightButtonIsPressed && !e.Handled)
	        {
		        _mouseRightButtonDownPoint = new ClickPoint( gViewer, e);

	            ContextMenuStrip cm = BuildContextMenu(_mouseRightButtonDownPoint.InMsaglSystem);
                cm.Show(this, new System.Drawing.Point(e.X, e.Y));
	        }
	    }

        protected virtual ContextMenuStrip BuildContextMenu(Point point)
	    {
	        var cm = new ContextMenuStrip();
	        //	        mi = new MenuItem();
	        //	        mi.Text = "Delete selected";
	        //	        mi.Click += deleteSelected_Click;
	        //	        cm.MenuItems.Add(mi);

	        var builder = new SchemaItemEditorsMenuBuilder();
	        var submenu = builder.BuildSubmenu(null).OfType<AsMenuCommand>();
	        foreach (AsMenuCommand item in submenu)
	        {
                cm.Items.Add(item);
                ISchemaItemFactory parentSchemItem = (item.Command as AddNewSchemaItem)?.ParentElement;
                parentSchemItem.ItemCreated += OnChildAdded;
	        }

            return cm;
	    }

        private void OnChildAdded(ISchemaItem schemaItem)
        {
	        var objectAt = gViewer.GetObjectAt(_mouseRightButtonDownPoint.InScreenSystem);
	        Subgraph subgraph = (objectAt as DNode)?.Node as Subgraph;
	        if (subgraph == null) return;

	        string nodeId = schemaItem.Id.ToString();
	        string nodeName = string.IsNullOrEmpty(schemaItem.Name)
		        ? "New Node"
		        : schemaItem.Name;

	        Node shape = gViewer.Graph.AddNode(nodeId);
	        shape.LabelText = nodeName;
	        subgraph.AddNode(shape);
	        gViewer.Redraw();

	        ServiceManager.Services.GetService<IPersistenceService>()
		        .SchemaProvider.InstancePersisted += (sender, args) =>
	        {
		        if (sender is ISchemaItem persistedSchemaItem &&
		            persistedSchemaItem.Id == schemaItem.Id)
		        {
			        Node node = gViewer.Graph.FindNode(schemaItem.Id.ToString());
			        node.LabelText = persistedSchemaItem.Name;
			        gViewer.Redraw();
		        }
	        };
        }
	}
}
