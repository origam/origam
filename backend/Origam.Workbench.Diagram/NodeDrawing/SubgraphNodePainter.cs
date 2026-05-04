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
using System.Linq;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Origam.Workbench.Diagram.Extensions;
using Node = Microsoft.Msagl.Drawing.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram.NodeDrawing;

internal class SubgraphNodePainter : INodeItemPainter
{
    private readonly InternalPainter painter;
    private readonly NodePainter nodePainter;
    private readonly NodeHeaderPainter nodeHeaderPainter;
    private readonly INodeFooterPainter footerPainter;

    public SubgraphNodePainter(InternalPainter internalPainter)
    {
        painter = internalPainter;
        nodePainter = new NodePainter(painter: internalPainter);
        nodeHeaderPainter = new NodeHeaderPainter(painter: internalPainter);
        footerPainter = new FooterPainter();
    }

    public ICurve GetBoundary(Node node)
    {
        Subgraph subgraph = (Subgraph)node;
        if (!subgraph.Nodes.Any() && !subgraph.Subgraphs.Any())
        {
            return nodePainter.GetBoundary(node: node);
        }

        var clusterBoundary = ((Cluster)node.GeometryNode).RectangularBoundary;
        var height = clusterBoundary.TopMargin + footerPainter.GetHeight(node: node);
        var labelWidth = GetLabelWidth(node: node);
        var width =
            clusterBoundary.MinWidth > labelWidth
                ? clusterBoundary.MinWidth
                : labelWidth + (painter.LabelSideMargin * 2);
        return CurveFactory.CreateRectangle(width: width, height: height, center: new Point());
    }

    public bool Draw(Node node, object graphicsObj)
    {
        Subgraph subgraph = (Subgraph)node;
        if (!subgraph.Nodes.Any())
        {
            return nodePainter.Draw(node: node, graphicsObj: graphicsObj);
        }
        var borderSize = new Size(
            width: (int)node.BoundingBox.Width,
            height: (int)node.BoundingBox.Height
        );
        var borderCorner = new System.Drawing.Point(
            x: (int)node.GeometryNode.Center.X - (borderSize.Width / 2),
            y: (int)node.GeometryNode.Center.Y - (borderSize.Height / 2)
        );
        Rectangle border = new Rectangle(location: borderCorner, size: borderSize);
        Rectangle headerBorder = new Rectangle(
            x: borderCorner.X,
            y: border.Bottom - painter.NodeHeaderHeight,
            width: border.Width,
            height: painter.NodeHeaderHeight
        );
        Graphics editorGraphics = (Graphics)graphicsObj;
        nodeHeaderPainter.Draw(node: node, editorGraphics: editorGraphics, border: headerBorder);
        editorGraphics.DrawUpSideDown(
            drawAction: graphics =>
            {
                graphics.DrawRectangle(pen: painter.GetActiveBorderPen(node: node), rect: border);
            },
            yAxisCoordinate: (float)node.GeometryNode.Center.Y
        );

        return true;
    }

    private float GetLabelWidth(Node node)
    {
        SizeF stringSize = painter.MeasureString(nodeLabelText: node.LabelText);
        var labelWidth =
            stringSize.Width + painter.NodeHeaderHeight + painter.Margin + painter.TextSideMargin;
        return labelWidth;
    }
}

interface INodeFooterPainter
{
    double GetHeight(Node node);
}

class FooterPainter : INodeFooterPainter
{
    public double GetHeight(Node node)
    {
        return 0;
    }
}
