using Microsoft.Msagl.Drawing;
using Origam.Schema;
using Node = Microsoft.Msagl.Drawing.Node;

namespace Origam.Workbench.Diagram.NodeDrawing
{
    class NodeFactory
    {
        private readonly InternalPainter internalPainter;

        public NodeFactory(INodeSelector nodeSelector)
        {
            internalPainter = new InternalPainter(nodeSelector);
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
    }
}