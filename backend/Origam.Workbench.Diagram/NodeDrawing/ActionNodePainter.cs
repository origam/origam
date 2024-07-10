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
class ActionNodePainter: INodeItemPainter
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
        INodeData nodeData = (INodeData) node.UserData;
        var borderSize = CalculateBorder(node);
        return CurveFactory.CreateRectangle(borderSize.Width + nodeData.LeftMargin,
            borderSize.Height, new Point());
    }
    
    private Size CalculateBorder(Node node)
    {
        var nodeData = (INodeData)node.UserData;
        int actualTextWidth = GetTextLines(nodeData)
            .Select(line=>line.Width(painter.Font))
            .Max();
        int totalWidth = textSideMargin + actualTextWidth + textSideMargin;
        int totalHeight = imageTopMargin + imageSize +
                          imageTextGap + painter.Font.Height * 2 +
                          textBottomMargin;
        return new Size(totalWidth, totalHeight);
    }
    private Tuple<int,int> CalculateLabelPointOffsets(int[] lineWidths)
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
        int widthDifference = Math.Abs(width0 - width1);
        int offset = widthDifference / 2;
        return width0 > width1
            ? new Tuple<int, int>(0, offset) 
            : new Tuple<int, int>(offset, 0);
    }
    public bool Draw(Node node, object graphicsObj)
    {
        INodeData nodeData = (INodeData) node.UserData;
        Graphics editorGraphics = (Graphics) graphicsObj;
        var image = nodeData.PrimaryImage;
        var borderSize = CalculateBorder(node);
        var borderCorner = new System.Drawing.Point(
            (int) node.GeometryNode.Center.X - borderSize.Width / 2,
            (int) node.GeometryNode.Center.Y - borderSize.Height / 2);
        Rectangle border = new Rectangle(borderCorner, borderSize);
        
        var imagePoint = new PointF(
            (float) (node.GeometryNode.Center.X - (float) image.Width / 2),
                borderCorner.Y+imageTopMargin);
        
        var lines = GetTextLines(nodeData);
        var lineWidths = lines
            .Select(line => line.Width(painter.Font))
            .ToArray();
        int actualTextWidth = lineWidths.Max();
        var (label1XOffset, label2XOffset) = CalculateLabelPointOffsets(lineWidths);
        float textXCoordinate = (float) node.GeometryNode.Center.X - (float) actualTextWidth / 2;
        PointF line1LabelPoint = new PointF(
            textXCoordinate + label1XOffset,
            (float) borderCorner.Y + imageTopMargin + image.Height+ imageTextGap);

        PointF line2LabelPoint = new PointF(
            textXCoordinate + label2XOffset, 
            line1LabelPoint.Y+painter.Font.Height);
        
        editorGraphics.DrawUpSideDown(drawAction: graphics =>
            {
                graphics.FillRectangle(painter.LightGreyBrush, border);
                graphics.DrawString(lines[0], painter.Font,
                    painter.GetTextBrush(nodeData.IsFromActivePackage),
                    line1LabelPoint, painter.DrawFormat);
                if (lines.Length > 1)
                {
                    graphics.DrawString(lines[1], painter.Font,
                        painter.GetTextBrush(nodeData.IsFromActivePackage),
                        line2LabelPoint, painter.DrawFormat);
                }
                graphics.DrawImage(image, imagePoint);
                if (Equals(painter.NodeSelector.Selected, node))
                {
                    graphics.DrawRectangle(painter.GetActiveBorderPen(node), border);
                }
            },
            yAxisCoordinate: (float) node.GeometryNode.Center.Y);
        return true;
    }
    private string[] GetTextLines(INodeData nodeData)
    {
        return nodeData.Text
            .Wrap(preferedTextWidth, painter.Font)
            .Split("\n")
            .Select(x => x.Trim())
            .ToArray();
    }
}
