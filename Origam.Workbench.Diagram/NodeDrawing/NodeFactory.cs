using System.Drawing;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using Origam.Schema;
using Node = Microsoft.Msagl.Drawing.Node;

namespace Origam.Workbench.Diagram.NodeDrawing
{
    class NodeFactory
    {
        private readonly InternalPainter internalPainter;
        private static int balloonNumber = 0;

        public NodeFactory(INodeSelector nodeSelector, GViewer gViewer)
        {
            internalPainter = new InternalPainter(nodeSelector, gViewer);
        }

        public Node AddNode(Graph graph, ISchemaItem schemaItem)
        {
            Node node = graph.AddNode(schemaItem.Id.ToString());
            node.Attr.Shape = Shape.DrawFromGeometry;
            var painter =  new NodePainter(internalPainter);
            node.DrawNodeDelegate = painter.Draw;
            node.NodeBoundaryDelegate = painter.GetBoundary;
            node.UserData = schemaItem;
            node.LabelText = schemaItem.Name;
            return node;
        }

        public Node AddNodeItem(Graph graph, ISchemaItem schemaItem,
            int leftMargin)
        {
            Node node = graph.AddNode(schemaItem.Id.ToString());
            node.Attr.Shape = Shape.DrawFromGeometry;
            var painter = new NodeItemPainter(internalPainter, leftMargin);
            node.DrawNodeDelegate = painter.Draw;
            node.NodeBoundaryDelegate = painter.GetBoundary;
            node.UserData = schemaItem;
            node.LabelText = schemaItem.Name;
            return node;
        }

        public Subgraph AddSubgraphNode(Subgraph parentSbubgraph,
            ISchemaItem schemaItem)
        {
            Subgraph subgraph = new Subgraph(schemaItem.Id.ToString());
            subgraph.Attr.Shape = Shape.DrawFromGeometry;
            var painter = new SubgraphNodePainter(internalPainter);
            subgraph.DrawNodeDelegate = painter.Draw;
            subgraph.NodeBoundaryDelegate = painter.GetBoundary;
            subgraph.UserData = schemaItem;
            subgraph.LabelText = schemaItem.Name;
            parentSbubgraph.AddSubgraph(subgraph);
            return subgraph;
        }

        public Subgraph AddSubgraph(Subgraph parentSbubgraph,
            ISchemaItem schemaItem)
        {
            Subgraph subgraph = new Subgraph(schemaItem.NodeId);
            subgraph.Attr.Shape = Shape.DrawFromGeometry;
            var painter = new SubgraphPainter(internalPainter);
            subgraph.DrawNodeDelegate = painter.Draw;
            subgraph.NodeBoundaryDelegate = painter.GetBoundary;
            subgraph.UserData = schemaItem;
            subgraph.LabelText = schemaItem.Name;
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
}