using Microsoft.Msagl.Drawing;
using Origam.Schema;

namespace Origam.Workbench.Diagram.DiagramFactory
{
    public interface IDiagramFactory<in T> where T : ISchemaItem
    {
        Graph Draw(T graphParent);
    }
}