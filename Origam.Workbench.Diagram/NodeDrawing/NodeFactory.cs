using System.Drawing;
using System.Linq;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using Origam.Gui.UI;
using Origam.Schema;
using Origam.Schema.EntityModel;
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

        public Node AddNodeItem(Graph graph, ISchemaItem schemaItem,
            int leftMargin)
        {
            INodeData nodeData = new NodeData(schemaItem, leftMargin, schemaService);
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
            parentSbubgraph.AddSubgraph(subgraph);
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
        Image Icon { get; set; }
        bool IsFromActivePackage { get; }
        string Id { get; }
        int LeftMargin { get; }
    }

    class NodeData : INodeData
    {
        private readonly WorkbenchSchemaService schemaService;
        public ISchemaItem SchemaItem { get; }
        public string Text => SchemaItem.Name;
        public Image Icon { get; set; }
        public bool IsFromActivePackage =>
            SchemaItem.SchemaExtension.Id == schemaService.ActiveSchemaExtensionId;
        public string Id => SchemaItem.Id.ToString();
        public int LeftMargin { get; }

        public NodeData(ISchemaItem schemaItem, WorkbenchSchemaService schemaService)
            : this(schemaItem, 0, schemaService)
        {
           
        }

        public NodeData(ISchemaItem schemaItem, int leftMargin, WorkbenchSchemaService schemaService)
        {
            this.schemaService = schemaService;
            SchemaItem = schemaItem;
            LeftMargin = leftMargin;
        }
    }
}