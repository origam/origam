using Microsoft.Msagl.GraphViewerGdi;

namespace Origam.Workbench.Diagram.Extensions
{
    public static class GViewerExtensions
    {
        public static void Redraw(this GViewer gViewer)
        {
            var graph = gViewer.Graph;
            gViewer.Graph = null;
            gViewer.Graph = graph;
        }
    }
}