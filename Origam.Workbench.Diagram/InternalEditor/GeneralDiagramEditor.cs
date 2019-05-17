using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.Schema;
using Origam.Workbench.Diagram.DiagramFactory;

namespace Origam.Workbench.Diagram.InternalEditor
{
    public class GeneralDiagramEditor<T>: IDiagramEditor where T: ISchemaItem
    {
        public GeneralDiagramEditor( GViewer gViewer, T schemaItem, IDiagramFactory<T,Graph> factory)
        {           
            gViewer.Graph = factory.Draw(schemaItem);
            gViewer.EdgeInsertButtonVisible = false;
        }

        public void Dispose()
        {
        }

        public void ReDrawAndReselect()
        {
            
        }
    }
}