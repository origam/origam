using Microsoft.Msagl.Drawing;
using Origam.Schema;

namespace Origam.Workbench.Diagram
{
    class GeneralDiagramFactory: IDiagramFactory<ISchemaItem>
    {
        private Graph graph;
		
        public Graph Draw(ISchemaItem item)
        {
            graph = new Graph();
            DrawUniShape(item, null);
            return graph;
        }

        private void DrawUniShape(ISchemaItem schemaItem, Node parentShape)
        {
            Node shape = this.AddNode(schemaItem.Id.ToString(), schemaItem.Name);
            if(parentShape != null)
            {
                this.graph.AddEdge(shape.Id, parentShape.Id);
            }
            foreach(ISchemaItem child in schemaItem.ChildItems)
            {
                DrawUniShape(child, shape);
            }
        }
		
        public Node AddNode(string id, string label)
        {
            Node shape = graph.AddNode(id);
            shape.LabelText = label;
            return shape;
        }
    }
}