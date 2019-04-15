using Microsoft.Msagl.Drawing;
using Origam.Schema;

namespace Origam.Workbench.Diagram
{
    public interface IDiagramFactory<in T> where T : ISchemaItem
    {
        Graph Draw(T graphParent);
    }
}