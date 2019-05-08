using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;

namespace Origam.Workbench.Diagram.NodeDrawing
{
    internal interface INodeItemPainter
    {
        ICurve GetBoundary(Node node);
        bool Draw(Node node, object graphicsObj);
    }
}