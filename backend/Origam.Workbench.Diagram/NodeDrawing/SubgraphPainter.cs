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

using System;
using System.Drawing;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Origam.Workbench.Diagram.Extensions;
using Origam.Workbench.Diagram.Graphs;
using Node = Microsoft.Msagl.Drawing.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram.NodeDrawing;

internal class SubgraphPainter : INodeItemPainter
{
    private readonly InternalPainter painter;
    private readonly int emptySubgraphWidth = 200;
    private readonly int emptySubgraphHeight = 80;
    private readonly int imageGap = 10;

    public SubgraphPainter(InternalPainter internalPainter)
    {
        painter = internalPainter;
    }

    public ICurve GetBoundary(Node node)
    {
        if (((BlockSubGraph)node).IsEmpty)
        {
            return CurveFactory.CreateRectangle(
                width: emptySubgraphWidth,
                height: emptySubgraphHeight,
                center: new Point()
            );
        }

        var clusterBoundary = ((Cluster)node.GeometryNode).RectangularBoundary;
        var height = clusterBoundary.TopMargin;
        var labelWidth = painter.GetLabelWidth(node: node);
        var width =
            clusterBoundary.MinWidth > labelWidth
                ? clusterBoundary.MinWidth
                : labelWidth + (painter.LabelSideMargin * 2);
        return CurveFactory.CreateRectangle(width: width, height: height, center: new Point());
    }

    public bool Draw(Node node, object graphicsObj)
    {
        INodeData nodeData = (INodeData)node.UserData;
        var borderSize = new Size(
            width: (int)node.BoundingBox.Width,
            height: (int)node.BoundingBox.Height
        );
        Graphics editorGraphics = (Graphics)graphicsObj;
        var labelWidth = painter.GetLabelWidth(node: node);
        double centerX = node.GeometryNode.Center.X;
        double centerY = node.GeometryNode.Center.Y;
        var borderCorner = new System.Drawing.Point(
            x: (int)centerX - (borderSize.Width / 2),
            y: (int)centerY - (borderSize.Height / 2)
        );
        Rectangle border = new Rectangle(location: borderCorner, size: borderSize);
        var labelPoint = new PointF(
            x: (float)(
                centerX
                - (labelWidth / 2)
                + painter.ImageLeftMargin
                + nodeData.PrimaryImage.Width
                + painter.ImageRightMargin
            ),
            y: (float)centerY - (border.Height / 2.0f) + painter.LabelTopMargin
        );
        var imagePoint = new PointF(
            x: (float)(centerX - (labelWidth / 2) + painter.ImageLeftMargin),
            y: (float)(centerY - (border.Height / 2.0f) + painter.ImageTopMargin)
        );
        Rectangle imageBackground = new Rectangle(
            location: borderCorner,
            size: new Size(width: border.Width, height: painter.HeadingBackgroundHeight)
        );

        var secondaryImagePoint = new PointF(
            x: imagePoint.X - nodeData.SecondaryImage?.Width ?? 0 - imageGap,
            y: imagePoint.Y
        );
        var (emptyMessagePoint, emptyGraphMessage) = GetEmptyNodeMessage(node: node);

        editorGraphics.DrawUpSideDown(
            drawAction: graphics =>
            {
                graphics.FillRectangle(brush: painter.LightGreyBrush, rect: imageBackground);
                graphics.DrawString(
                    s: node.LabelText,
                    font: painter.Font,
                    brush: painter.GetTextBrush(isFromActivePackage: nodeData.IsFromActivePackage),
                    point: labelPoint,
                    format: painter.DrawFormat
                );
                if (!string.IsNullOrWhiteSpace(value: emptyGraphMessage))
                {
                    graphics.DrawString(
                        s: emptyGraphMessage,
                        font: painter.Font,
                        brush: painter.BlackBrush,
                        point: emptyMessagePoint,
                        format: painter.DrawFormat
                    );
                }
                graphics.DrawRectangle(pen: painter.GetActiveBorderPen(node: node), rect: border);
                graphics.DrawImage(image: nodeData.PrimaryImage, point: imagePoint);
                if (nodeData.SecondaryImage != null)
                {
                    graphics.DrawImage(image: nodeData.SecondaryImage, point: secondaryImagePoint);
                }
            },
            yAxisCoordinate: (float)node.GeometryNode.Center.Y
        );
        return true;
    }

    private Tuple<PointF, string> GetEmptyNodeMessage(Node node)
    {
        double centerX = node.GeometryNode.Center.X;
        double centerY = node.GeometryNode.Center.Y;

        if (!((BlockSubGraph)node).IsEmpty)
        {
            return new Tuple<PointF, string>(item1: new PointF(), item2: "");
        }
        string emptyGraphMessage = Strings.SubgraphPainter_Right_click_to_add_steps;
        SizeF messageSize = painter.MeasureString(nodeLabelText: emptyGraphMessage);
        var emptyMessagePoint = new PointF(
            x: (float)centerX - (messageSize.Width / 2),
            y: (float)centerY + (painter.HeadingBackgroundHeight / 2) - (messageSize.Height / 2)
        );

        return new Tuple<PointF, string>(item1: emptyMessagePoint, item2: emptyGraphMessage);
    }
}
