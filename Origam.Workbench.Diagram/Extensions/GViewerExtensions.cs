using System.Linq;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;

namespace Origam.Workbench.Diagram.Extensions
{
    public static class GViewerExtensions
    {

        public static IViewerNode FindViewerNode(this GViewer gViewer, Node node)
        {
            return gViewer
                .Entities.OfType<IViewerNode>()
                .SingleOrDefault(viewerNode => viewerNode.Node == node);
        }
       
        public static void Redraw(this GViewer gViewer)
        {
            var graph = gViewer.Graph;
            gViewer.Graph = null;
            gViewer.Graph = graph;
        }
    }
}