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

internal class BalloonPainter : INodeItemPainter
{
    private readonly InternalPainter painter;
    private readonly SolidBrush balloonBrush;
    private readonly int balloonRadius = 20;

    public BalloonPainter(InternalPainter painter, SolidBrush balloonBrush)
    {
        this.painter = painter;
        this.balloonBrush = balloonBrush;
    }

    public ICurve GetBoundary(Node node)
    {
        return new Ellipse(
            axisA: balloonRadius,
            axisB: balloonRadius,
            center: new Point(xCoordinate: 0, yCoordinate: 0)
        );
    }

    public bool Draw(Node node, object graphicsObj)
    {
        Graphics editorGraphics = (Graphics)graphicsObj;

        SizeF stringSize = editorGraphics.MeasureString(text: node.LabelText, font: painter.Font);

        var labelPoint = new PointF(
            x: (float)node.GeometryNode.Center.X - (stringSize.Width / 2),
            y: (float)node.GeometryNode.Center.Y - ((int)stringSize.Height / 2)
        );
        var boundingRectangle = new Rectangle(
            x: (int)node.BoundingBox.LeftBottom.X,
            y: (int)node.BoundingBox.LeftBottom.Y,
            width: (int)node.BoundingBox.Size.Width,
            height: (int)node.BoundingBox.Size.Height
        );

        editorGraphics.DrawUpSideDown(
            drawAction: graphics =>
            {
                graphics.FillEllipse(brush: balloonBrush, rect: boundingRectangle);
                graphics.DrawString(
                    s: node.LabelText,
                    font: painter.Font,
                    brush: painter.WhiteBrush,
                    point: labelPoint,
                    format: painter.DrawFormat
                );
                graphics.DrawEllipse(pen: painter.BlackPen, rect: boundingRectangle);
            },
            yAxisCoordinate: (float)node.GeometryNode.Center.Y
        );
        return true;
    }
}
