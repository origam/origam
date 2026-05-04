#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Drawing;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Origam.Workbench.Diagram.Extensions;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram.NodeDrawing;

internal class NodePainter : INodeItemPainter
{
    private readonly InternalPainter painter;
    private readonly NodeHeaderPainter nodeHeaderPainter;

    public NodePainter(InternalPainter painter)
    {
        this.painter = painter;
        nodeHeaderPainter = new NodeHeaderPainter(painter: painter);
    }

    public ICurve GetBoundary(Node node)
    {
        var borderSize = painter.CalculateMinHeaderBorder(node: node);
        return CurveFactory.CreateRectangle(
            width: borderSize.Width,
            height: borderSize.Height,
            center: new Point()
        );
    }

    public bool Draw(Node node, object graphicsObj)
    {
        Graphics editorGraphics = (Graphics)graphicsObj;
        var borderSize = painter.CalculateMinHeaderBorder(node: node);
        var borderCorner = new System.Drawing.Point(
            x: (int)node.GeometryNode.Center.X - (borderSize.Width / 2),
            y: (int)node.GeometryNode.Center.Y - (borderSize.Height / 2)
        );
        Rectangle border = new Rectangle(location: borderCorner, size: borderSize);

        nodeHeaderPainter.Draw(node: node, editorGraphics: editorGraphics, border: border);

        editorGraphics.DrawUpSideDown(
            drawAction: graphics =>
            {
                graphics.DrawRectangle(pen: painter.GetActiveBorderPen(node: node), rect: border);
            },
            yAxisCoordinate: (float)node.GeometryNode.Center.Y
        );
        return true;
    }
}
