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
using Microsoft.Msagl.Core.Layout;
using Origam.Workbench.Diagram.Extensions;
using Node = Microsoft.Msagl.Drawing.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram.NodeDrawing;

internal class ActionSubgraphPainter : INodeItemPainter
{
    private readonly InternalPainter painter;
    private readonly double labelLeftMargin = 10;

    public ActionSubgraphPainter(InternalPainter internalPainter)
    {
        painter = internalPainter;
    }

    public ICurve GetBoundary(Node node)
    {
        var clusterBoundary = ((Cluster)node.GeometryNode).RectangularBoundary;
        var height = clusterBoundary.TopMargin;
        var labelWidth = painter.GetLabelWidth(node);
        var width =
            clusterBoundary.MinWidth > labelWidth
                ? clusterBoundary.MinWidth
                : labelWidth + painter.LabelSideMargin * 2;
        return CurveFactory.CreateRectangle(width, height, new Point());
    }

    public bool Draw(Node node, object graphicsObj)
    {
        INodeData nodeData = (INodeData)node.UserData;
        var borderSize = new Size((int)node.BoundingBox.Width, (int)node.BoundingBox.Height);
        Graphics editorGraphics = (Graphics)graphicsObj;
        double centerX = node.GeometryNode.Center.X;
        double centerY = node.GeometryNode.Center.Y;
        var labelPoint = new PointF(
            (float)(centerX - borderSize.Width / 2 + labelLeftMargin),
            (float)centerY - borderSize.Height / 2.0f + painter.LabelTopMargin
        );

        editorGraphics.DrawUpSideDown(
            drawAction: graphics =>
            {
                graphics.DrawString(
                    nodeData.Text,
                    painter.Font,
                    painter.BlackBrush,
                    labelPoint,
                    painter.DrawFormat
                );
            },
            yAxisCoordinate: (float)node.GeometryNode.Center.Y
        );
        return true;
    }
}
