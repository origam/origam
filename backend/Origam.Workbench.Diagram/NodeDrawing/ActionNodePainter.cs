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
using System.Linq;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Origam.Extensions;
using Origam.Workbench.Diagram.Extensions;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram.NodeDrawing;

class ActionNodePainter : INodeItemPainter
{
    private readonly InternalPainter painter;
    private readonly int preferedTextWidth = 35;
    private readonly int imageSize = 12;
    private readonly int textSideMargin = 3;
    private readonly int imageTopMargin = 10;
    private readonly int imageTextGap = 8;
    private readonly int textBottomMargin = 5;

    public ActionNodePainter(InternalPainter internalPainter)
    {
        painter = internalPainter;
    }

    public ICurve GetBoundary(Node node)
    {
        INodeData nodeData = (INodeData)node.UserData;
        var borderSize = CalculateBorder(node: node);
        return CurveFactory.CreateRectangle(
            width: borderSize.Width + nodeData.LeftMargin,
            height: borderSize.Height,
            center: new Point()
        );
    }

    private Size CalculateBorder(Node node)
    {
        var nodeData = (INodeData)node.UserData;
        int actualTextWidth = GetTextLines(nodeData: nodeData)
            .Select(selector: line => line.Width(font: painter.Font))
            .Max();
        int totalWidth = textSideMargin + actualTextWidth + textSideMargin;
        int totalHeight =
            imageTopMargin
            + imageSize
            + imageTextGap
            + (painter.Font.Height * 2)
            + textBottomMargin;
        return new Size(width: totalWidth, height: totalHeight);
    }

    private Tuple<int, int> CalculateLabelPointOffsets(int[] lineWidths)
    {
        int width0 = 0;
        int width1 = 0;
        if (lineWidths.Length > 0)
        {
            width0 = lineWidths[0];
        }
        if (lineWidths.Length > 1)
        {
            width1 = lineWidths[1];
        }
        int widthDifference = Math.Abs(value: width0 - width1);
        int offset = widthDifference / 2;
        return width0 > width1
            ? new Tuple<int, int>(item1: 0, item2: offset)
            : new Tuple<int, int>(item1: offset, item2: 0);
    }

    public bool Draw(Node node, object graphicsObj)
    {
        INodeData nodeData = (INodeData)node.UserData;
        Graphics editorGraphics = (Graphics)graphicsObj;
        var image = nodeData.PrimaryImage;
        var borderSize = CalculateBorder(node: node);
        var borderCorner = new System.Drawing.Point(
            x: (int)node.GeometryNode.Center.X - (borderSize.Width / 2),
            y: (int)node.GeometryNode.Center.Y - (borderSize.Height / 2)
        );
        Rectangle border = new Rectangle(location: borderCorner, size: borderSize);

        var imagePoint = new PointF(
            x: (float)(node.GeometryNode.Center.X - ((float)image.Width / 2)),
            y: borderCorner.Y + imageTopMargin
        );

        var lines = GetTextLines(nodeData: nodeData);
        var lineWidths = lines.Select(selector: line => line.Width(font: painter.Font)).ToArray();
        int actualTextWidth = lineWidths.Max();
        var (label1XOffset, label2XOffset) = CalculateLabelPointOffsets(lineWidths: lineWidths);
        float textXCoordinate = (float)node.GeometryNode.Center.X - ((float)actualTextWidth / 2);
        PointF line1LabelPoint = new PointF(
            x: textXCoordinate + label1XOffset,
            y: (float)borderCorner.Y + imageTopMargin + image.Height + imageTextGap
        );

        PointF line2LabelPoint = new PointF(
            x: textXCoordinate + label2XOffset,
            y: line1LabelPoint.Y + painter.Font.Height
        );

        editorGraphics.DrawUpSideDown(
            drawAction: graphics =>
            {
                graphics.FillRectangle(brush: painter.LightGreyBrush, rect: border);
                graphics.DrawString(
                    s: lines[0],
                    font: painter.Font,
                    brush: painter.GetTextBrush(isFromActivePackage: nodeData.IsFromActivePackage),
                    point: line1LabelPoint,
                    format: painter.DrawFormat
                );
                if (lines.Length > 1)
                {
                    graphics.DrawString(
                        s: lines[1],
                        font: painter.Font,
                        brush: painter.GetTextBrush(
                            isFromActivePackage: nodeData.IsFromActivePackage
                        ),
                        point: line2LabelPoint,
                        format: painter.DrawFormat
                    );
                }
                graphics.DrawImage(image: image, point: imagePoint);
                if (Equals(objA: painter.NodeSelector.Selected, objB: node))
                {
                    graphics.DrawRectangle(
                        pen: painter.GetActiveBorderPen(node: node),
                        rect: border
                    );
                }
            },
            yAxisCoordinate: (float)node.GeometryNode.Center.Y
        );
        return true;
    }

    private string[] GetTextLines(INodeData nodeData)
    {
        return nodeData
            .Text.Wrap(widthInPixels: preferedTextWidth, font: painter.Font)
            .Split(splitWith: "\n")
            .Select(selector: x => x.Trim())
            .ToArray();
    }
}
