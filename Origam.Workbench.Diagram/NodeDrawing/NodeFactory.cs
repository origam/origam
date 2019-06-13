using System.Drawing;
using System.Linq;
using System.Net.Mime;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using Origam.Extensions;
using Origam.Gui.UI;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Diagram.Graphs;
using Origam.Workbench.Services;
using Node = Microsoft.Msagl.Drawing.Node;

namespace Origam.Workbench.Diagram.NodeDrawing
{
    class NodeFactory
    {
        private readonly InternalPainter internalPainter;
        private readonly WorkbenchSchemaService schemaService;
        private static int balloonNumber = 0;

        public NodeFactory(INodeSelector nodeSelector, GViewer gViewer, 
            WorkbenchSchemaService schemaService)
        {
            this.schemaService = schemaService;
            internalPainter = new InternalPainter(nodeSelector, gViewer);
        }

        public Node AddNode(Graph graph, ISchemaItem schemaItem)
        {
            INodeData nodeData = new NodeData(schemaItem, schemaService);
            Node node = graph.AddNode(nodeData.Id);
            node.Attr.Shape = Shape.DrawFromGeometry;
            var painter =  new NodePainter(internalPainter);
            node.DrawNodeDelegate = painter.Draw;
            node.NodeBoundaryDelegate = painter.GetBoundary;
            node.UserData = nodeData;
            node.LabelText = nodeData.Text;
            return node;
        }

        public Node AddNodeItem(Graph graph, INodeData nodeData)
        {
            Node node = graph.AddNode(nodeData.Id);
            node.Attr.Shape = Shape.DrawFromGeometry;
            var painter =
                new NodeItemPainter(internalPainter);
            node.DrawNodeDelegate = painter.Draw;
            node.NodeBoundaryDelegate = painter.GetBoundary;
            node.UserData = nodeData;
            node.LabelText = nodeData.Text;
            return node;
        }

        public Subgraph AddSubgraphNode(Subgraph parentSbubgraph,
            ISchemaItem schemaItem)
        {
            INodeData nodeData = new NodeData(schemaItem, schemaService);
            Subgraph subgraph = new Subgraph(nodeData.Id);
            subgraph.Attr.Shape = Shape.DrawFromGeometry;
            var painter =
                new SubgraphNodePainter(internalPainter);
            subgraph.DrawNodeDelegate = painter.Draw;
            subgraph.NodeBoundaryDelegate = painter.GetBoundary;
            subgraph.UserData = nodeData;
            subgraph.LabelText = nodeData.Text;
            if (parentSbubgraph is BlockSubGraph blockSubGraph)
            {
                blockSubGraph.MainDrawingSubgraf.AddSubgraph(subgraph);
            }
            else
            {
                parentSbubgraph.AddSubgraph(subgraph);
            }
            return subgraph;
        }
        
        public BlockSubGraph AddSubgraph(Subgraph parentSbubgraph,
            IWorkflowBlock schemaItem)
        {
            INodeData nodeData = new NodeData(schemaItem, schemaService);
            BlockSubGraph subgraph = new BlockSubGraph(nodeData.Id);
            subgraph.Attr.Shape = Shape.DrawFromGeometry;
            var painter = new SubgraphPainter(internalPainter);
            subgraph.DrawNodeDelegate = painter.Draw;
            subgraph.NodeBoundaryDelegate = painter.GetBoundary;
            subgraph.UserData = nodeData;
            subgraph.LabelText = nodeData.Text;
            parentSbubgraph.AddSubgraph(subgraph);
            return subgraph;
        }

        public Subgraph AddActionSubgraph(Subgraph parentSbubgraph, ISchemaItem schemaItem)
        {
            INodeData nodeData = new NodeItemLabel(schemaItem.Name);
            Subgraph subgraph = new Subgraph(nodeData.Id);
            subgraph.Attr.Shape = Shape.DrawFromGeometry;
            var painter =
                new ActionSubgraphPainter(internalPainter);
            subgraph.DrawNodeDelegate = painter.Draw;
            subgraph.NodeBoundaryDelegate = painter.GetBoundary;
            subgraph.UserData = nodeData;

            subgraph.LayoutSettings = new SugiyamaLayoutSettings
            {
                PackingMethod = PackingMethod.Compact,
                PackingAspectRatio = 1000,
                AdditionalClusterTopMargin = 20,
                ClusterMargin = 10
            };
            
            parentSbubgraph.AddSubgraph(subgraph);
            return subgraph;
        }
        
        
        public void AddActionNode(Subgraph actionSubgraph, EntityUIAction action)
        {
            INodeData nodeData = new NodeData(action, schemaService);
            Subgraph subgraph = new Subgraph(nodeData.Id);
  
            subgraph.Attr.Shape = Shape.DrawFromGeometry;
            var painter =
                new ActionNodePainter(internalPainter);
            subgraph.DrawNodeDelegate = painter.Draw;
            subgraph.NodeBoundaryDelegate = painter.GetBoundary;
            subgraph.UserData = nodeData;
            actionSubgraph.AddSubgraph(subgraph);
        }
        
        public Node AddStarBalloon(Graph graph)
        {
            return AddBalloon(graph, internalPainter.GreenBrush, "Start");
        }
        
        public Node AddEndBalloon(Graph graph)
        {
            return AddBalloon(graph, internalPainter.RedBrush, "End");
        }

        private Node AddBalloon(Graph graph, SolidBrush balloonBrush, string label)
        {
            Node node = graph.AddNode($"{label} balloon {balloonNumber++}");
            node.Attr.Shape = Shape.DrawFromGeometry;
            var painter = new BalloonPainter(internalPainter, balloonBrush);
            node.DrawNodeDelegate = painter.Draw;
            node.NodeBoundaryDelegate = painter.GetBoundary;
            node.LabelText = label;
            return node;
        }
    }

    internal interface INodeData
    {
        ISchemaItem SchemaItem { get; }
        string Text { get; }
        Image PrimaryImage { get;}
        Image SecondaryImage { get;}
        bool IsFromActivePackage { get; }
        string Id { get; }
        int LeftMargin { get; }
    }

    class NodeItemLabel: INodeData
    {
        private static int lastId;
        
        public ISchemaItem SchemaItem { get; }
        public string Text { get; }
        public Image PrimaryImage { get; }
        public Image SecondaryImage { get; }
        public bool IsFromActivePackage { get; } = true;
        public string Id { get; }
        public int LeftMargin { get; }

        public NodeItemLabel(string text)
        {
            Text = text;
            Id = "NodeItemLabel_" + lastId++;
        }

        public NodeItemLabel(string text, int leftMargin):this(text)
        {
            LeftMargin = leftMargin;
        }
    }

    class ActionNodeData : NodeData
    {
        public ActionNodeData(EntityMenuAction action, int leftMargin, WorkbenchSchemaService schemaService
        ) 
            : base(action, schemaService)
        {
            LeftMargin = leftMargin;
        }
    }

    class NodeItemData: NodeData
    {
        public NodeItemData(ISchemaItem schemaItem, int leftMargin, WorkbenchSchemaService schemaService)
            : base(schemaItem, schemaService)
        {
            LeftMargin = leftMargin;
        }
    }


    class NodeData : INodeData
    {
        private readonly WorkbenchSchemaService schemaService;
        private Image primaryImage;
        private Image secondaryImage;
        
        public virtual Image PrimaryImage
        {
            get
            {
                if (primaryImage == null)
                {
                    if (SchemaItem.NodeImage != null)
                    {
                        primaryImage = SchemaItem.NodeImage.ToBitmap();
                        return primaryImage;
                    }
                    primaryImage = GetImage(SchemaItem.Icon);
                }

                return primaryImage;
            }
        }

        public Image SecondaryImage {
            get
            {
                if (secondaryImage  == null &&
                    SchemaItem is AbstractWorkflowStep workflowStep &&
                    workflowStep.StartConditionRule != null)
                {
                    secondaryImage = GetImage(workflowStep.StartConditionRule.Icon);
                }
                return secondaryImage;
            }
        }
        public ISchemaItem SchemaItem { get; }
        public string Text => SchemaItem.Name;
        public bool IsFromActivePackage =>
            SchemaItem.SchemaExtension.Id == schemaService.ActiveSchemaExtensionId;
        public string Id => SchemaItem.Id.ToString();
        public int LeftMargin { get; protected set; } = 0;

        public NodeData(ISchemaItem schemaItem, WorkbenchSchemaService schemaService)
        {
            this.schemaService = schemaService;
            SchemaItem = schemaItem;
        }
        
        private Image GetImage(string iconId)
        {
            var schemaBrowser =
                WorkbenchSingleton.Workbench.GetPad(typeof(IBrowserPad)) as
                    IBrowserPad;
            var imageList = schemaBrowser.ImageList;
            return imageList.Images[schemaBrowser.ImageIndex(iconId)];
        }
    }
}