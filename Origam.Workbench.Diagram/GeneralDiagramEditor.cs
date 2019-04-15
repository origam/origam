using Microsoft.Msagl.GraphViewerGdi;
using Origam.Schema;

namespace Origam.Workbench.Diagram
{
    public class GeneralDiagramEditor: IDiagramEditor
    {
        private DiagramFactory _factory;
        private readonly GViewer gViewer;

        public GeneralDiagramEditor( GViewer gViewer, ISchemaItem schemaItem)
        {
            this.gViewer = gViewer;
            
            _factory = new DiagramFactory(schemaItem);
            gViewer.Graph = _factory.Draw();
        }

        public void Dispose()
        {
        }
    }
}