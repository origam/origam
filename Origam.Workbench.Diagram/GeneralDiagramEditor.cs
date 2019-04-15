using Microsoft.Msagl.GraphViewerGdi;
using Origam.Schema;

namespace Origam.Workbench.Diagram
{
    public class GeneralDiagramEditor<T>: IDiagramEditor where T: ISchemaItem
    {
        public GeneralDiagramEditor( GViewer gViewer, T schemaItem, IDiagramFactory<T> factory)
        {           
            gViewer.Graph = factory.Draw(schemaItem);
        }

        public void Dispose()
        {
        }
    }
}