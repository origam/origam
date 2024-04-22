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

class NodeItemPainter : INodeItemPainter
{
    private readonly InternalPainter painter;

    public NodeItemPainter(InternalPainter internalPainter)
    {
            painter = internalPainter;
        }

    public ICurve GetBoundary(Node node)
    {
            INodeData nodeData = (INodeData) node.UserData;
            var borderSize = painter.CalculateMinHeaderBorder(node);
            return CurveFactory.CreateRectangle(borderSize.Width + nodeData.LeftMargin,
                borderSize.Height, new Point());
        }

    public bool Draw(Node node, object graphicsObj)
    {
            INodeData nodeData = (INodeData) node.UserData;
            Graphics editorGraphics = (Graphics) graphicsObj;
            var image =  nodeData.PrimaryImage;
            
            SizeF stringSize =
                editorGraphics.MeasureString(node.LabelText, painter.Font);

            var borderSize = painter.CalculateMinHeaderBorder(node);
            var borderCorner = new System.Drawing.Point(
                (int) node.GeometryNode.Center.X - borderSize.Width / 2,
                (int) node.GeometryNode.Center.Y - borderSize.Height / 2);
            Rectangle border = new Rectangle(borderCorner, borderSize);

            int labelOffsetDueToImageWidth = image == null 
                ? 0 
                : painter.NodeHeaderHeight;
            var labelPoint = new PointF(
                (float) node.GeometryNode.Center.X - (float) border.Width / 2 +
                labelOffsetDueToImageWidth +  nodeData.LeftMargin,
                (float) node.GeometryNode.Center.Y -
                (int) stringSize.Height / 2);

            var imageHorizontalBorder =
                (painter.NodeHeaderHeight - image?.Width ?? 0) / 2;
            var imageVerticalBorder =
                (painter.NodeHeaderHeight - image?.Height?? 0) / 2;
            var imagePoint = new PointF(
                (float) (node.GeometryNode.Center.X - (float) border.Width / 2 +
                         imageHorizontalBorder) +  nodeData.LeftMargin,
                (float) (node.GeometryNode.Center.Y -
                         (float) border.Height / 2 + imageVerticalBorder));

            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.DrawString(node.LabelText, painter.Font,
                        painter.GetTextBrush(nodeData.IsFromActivePackage),
                        labelPoint, painter.DrawFormat);
                    if (image != null)
                    {
                        graphics.DrawImage(image, imagePoint);
                    }
                    if (Equals(painter.NodeSelector.Selected, node))
                    {
                        graphics.DrawRectangle(painter.GetActiveBorderPen(node), border);
                    }
                },
                yAxisCoordinate: (float) node.GeometryNode.Center.Y);

            return true;
        }
}