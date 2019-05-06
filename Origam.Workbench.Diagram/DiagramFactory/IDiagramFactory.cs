using Microsoft.Msagl.Drawing;
using Origam.Schema;

namespace Origam.Workbench.Diagram.DiagramFactory
{
    public interface IDiagramFactory<in Titem, out Tgraph> where Titem : ISchemaItem where Tgraph: Graph
    {
        Tgraph Draw(Titem graphParent);
    }
}