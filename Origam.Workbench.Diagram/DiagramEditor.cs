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
using Origam.Workbench.Diagram.DiagramFactory;
using Origam.Workbench.Diagram.Extensions;
using Origam.Workbench.Diagram.InternalEditor;
using Origam.Workbench.Services;
using Point = Microsoft.Msagl.Core.Geometry.Point;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Origam.Workbench.Editors
{
	public class DiagramEditor : AbstractViewContent
	{
        private GViewer gViewer;
        private readonly IPersistenceProvider persistenceProvider;
        private IDiagramEditor internalEditor;

        public DiagramEditor()
		{
			persistenceProvider = ServiceManager.Services
				.GetService<IPersistenceService>()
				.SchemaProvider;
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
        {
	        if (disposing)
	        {
		        internalEditor.Dispose();
	        }

	        base.Dispose(disposing);
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
            this.gViewer.SaveAsMsaglEnabled = false;
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

		#endregion

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
		
		protected override void ViewSpecificLoad(object objectToLoad)
		{
			switch (objectToLoad)
			{
				case IWorkflowBlock workflowBlock:
					internalEditor = new WorkFlowDiagramEditor(
						graphParentId: workflowBlock.Id,
						gViewer: gViewer,
						parentForm: this,
						persistenceProvider: persistenceProvider,
						factory: new WorkFlowDiagramFactory(gViewer));
					break;
				case IContextStore contextStore:
					internalEditor = new GeneralDiagramEditor<IContextStore>(
						gViewer: gViewer,
						schemaItem: contextStore,
						factory: new ContextStoreDiagramFactory(persistenceProvider, gViewer));
					break;
				case ISchemaItem schemaItem:
					internalEditor = new GeneralDiagramEditor<ISchemaItem>(
						gViewer: gViewer, 
						schemaItem: schemaItem, 
						factory: new GeneralDiagramFactory());
					break;
			}
		}
	}
}
