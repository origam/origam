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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.DA.ObjectPersistence;
using Origam.Gui.UI;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.BaseComponents;
using Origam.Workbench.Diagram;
using Origam.Workbench.Diagram.DiagramFactory;
using Origam.Workbench.Diagram.InternalEditor;
using Origam.Workbench.Services;
using DrawingNode = Microsoft.Msagl.Drawing.Node;

namespace Origam.Workbench.Editors;

public class DiagramEditor : AbstractViewContent, IToolStripContainer
{
    private GViewer gViewer;
    private readonly IPersistenceProvider persistenceProvider;
    private IDiagramEditor internalEditor;
    private HScrollBar hScrollBar;
    private TableLayoutPanel tableLayoutPanel1;
    private readonly NodeSelector nodeSelector;
    private System.Drawing.Point lastMouseLocation;
    private WorkbenchSchemaService schemaService;

    public DiagramEditor()
    {
        schemaService = ServiceManager.Services.GetService<WorkbenchSchemaService>();
        nodeSelector = new NodeSelector();
        persistenceProvider = ServiceManager
            .Services.GetService<IPersistenceService>()
            .SchemaProvider;
        InitializeComponent();
        gViewer.OutsideAreaBrush = Brushes.White;
        gViewer.EdgeAdded += (sender, args) => gViewer.InsertingEdge = false;
        gViewer.LayoutEditor.NodeMovingEnabled = false;
        gViewer.LayoutEditor.ShouldProcessRightClickOnSelectedEdge = false;
        gViewer.FixedScale = 1;
        gViewer.MouseWheel += GViewerMouseWheel;
        gViewer.ZoomWhenMouseWheelScroll = false;
        gViewer.MouseMove += OnMouseMove;
        gViewer.MouseLeave += OnMouseLeave;
    }

    private void OnMouseLeave(object sender, EventArgs args)
    {
        lastMouseLocation = System.Drawing.Point.Empty;
    }

    private void OnMouseMove(object sender, MouseEventArgs args)
    {
        if (
            args.Button == MouseButtons.Left
            && lastMouseLocation != System.Drawing.Point.Empty
            && !gViewer.InsertingEdge
        )
        {
            int dx = args.Location.X - lastMouseLocation.X;
            int dy = args.Location.Y - lastMouseLocation.Y;
            gViewer.Pan(dx, dy);
        }
        lastMouseLocation = args.Location;
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
        Microsoft.Msagl.Core.Geometry.Curves.PlaneTransformation planeTransformation1 =
            new Microsoft.Msagl.Core.Geometry.Curves.PlaneTransformation();
        this.hScrollBar = new System.Windows.Forms.HScrollBar();
        this.gViewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
        this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        this.tableLayoutPanel1.SuspendLayout();
        this.SuspendLayout();
        //
        // hScrollBar
        //
        this.hScrollBar.Dock = System.Windows.Forms.DockStyle.Fill;
        this.hScrollBar.Location = new System.Drawing.Point(0, 325);
        this.hScrollBar.Name = "hScrollBar";
        this.hScrollBar.Size = new System.Drawing.Size(548, 20);
        this.hScrollBar.TabIndex = 1;
        this.hScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(
            this.HScrollBar_Scroll
        );
        //
        // gViewer
        //
        this.gViewer.ArrowheadLength = 10D;
        this.gViewer.AsyncLayout = false;
        this.gViewer.AutoScroll = true;
        this.gViewer.BackwardEnabled = false;
        this.gViewer.BuildHitTree = true;
        this.gViewer.CurrentLayoutMethod = Microsoft
            .Msagl
            .GraphViewerGdi
            .LayoutMethod
            .UseSettingsOfTheGraph;
        this.gViewer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.gViewer.EdgeInsertButtonVisible = true;
        this.gViewer.FileName = "";
        this.gViewer.FixedScale = 0D;
        this.gViewer.ForwardEnabled = false;
        this.gViewer.Graph = null;
        this.gViewer.InsertingEdge = false;
        this.gViewer.LayoutAlgorithmSettingsButtonVisible = true;
        this.gViewer.LayoutEditingEnabled = true;
        this.gViewer.Location = new System.Drawing.Point(3, 3);
        this.gViewer.LooseOffsetForRouting = 0.25D;
        this.gViewer.MouseHitDistance = 0.05D;
        this.gViewer.Name = "gViewer";
        this.gViewer.NavigationVisible = true;
        this.gViewer.NeedToCalculateLayout = true;
        this.gViewer.OffsetForRelaxingInRouting = 0.6D;
        this.gViewer.PaddingForEdgeRouting = 8D;
        this.gViewer.PanButtonPressed = false;
        this.gViewer.SaveAsImageEnabled = true;
        this.gViewer.SaveAsMsaglEnabled = false;
        this.gViewer.SaveButtonVisible = true;
        this.gViewer.SaveGraphButtonVisible = true;
        this.gViewer.SaveInVectorFormatEnabled = true;
        this.gViewer.Size = new System.Drawing.Size(542, 319);
        this.gViewer.TabIndex = 0;
        this.gViewer.TightOffsetForRouting = 0.125D;
        this.gViewer.ToolBarIsVisible = false;
        this.gViewer.Transform = planeTransformation1;
        this.gViewer.UndoRedoButtonsVisible = true;
        this.gViewer.WindowZoomButtonPressed = false;
        this.gViewer.ZoomF = 1D;
        this.gViewer.ZoomWhenMouseWheelScroll = true;
        this.gViewer.ZoomWindowThreshold = 0.05D;
        //
        // tableLayoutPanel1
        //
        this.tableLayoutPanel1.ColumnCount = 2;
        this.tableLayoutPanel1.ColumnStyles.Add(
            new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F)
        );
        this.tableLayoutPanel1.ColumnStyles.Add(
            new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F)
        );
        this.tableLayoutPanel1.Controls.Add(this.gViewer, 0, 0);
        this.tableLayoutPanel1.Controls.Add(this.hScrollBar, 0, 1);
        this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel1.Name = "tableLayoutPanel1";
        this.tableLayoutPanel1.RowCount = 2;
        this.tableLayoutPanel1.RowStyles.Add(
            new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
        );
        this.tableLayoutPanel1.RowStyles.Add(
            new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F)
        );
        this.tableLayoutPanel1.Size = new System.Drawing.Size(548, 345);
        this.tableLayoutPanel1.TabIndex = 2;
        //
        // DiagramEditor
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(7, 16);
        this.ClientSize = new System.Drawing.Size(548, 345);
        this.Controls.Add(this.tableLayoutPanel1);
        this.Font = new System.Drawing.Font(
            "Microsoft Sans Serif",
            8.25F,
            System.Drawing.FontStyle.Regular,
            System.Drawing.GraphicsUnit.Point,
            ((byte)(238))
        );
        this.Name = "DiagramEditor";
        this.tableLayoutPanel1.ResumeLayout(false);
        this.ResumeLayout(false);
    }
    #endregion
    void GViewerMouseWheel(object sender, MouseEventArgs e)
    {
        int delta = e.Delta / 3;
        if (delta != 0)
        {
            gViewer.Pan(0, delta);
        }
    }

    protected override void ViewSpecificLoad(object objectToLoad)
    {
        switch (objectToLoad)
        {
            case IWorkflowBlock workflowBlock:
            {
                internalEditor = new WorkFlowDiagramEditor(
                    graphParentId: workflowBlock.Id,
                    gViewer: gViewer,
                    parentForm: this,
                    persistenceProvider: persistenceProvider,
                    factory: new WorkFlowDiagramFactory(nodeSelector, gViewer, schemaService),
                    nodeSelector: nodeSelector
                );
                break;
            }

            case IContextStore contextStore:
            {
                internalEditor = new GeneralDiagramEditor<IContextStore>(
                    gViewer: gViewer,
                    schemaItem: contextStore,
                    factory: new ContextStoreDiagramFactory(
                        persistenceProvider,
                        nodeSelector,
                        gViewer,
                        schemaService
                    ),
                    persistenceProvider: persistenceProvider
                );
                break;
            }

            case ISchemaItem schemaItem:
            {
                internalEditor = new GeneralDiagramEditor<ISchemaItem>(
                    gViewer: gViewer,
                    schemaItem: schemaItem,
                    factory: new GeneralDiagramFactory(),
                    persistenceProvider: persistenceProvider
                );
                break;
            }
        }
    }

    public List<ToolStrip> GetToolStrips(int maxWidth = -1)
    {
        LabeledToolStrip toolStrip = new LabeledToolStrip(this);
        toolStrip.Text = Diagram.Strings.DiagramEditor_ToolStrip_Title;

        BigToolStripButton zoomHomeButton = new BigToolStripButton();
        zoomHomeButton.Text = Diagram.Strings.DiagramEditor_ToolStrip_Zoom_Home;
        zoomHomeButton.Image = ImageRes.UnknownIcon;
        zoomHomeButton.Click += ZoomHome;
        toolStrip.Items.Add(zoomHomeButton);
        BigToolStripButton zoomInButton = new BigToolStripButton();
        zoomInButton.Text = Diagram.Strings.DiagramEditor_ToolStrip_Zoom_PLUS;
        zoomInButton.Image = ImageRes.UnknownIcon;
        zoomInButton.Click += (sender, args) => gViewer.ZoomInPressed();
        toolStrip.Items.Add(zoomInButton);

        BigToolStripButton zoomOutButton = new BigToolStripButton();
        zoomOutButton.Text = Diagram.Strings.DiagramEditor_ToolStrip_Zoom_MINUS;
        zoomOutButton.Image = ImageRes.UnknownIcon;
        zoomOutButton.Click += (sender, args) => gViewer.ZoomOutPressed();
        toolStrip.Items.Add(zoomOutButton);

        BigToolStripButton edgeButton = new BigToolStripButton();
        edgeButton.Text = Diagram.Strings.DiagramEditor_ToolStrip_Dependency;
        edgeButton.Image = ImageRes.UnknownIcon;
        edgeButton.Click += ToggleInsertEdge;
        toolStrip.Items.Add(edgeButton);
        return new List<ToolStrip> { toolStrip };
    }

    private void ToggleInsertEdge(object sender, EventArgs e)
    {
        gViewer.InsertingEdge = !gViewer.InsertingEdge;
    }

    private void ZoomHome(object sender, EventArgs e)
    {
        gViewer.Transform = null;
        gViewer.Invalidate();
    }

    public event EventHandler ToolStripsLoaded
    {
        add { }
        remove { }
    }
    public event EventHandler AllToolStripsRemoved
    {
        add { }
        remove { }
    }
    public event EventHandler ToolStripsNeedUpdate
    {
        add { }
        remove { }
    }

    private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
    {
        if (e.NewValue == e.OldValue)
        {
            return;
        }

        var focusSubgraph = gViewer.Graph.RootSubgraph.Subgraphs.FirstOrDefault();
        if (focusSubgraph == null)
        {
            return;
        }

        double distanceFromCenter =
            (focusSubgraph.Width / 100 * e.NewValue) - (focusSubgraph.Width / 2);
        gViewer.CenterToXCoordinate(focusSubgraph.Pos.X + distanceFromCenter);
    }
    //        private void VScrollBar_Scroll(object sender, ScrollEventArgs e)
    //        {
    //	        if (e.NewValue == e.OldValue) return;
    //	        var focusSubgraph = gViewer.Graph.RootSubgraph.Subgraphs.FirstOrDefault();
    //	        if (focusSubgraph == null) return;
    //
    //	        double distanceFromCenter = -(focusSubgraph.Height / 100 * (100 - e.NewValue) - focusSubgraph.Height / 2);
    //	        gViewer.CenterToYCoordinate(focusSubgraph.Pos.Y + distanceFromCenter);
    //        }
}
